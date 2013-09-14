using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Admo.classes;
using Admo.classes.lib;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit.FaceTracking;
using NLog;
using Timer = System.Timers.Timer;

namespace Admo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Logger Log = LogManager.GetCurrentClassLogger();
        //kinect toolkit variables
        private KinectSensorChooser _sensorChooser;
        //kinect variables
        private bool _closing = false;
        const int SkeletonCount = 6;
        Skeleton[] allSkeletons = new Skeleton[SkeletonCount];
        Skeleton _oldFirst;
        private KinectSensor _currentKinectSensor;
        private String _kinectType;
        public static int KinectElevationAngle = 0;  //used in application handler
   
        //drawing variables
        private int _faceX = 700;
        private int _faceY = 1000;
       


        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap _colorBitmap;

        //face tracking variables
        private FaceTracker _faceTracker;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] _colorImage;
        public short[] DepthImage;

        //variable indicating whether user is looking at the screen
        private bool LookingAtScreen = false; //currently not used but keeping cause it might used
        public static readonly LifeCycle LifeCycle=new LifeCycle();
   
        private ColorImageFormat _colorImageFormat = ColorImageFormat.Undefined;
        private DepthImageFormat _depthImageFormat = DepthImageFormat.Undefined;

        public static KinectLib KinectLib = new KinectLib(); //used in application handler as static

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public void OnConfigChange()
        {
            KinectElevationAngle = Config.GetElevationAngle();
            if (_currentKinectSensor != null) _currentKinectSensor.ElevationAngle = KinectElevationAngle;

            if (!Boolean.Parse(Config.ReadConfigOption(Config.Keys.CalibrationActive))) return;
            //set calibration values to zero in preparation for calibration
            Application_Handler.fov_top = 0;
            Application_Handler.fov_left = 0;
            Application_Handler.fov_width = 640;
            Application_Handler.fov_height = 480;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Config.Init();
            Config.OptionChanged += OnConfigChange;
            SocketServer.StartServer();
            LifeCycle.ActivateTimers();

            Application_Handler.ConfigureCalibrationByConfig();
  
            //start and stop old kinect sensor kinect sensor
            KinectSensor sensor1 = KinectSensor.KinectSensors[0];
            sensor1.Stop();

            // initialize the sensor chooser and UI
            _sensorChooser = new KinectSensorChooser();
            _sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            
            sensorChooserUi.KinectSensorChooser = _sensorChooser;
            _sensorChooser.Start();
            

            if (!Config.IsDevMode())
            {
                //Minimize the window so that the chrome window is always infront.
                WindowState = (WindowState) FormWindowState.Minimized;
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

            //Kinect filter parameters
            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 0.02f,        
                MaxDeviationRadius = 0.04f   
            };

            bool error = false;
            
            if (args.OldSensor != null)
            {
                try
                {
                    Log.Warn("old sensor active");
                    //args.OldSensor.DepthStream.Range = DepthRange.Default;
                    //args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                    error = true;
                }
            }       
            else //if (args.NewSensor != null)
            {           
                try
                {


                    //set depthstream and skeletal tracking options
                    args.NewSensor.DepthStream.Range = DepthRange.Default;
                    args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    args.NewSensor.SkeletonStream.Enable(parameters);

                    args.NewSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(SensorAllFramesReady);
                   
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    //only enable RGB camera if facetracking or dev-mode is enabled
                    if (Config.RunningFacetracking || Config.IsDevMode())
                    {
                        args.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                        this._colorImage = new byte[args.NewSensor.ColorStream.FramePixelDataLength];
                    }                       
                       
                    //setup short containing depth data
                    this.DepthImage = new short[args.NewSensor.DepthStream.FramePixelDataLength];
                    // This is the bitmap we'll display on-screen
                    this._colorBitmap = new WriteableBitmap(args.NewSensor.ColorStream.FrameWidth, args.NewSensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                    
                    if (Config.IsDevMode())
                    {                       
                        // Set the image we display to point to the bitmap where we'll put the image data
                        this.Image.Source = this._colorBitmap;
                    }
                    else
                    {
                        
                        var bi3 = new BitmapImage();
                        bi3.BeginInit();
                        bi3.UriSource = new Uri("images/background.png", UriKind.Relative);
                        bi3.EndInit();
                        this.Image.Stretch = Stretch.Uniform;
                        this.Image.Source = bi3;
                    }
                    
                    _currentKinectSensor = args.NewSensor;
                    
                    try
                    {
                        args.NewSensor.Start();
                        args.NewSensor.ElevationAngle = KinectElevationAngle = Config.GetElevationAngle();
                    }
                    catch (System.IO.IOException ioe)
                    {
                        Log.Error(ioe);
                        //this.sensorChooser.AppConflictOccurred();
                    }

                    //check whether we are using Kinect for Windows or Xbox Kinect
                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                        _kinectType = "kinect for windows";
                        Log.Info("Using " + _kinectType);
                    }
                    catch (InvalidOperationException)
                    {
                        _kinectType = "xbox kinect";
                        Console.WriteLine(_kinectType);
                        Log.Info("Using " + _kinectType);
                    } 
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

        public void Dispose()
        {
            DestroyFaceTracker();
        }

        private void DestroyFaceTracker()
        {
            if (_faceTracker == null) return;
            _faceTracker.Dispose();
            _faceTracker = null;
        }

        void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (_closing) return;

            ColorImageFrame colorFrame = null;
            DepthImageFrame depthFrame = null;
            SkeletonFrame skeletonFrameData = null;

            try
            {

                if (Config.IsDevMode()|| Config.RunningFacetracking)
                {
                    colorFrame = e.OpenColorImageFrame();
                    depthFrame = e.OpenDepthImageFrame();
                    skeletonFrameData = e.OpenSkeletonFrame();

                    if (colorFrame == null || depthFrame == null || skeletonFrameData == null)
                    {
                        return;
                    }

                    if (Config.RunningFacetracking)
                    {
                        // Check for changes in any of the data this function is receiving
                        // and reset things appropriately.
                        if (this._depthImageFormat != depthFrame.Format)
                        {
                            this.DestroyFaceTracker();
                            //this.depthImage = null;
                            this._depthImageFormat = depthFrame.Format;
                        }

                        if (this._colorImageFormat != colorFrame.Format)
                        {
                            this.DestroyFaceTracker();
                            //this.colorImage = null;
                            this._colorImageFormat = colorFrame.Format;
                        }
                    }
                }
                else
                {
                    depthFrame = e.OpenDepthImageFrame();
                    skeletonFrameData = e.OpenSkeletonFrame();

                    if (depthFrame == null || skeletonFrameData == null)
                    {
                        return;
                    }
                }

                //LifeCycle.LifeLoop();

                if ((colorFrame != null)&&((Config.RunningFacetracking)|| Config.IsDevMode()))
                {
                    DisplayVideo(colorFrame);
                }


                //Get a skeleton
                if (skeletonFrameData != null)
                {
                    //get closest skeleton
                    skeletonFrameData.CopySkeletonDataTo(allSkeletons);
                    Skeleton first = KinectLib.GetPrimarySkeleton(allSkeletons);

                    //check whether there is a user/skeleton
                    if (first == null)
                    {

                        //check whether there is a user in fov who's skeleton has not yet been registered
                        Application_Handler.FindPlayer(depthFrame);

                        //set detection variable
                        Application_Handler.detected = false;

                        return;
                    }
                    else
                    {

                        //get joint coordinates
                        float[] coordinates = KinectLib.GetCoordinates(first);

                        //Map the skeletal coordinates to the video map
                        MapSkeletonToVideo(first, depthFrame, coordinates);

                        //Handles gestures - eg Swipe gesture
                        Application_Handler.ManageGestures(coordinates);

                        //Managing data send to Node                 
                        Application_Handler.Manage_Skeletal_Data(coordinates, first);
                 
                        
                        if ((coordinates[19] > 0.9)&&(Config.RunningFacetracking))
                        {
                            if (_oldFirst == null)
                            {
                                _oldFirst = first;
                                Log.Debug("first skeleton lock");
                            }
                            else if ((_oldFirst != first))
                            {
                                Console.WriteLine("locked skeleton changed");
                                _oldFirst = first;
                                if ((this._faceTracker != null))
                                {
                                    this._faceTracker.ResetTracking();
                                }
                            }

                            if (this._faceTracker == null)
                            {
                                try
                                {
                                    this._faceTracker = new FaceTracker(_currentKinectSensor);
                                    Console.WriteLine("initiate new FaceTracker");
                                }
                                catch (InvalidOperationException)
                                {
                                    // During some shutdown scenarios the FaceTracker
                                    // is unable to be instantiated.  Catch that exception
                                    // and don't track a face.                            
                                    this._faceTracker = null;
                                }
                            }

                            if (this._faceTracker != null)
                            {
                                FaceTrackFrame faceTrackFrame = this._faceTracker.Track(
                                    _colorImageFormat,
                                    this._colorImage,
                                    _depthImageFormat,
                                    this.DepthImage,
                                    first);


                                if (faceTrackFrame.TrackSuccessful)
                                {                                    
                                    _faceX = (int)(100 * faceTrackFrame.Rotation.X);

                                    //gets the x,y-coordinate of where the user is looking at the screen
                                    _faceX = Convert.ToInt32((-700 * coordinates[19] * Math.Tan(faceTrackFrame.Rotation.X * Math.PI / 180)) + (coordinates[6] * 1000));
                                    _faceY = Convert.ToInt32((-1000 * coordinates[19] * Math.Tan(faceTrackFrame.Rotation.Y * Math.PI / 180)) + (coordinates[6] * 1000));

                                    //set limits
                                    if (_faceX > 700)
                                        _faceX = 700;
                                    else if (_faceX < -700)
                                        _faceX = -700;

                                    if (_faceY > 1000)
                                        _faceY = 1000;
                                    else if (_faceY < -1000)
                                        _faceY = -1000;

                                    //set variable indicating whether user is looking at the screen (where the screen 2000mmx1700mm)
                                    if ((_faceY < 1000) && (_faceY > -1000))
                                        LookingAtScreen = true;
                                    else
                                        LookingAtScreen = false;

                                    //display ellipse indicating where useris looking
                                    if (Config.IsDevMode())
                                    {
                                        Canvas.SetTop(faceEllipse, ((240 - _faceX / 10) - faceEllipse.Height / 2));
                                        Canvas.SetLeft(faceEllipse, ((320 + _faceY/2) - faceEllipse.Width / 2));
                                    }

                                    //set x,y-coordinates relative to screen size canvas
                                    _faceX = _faceX + 700;
                                    _faceY = _faceY + 1000;


                                    
                                }
                            }

                        }
                        
                    }

                }
            }
            finally
            {
                if (colorFrame != null)
                {
                    colorFrame.Dispose();
                }

                if (depthFrame != null)
                {
                    depthFrame.Dispose();
                }

                if (skeletonFrameData != null)
                {
                    skeletonFrameData.Dispose();
                }
            }
                     

        }

        void DisplayVideo(ColorImageFrame colorFrame)
        {
            //color frame handlers
            if (colorFrame == null) return;
            // Copy the pixel data from the image to a temporary array
            colorFrame.CopyPixelDataTo(this._colorImage);

            if (Config.IsDevMode())
            {
                // Write the pixel data into our bitmap
                this._colorBitmap.WritePixels(
                    new Int32Rect(0, 0, this._colorBitmap.PixelWidth, this._colorBitmap.PixelHeight),
                    this._colorImage,
                    this._colorBitmap.PixelWidth * sizeof(int),
                    0);
            }
        }

        //overlaying IR camera and RGB camera video feeds
        void MapSkeletonToVideo(Skeleton first, DepthImageFrame depth, float[] coord)
        {
            
                if (depth == null || _sensorChooser.Kinect == null)
                {
                    return;
                }
                depth.CopyPixelDataTo(DepthImage);

                CoordinateMapper cm = new CoordinateMapper(_currentKinectSensor);
                //Map a skeletal point to a point on the color image 
                ColorImagePoint headColorPoint = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.Head].Position, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftColorPoint = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.HandLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightColorPoint = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.HandRight].Position, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftElbowColorPoint = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.ElbowLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);    
                ColorImagePoint rightElbowColorPoint = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.ElbowRight].Position, ColorImageFormat.RgbResolution640x480Fps30);

                int[] depth_coord = Image_Processing.ProcessHands(DepthImage);
                DepthImagePoint new_hand = new DepthImagePoint();
                new_hand.X = depth_coord[0];
                new_hand.Y = depth_coord[1];
                new_hand.Depth = depth_coord[2];
                DepthImagePoint new_hand2 = new DepthImagePoint();
                new_hand2.X = depth_coord[3];
                new_hand2.Y = depth_coord[4];
                new_hand2.Depth = depth_coord[5];
                ColorImagePoint depth_hand = cm.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, new_hand, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint depth_hand2 = cm.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, new_hand2, ColorImageFormat.RgbResolution640x480Fps30);

                //get depth coordinates of head and hands relative to the body
                double relative_z_head = Application_Handler.RelativeCoordinates(coord[7], coord[19]);
                double relative_z_lefthand = Application_Handler.RelativeCoordinates(coord[1], coord[14]);
                double relative_z_righthand = Application_Handler.RelativeCoordinates(coord[3], coord[15]);
                //Console.WriteLine(relative_z_righthand + " .. " + coord[15] + " ....... " + relative_z_head + " .. " + coord[19]);

                int hand_selection = Application_Handler.Select_Hand(relative_z_head, relative_z_righthand, relative_z_lefthand);

                if (hand_selection == 1)
                {
                    Application_Handler.UncalibratedCoordinates[4] = Application_Handler.stick_coord[4] = rightColorPoint.X;
                    Application_Handler.UncalibratedCoordinates[5] = Application_Handler.stick_coord[5] = rightColorPoint.Y;
                    Coordinate_History.x_filter.Clear();
                    Coordinate_History.y_filter.Clear();
                    Coordinate_History.previous_x = rightColorPoint.X;
                    Coordinate_History.previous_y = rightColorPoint.Y;
                }
                else
                {
                    Coordinate_History.old_x = rightColorPoint.X;
                    Coordinate_History.old_y = rightColorPoint.Y;
                    Application_Handler.UncalibratedCoordinates[4] = Application_Handler.stick_coord[4] = depth_hand.X;
                    Application_Handler.UncalibratedCoordinates[5] = Application_Handler.stick_coord[5] = depth_hand.Y;
                    Coordinate_History.FilterCoordinates(Application_Handler.stick_coord[4], Application_Handler.stick_coord[5]);
                }

                Application_Handler.UncalibratedCoordinates[0] = Application_Handler.stick_coord[0] = headColorPoint.X;
                Application_Handler.UncalibratedCoordinates[1] = Application_Handler.stick_coord[1] = headColorPoint.Y;

                Application_Handler.UncalibratedCoordinates[2] = Application_Handler.stick_coord[2] = leftColorPoint.X;
                Application_Handler.UncalibratedCoordinates[3] = Application_Handler.stick_coord[3] = leftColorPoint.Y;

                //elbows don't need to be passed to uncalibrated coordinate set
                Application_Handler.stick_coord[6] = leftElbowColorPoint.X;
                Application_Handler.stick_coord[7] = leftElbowColorPoint.Y;
                Application_Handler.stick_coord[8] = rightElbowColorPoint.X;
                Application_Handler.stick_coord[9] = rightElbowColorPoint.Y;

                //only show video HUD when running in dev mode
                if (Config.IsDevMode())
                {
                    Animations(first, headColorPoint, leftColorPoint, rightColorPoint, depth_hand, depth_hand2, coord, depth_coord, hand_selection);
                }
            

        }

        //delete if in production
        public void Animations(Skeleton first, ColorImagePoint headColorPoint, ColorImagePoint leftColorPoint, ColorImagePoint rightColorPoint, ColorImagePoint depth_hand, ColorImagePoint depth_hand2, float[] coord, int[] depth_coord, int hand_selection)
        {
            var leftHand = first.Joints[JointType.HandLeft];
            var rightHand = first.Joints[JointType.HandRight];
            if (leftHand.TrackingState == JointTrackingState.Tracked)
            {
                leftEllipse.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
            }
            else
            {
                leftEllipse.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 255));
            }

            if (rightHand.TrackingState == JointTrackingState.Tracked)
            {
                rightEllipse.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
            }
            else
            {
                rightEllipse.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 255));
            }
            //Set location
            CameraPosition(head_ellipse, headColorPoint);
            CameraPosition(leftEllipse, leftColorPoint);
            CameraPosition(rightEllipse, rightColorPoint);

            CameraPosition(depthEllipse, depth_hand);
            CameraPosition(depth2Ellipse, depth_hand2);

            if (hand_selection == 2)
                CameraPosition(realEllipse, depth_hand);


            depth_rectangle.Width = depth_rectangle.Height = Image_Processing.depth_size;

            Canvas.SetTop(depth_rectangle, (depth_hand.Y - depth_rectangle.Height / 2));
            Canvas.SetLeft(depth_rectangle, (depth_hand.X - depth_rectangle.Width / 2));

            crop_rectangle.Width = Application_Handler.fov_width;
            crop_rectangle.Height = Application_Handler.fov_height;

            Canvas.SetTop(crop_rectangle, Application_Handler.fov_top);
            Canvas.SetLeft(crop_rectangle, Application_Handler.fov_left);
            
        }

        //set element position on canvas
        private void CameraPosition(FrameworkElement element, ColorImagePoint point)
        {
            //Divide by 2 for width and height so point is right in the middle 
            // instead of in top/left corner
            Canvas.SetLeft(element, point.X  - element.Width / 2);
            Canvas.SetTop(element, point.Y  - element.Height / 2);
        }


        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            KinectLib.StopKinectSensor(_sensorChooser.Kinect);
            SocketServer.Stop();
            Log.Info("Shutting down server");
            _closing = true; 
        }
    }
}
