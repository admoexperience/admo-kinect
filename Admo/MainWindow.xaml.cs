using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Admo.classes;
using Admo.classes.lib;
using Admo.Utilities;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;

using Microsoft.Kinect.Toolkit.BackgroundRemoval;
using System.Windows.Forms;
using NLog;

namespace Admo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        //kinect toolkit variables
        private KinectSensorChooser _sensorChooser;
        //kinect variables
        private bool _closing;
        private const int SkeletonCount = 6;
        private readonly Skeleton[] allSkeletons = new Skeleton[SkeletonCount];
        private KinectSensor _currentKinectSensor;
        public static int KinectElevationAngle = 0; //used in application handler

        public static bool SilhouetteEnabled = true;

        //drawing variables

        /// <summary>
        ///     Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap _colorBitmap;

        //background removal stream
        private BackgroundRemovedColorStream backgroundRemovedColorStream;
        //face tracking variables

        /// <summary>
        ///     Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] _colorImage;

        public short[] DepthImage;

        //variable indicating whether user is looking at the screen
        public static readonly LifeCycle LifeCycle = new LifeCycle();

        public static KinectLib KinectLib = new KinectLib(); //used in application handler as static
        private readonly GestureDetection _gestureDetectionRight = new GestureDetection();
        private readonly GestureDetection _gestureDetectionLeft = new GestureDetection();

        private ApplicationHandler _applicationHandler;
        private double _angleChangeTime = Utils.GetCurrentTimeInSeconds();
        private WebServer _webServer;

        private ShortCutHandler keyHandler;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            this.Closed += WindowClosing;
        }


        public void OnConfigChange()
        {
            KinectElevationAngle = Config.GetElevationAngle();

            if (_currentKinectSensor != null && _currentKinectSensor.IsRunning)
            {
                //Required because kinect angle can only be changed once per second
                //Ignore resharper

                if (_currentKinectSensor.ElevationAngle != KinectElevationAngle &&
                    (_angleChangeTime - Utils.GetCurrentTimeInSeconds()) > 1)
                {
                    _angleChangeTime = Utils.GetCurrentTimeInSeconds();
                    _currentKinectSensor.ElevationAngle = KinectElevationAngle;
                }
            }

            if (Config.IsCalibrationActive())
            {
                //set calibration values to zero in preparation for calibration
                TheHacks.FovTop = 0;
                TheHacks.FovLeft = 0;
                TheHacks.FovWidth = 640;
                TheHacks.FovHeight = 480;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            
            Config.Init();
            Config.OptionChanged += OnConfigChange;
            SocketServer.StartServer();
            LifeCycle.ActivateTimers();
            _applicationHandler = new ApplicationHandler();
            ApplicationHandler.ConfigureCalibrationByConfig();
            keyHandler = new ShortCutHandler(this);

            //start and stop old kinect sensor kinect sensor
            if (KinectSensor.KinectSensors.Count > 0)
            {
                var sensor1 = KinectSensor.KinectSensors[0];
                sensor1.Stop();
            }
            _webServer = new WebServer(Config.GetWebServerBasePath());
            _webServer.Start();

            // initialize the sensor chooser and UI
            _sensorChooser = new KinectSensorChooser();
            _sensorChooser.KinectChanged += SensorChooserOnKinectChanged;

            sensorChooserUi.KinectSensorChooser = _sensorChooser;
            _sensorChooser.Start();


            if (!Config.IsDevMode())
            {
                //Minimize the window so that the chrome window is always infront.
                WindowState = (WindowState)FormWindowState.Minimized;
            }
        }


        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            //get kinect sensor
            _currentKinectSensor = KinectSensor.KinectSensors[0];

            //stop any previous kinect session
            KinectLib.StopKinectSensor(_currentKinectSensor);
            if (_currentKinectSensor == null)
            {
                return;
            }



            var parameters = KinectLib.GetTransformSmoothParameters(Config.GetTransformSmoothType());
            var error = false;

            if (args.OldSensor != null)
            {
                try
                {
                    Log.Warn("old sensor active");
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();

                    // Create the background removal stream to process the data and remove background, and initialize it.
                    if (null != backgroundRemovedColorStream)
                    {
                        //backgroundRemovedColorStream.BackgroundRemovedFrameReady -= BackgroundRemovedFrameReadyHandler;
                        backgroundRemovedColorStream.Dispose();
                        backgroundRemovedColorStream = null;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                    Log.Error("Unable to disable sensor stream");
                }
            }
            else //if (args.NewSensor != null)
            {
                try
                {
                    backgroundRemovedColorStream = new BackgroundRemovedColorStream(args.NewSensor);
                    backgroundRemovedColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution640x480Fps30);
                    
                    //set depthstream and skeletal tracking options
                    args.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.DepthStream.Range = DepthRange.Default;
                    args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    args.NewSensor.SkeletonStream.Enable(parameters);

                    

                    backgroundRemovedColorStream.BackgroundRemovedFrameReady += BackgroundRemovedFrameReadyHandler;
                    
                    args.NewSensor.AllFramesReady += SensorAllFramesReady;

                    


                    //only enable RGB camera if facetracking or dev-mode is enabled
                    if (Config.RunningFacetracking || Config.IsDevMode())
                    {
                        //args.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                        _colorImage = new byte[args.NewSensor.ColorStream.FramePixelDataLength];
                    }

                    //setup short containing depth data
                    DepthImage = new short[args.NewSensor.DepthStream.FramePixelDataLength];
                    // This is the bitmap we'll display on-screen
                    _colorBitmap = new WriteableBitmap(args.NewSensor.ColorStream.FrameWidth,
                                                       args.NewSensor.ColorStream.FrameHeight, 96.0, 96.0,
                                                       PixelFormats.Bgr32, null);
                    
                    
                    
                    if (Config.IsDevMode())
                    {
                        // Set the image we display to point to the bitmap where we'll put the image data
                        Image.Source = _colorBitmap;
                    }
                    else
                    {
                        var bi3 = new BitmapImage();
                        bi3.BeginInit();
                        bi3.UriSource = new Uri("resources/background.png", UriKind.Relative);
                        bi3.EndInit();
                        Image.Stretch = Stretch.Uniform;
                        Image.Source = bi3;
                    }

                    _currentKinectSensor = args.NewSensor;

                    try
                    {
                        args.NewSensor.Start();
                        args.NewSensor.ElevationAngle = KinectElevationAngle = Config.GetElevationAngle();
                    }
                    catch (IOException ioe)
                    {
                        Log.Error(ioe);
                    }

                    args.NewSensor.DepthStream.Range = DepthRange.Near;
                    args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                }
                catch (InvalidOperationException)
                {
                    error = true;
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }

                if (!error)
                {
                    kinectRegion.KinectSensor = args.NewSensor;
                }
            }
        }

        private void BackgroundRemovedFrameReadyHandler(object sender, BackgroundRemovedColorFrameReadyEventArgs e)
        {

            if (_closing) return;

            using (var backgroundRemovedFrame = e.OpenBackgroundRemovedColorFrame())
            {
                if (backgroundRemovedFrame != null)
                {                    

                    byte[] backgroundImage = backgroundRemovedFrame.GetRawPixelData();
                    int width = backgroundRemovedFrame.Width;
                    int height = backgroundRemovedFrame.Height;

                    byte [] userOutlineArray = new byte[width * height];                    

                    int add = (width * 200 + width / 2) * 4;

                    int boundaries = 20;

                    for (int tel = 0; tel < (width * height * 4); tel = tel + 4)
                    {
                        int indexY = (int)((tel/4) / width);
                        int indexX = (int)((tel/4) % width);
                        /*
                        int indexX = (tel / 4) % width; 
                        int indexY = (tel / 4) / width;
                        */

                        if (backgroundImage[tel + 3] > 50)
                        {
                            //do not write user pixels if not in at the boundaries
                            if ((indexX < boundaries) || (indexX > (width - boundaries)) || (indexY < boundaries) || (indexY > (height - boundaries)))
                            {
                                userOutlineArray[tel / 4] = 0;
                            }
                            else
                            {
                                userOutlineArray[tel / 4] = 1;
                            }

                        }
                        else
                        {
                            //no user
                            userOutlineArray[tel / 4] = 0;
                        }

                    }

                    String userVector = _applicationHandler.GetUserOutline(userOutlineArray);
                    
                    SocketServer.SendImageFrame(userVector);
                }

                
            }
            
        }

        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (_closing) return;


            DepthImageFrame depthFrame = null;
            SkeletonFrame skeletonFrameData = null;
            ColorImageFrame colorFrame = null;

            try
            {
                depthFrame = e.OpenDepthImageFrame();
                skeletonFrameData = e.OpenSkeletonFrame();
                colorFrame = e.OpenColorImageFrame();

                if (Config.IsDevMode())
                {
                    if (colorFrame == null)
                    {
                        return;
                    }
                    else
                    {
                        //DisplayVideo(colorFrame);
                    }
                }

                if (depthFrame == null || skeletonFrameData == null || colorFrame == null)
                {
                    return;
                }

                //get closest skeleton
                skeletonFrameData.CopySkeletonDataTo(allSkeletons);
                var first = KinectLib.GetPrimarySkeleton(allSkeletons);

                var rawDepthData = new short[depthFrame.PixelDataLength];
                depthFrame.CopyPixelDataTo(rawDepthData);

                //check whether there is a user/skeleton
                if (first == null)
                {                   

                    //check whether there is a user in fov who's skeleton has not yet been registered

                    var kinectState = _applicationHandler.FindPlayer(rawDepthData, depthFrame.Height, depthFrame.Width);
                    SocketServer.SendKinectData(kinectState);
                    //set detection variable

                    _applicationHandler.Detected = false;
                }
                else
                {
                    //if silhouette is enable which means that the app is not using the webcam, then we need to use the backgroundremoval stream
                    if (SilhouetteEnabled)
                    {
                        backgroundRemovedColorStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                        backgroundRemovedColorStream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);
                        backgroundRemovedColorStream.ProcessSkeleton(allSkeletons, skeletonFrameData.Timestamp);
                        backgroundRemovedColorStream.SetTrackedPlayer(first.TrackingId);
                    }
                    GetDataForSocketServer(first);

                    //Map the skeletal coordinates to the video map
                    MapSkeletonToVideo(first);

                }
            }
            finally
            {
                if (depthFrame != null)
                {
                    depthFrame.Dispose();
                }

                if (skeletonFrameData != null)
                {
                    skeletonFrameData.Dispose();
                }

                if (colorFrame != null)
                {
                    colorFrame.Dispose();
                }
            }
        }

        private void GetDataForSocketServer(Skeleton first)
        {
            //swipe gesture detection
            var gestureRight =
                _gestureDetectionRight.DetectSwipe(new HandHead(
                                                          first.Joints[JointType.HandRight].Position.X,
                                                          first.Joints[JointType.HandRight].Position.Y,
                                                          first.Joints[JointType.Head].Position.X));
            if (gestureRight == GestureDetection.SwipeLeftGesture)
            {
                   SocketServer.SendGestureEvent(gestureRight);
            }

            var gestureLeft =
                _gestureDetectionLeft.DetectSwipe(new HandHead(first.Joints[JointType.HandLeft].Position.X,
                                                                  first.Joints[JointType.HandLeft].Position.Y,
                                                                  first.Joints[JointType.Head].Position.X));
            if (gestureLeft == GestureDetection.SwipeRightGesture)
            {
                SocketServer.SendGestureEvent(gestureLeft);
            }
            //Managing data send to Node                 
            _applicationHandler.Manage_Skeletal_Data(first, new CoordinateMapper(_currentKinectSensor));
        }

        private void DisplayVideo(ColorImageFrame colorFrame)
        {
            //color frame handlers
            if (colorFrame == null) return;
            // Copy the pixel data from the image to a temporary array
            colorFrame.CopyPixelDataTo(_colorImage);

            if (Config.IsDevMode())
            {
                // Write the pixel data into our bitmap
                _colorBitmap.WritePixels(
                    new Int32Rect(0, 0, _colorBitmap.PixelWidth, _colorBitmap.PixelHeight),
                    _colorImage,
                    _colorBitmap.PixelWidth * sizeof(int),
                    0);
            }
            colorFrame.Dispose();
        }

        //overlaying IR camera and RGB camera video feeds
        private void MapSkeletonToVideo(Skeleton first)
        {
            if (_sensorChooser.Kinect == null)
            {
                return;
            }

            var cm = new CoordinateMapper(_currentKinectSensor);
            //Map a skeletal point to a point on the color image 
            ColorImagePoint headColorPoint = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.Head].Position,
                                                                             ColorImageFormat.RgbResolution640x480Fps30);
            ColorImagePoint leftColorPoint = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.HandLeft].Position,
                                                                             ColorImageFormat.RgbResolution640x480Fps30);
            ColorImagePoint rightColorPoint = cm.MapSkeletonPointToColorPoint(
                first.Joints[JointType.HandRight].Position, ColorImageFormat.RgbResolution640x480Fps30);
            //get depth coordinates of head and hands relative to the body


            //only show video HUD when running in dev mode
            if (Config.IsDevMode())
            {
                Animations(first, headColorPoint, leftColorPoint, rightColorPoint);
            }
        }

        //delete if in production
        public void Animations(Skeleton first, ColorImagePoint headColorPoint, ColorImagePoint leftColorPoint,
                               ColorImagePoint rightColorPoint)
        {
            Joint leftHand = first.Joints[JointType.HandLeft];
            Joint rightHand = first.Joints[JointType.HandRight];
            if (leftHand.TrackingState == JointTrackingState.Tracked)
            {
                leftEllipse.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
            else
            {
                leftEllipse.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
            }

            if (rightHand.TrackingState == JointTrackingState.Tracked)
            {
                rightEllipse.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
            else
            {
                rightEllipse.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
            }
            //Set location
            CameraPosition(head_ellipse, headColorPoint);
            CameraPosition(leftEllipse, leftColorPoint);
            CameraPosition(rightEllipse, rightColorPoint);

            crop_rectangle.Width = TheHacks.FovWidth;
            crop_rectangle.Height = TheHacks.FovHeight;

            Canvas.SetTop(crop_rectangle, TheHacks.FovTop);
            Canvas.SetLeft(crop_rectangle, TheHacks.FovLeft);
        }


        //set element position on canvas
        private void CameraPosition(FrameworkElement element, ColorImagePoint point)
        {
            //Divide by 2 for width and height so point is right in the middle 
            // instead of in top/left corner
            Canvas.SetLeft(element, point.X - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);
        }

        private void WindowClosing(object sender, EventArgs e)
        {
            Log.Info("Shutting down server");
            if (_sensorChooser!= null &&_sensorChooser.Kinect != null)
            {
                KinectLib.StopKinectSensor(_sensorChooser.Kinect);
            }
            KinectLib.StopKinectSensor(_sensorChooser.Kinect);
            //need to stop backgroundremovedcolour stream - it not working...
            if (null != this.backgroundRemovedColorStream)
            {
                this.backgroundRemovedColorStream.Dispose();
                this.backgroundRemovedColorStream = null;
            }
           
            SocketServer.Stop();
           
            _closing = true;

            if (_webServer != null)
            {
                _webServer.Close();
            }
            System.Windows.Application.Current.Shutdown();

        }

        public long LastHitTime { get; set; }
    }
}
