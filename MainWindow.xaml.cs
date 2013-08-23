using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Admo.classes;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit.FaceTracking;
using NLog;

namespace Admo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Logger Log = LogManager.GetCurrentClassLogger();
        //kinect toolkit variables
        private KinectSensorChooser sensorChooser;
        //kinect variables
        public static bool closing = false;
        const int skeletonCount = 6;
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];
        Skeleton old_first;
        public static KinectSensor CurrentKinect;
        public static String kinect_type;
        public static int KinectElevationAngle = 0;
   
        //drawing variables
        public static int face_x = 700;
        public static int face_y = 1000;
       


        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        //face tracking variables
        private FaceTracker faceTracker;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorImage;
        public short[] depthImage;

        //variable indicating whether user is looking at the screen
        public static bool looking_at_screen = false;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public static void OnConfigChange()
        {
            KinectElevationAngle = Config.GetElevationAngle();
            CurrentKinect.ElevationAngle = KinectElevationAngle;

            if (Boolean.Parse(Config.ReadConfigOption(Config.Keys.CalibrationActive)))
            {
                //set calibration values to zero in preparation for calibration
                Application_Handler.fov_top = 0;
                Application_Handler.fov_left = 0;
                Application_Handler.fov_width = 640;
                Application_Handler.fov_height = 480;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Config.Init();
            Config.OptionChanged += OnConfigChange;
            SocketServer.StartServer();

            Application_Handler.ConfigureCalibrationByConfig();
  
            //start and stop old kinect sensor kinect sensor
            KinectSensor sensor1 = KinectSensor.KinectSensors[0];
            sensor1.Stop();

            // initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            if (!Config.IsDevMode())
            {
                //Minimize the window so that the chrome window is always infront.
                this.WindowState = (WindowState) FormWindowState.Minimized;
            }

        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {

            //get kinect sensor
            CurrentKinect = KinectSensor.KinectSensors[0];
            //stop any previous kinect session
            StopKinect(CurrentKinect);
            if (CurrentKinect == null)
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

                    args.NewSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);                    
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    //only enable RGB camera if facetracking or dev-mode is enabled
                    if (Config.RunningFacetracking || Config.IsDevMode())
                    {
                        args.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                        this.colorImage = new byte[args.NewSensor.ColorStream.FramePixelDataLength];
                    }                       
                       
                    //setup short containing depth data
                    this.depthImage = new short[args.NewSensor.DepthStream.FramePixelDataLength];
                    // This is the bitmap we'll display on-screen
                    this.colorBitmap = new WriteableBitmap(args.NewSensor.ColorStream.FrameWidth, args.NewSensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                    
                    if (Config.IsDevMode())
                    {                       
                        // Set the image we display to point to the bitmap where we'll put the image data
                        this.Image.Source = this.colorBitmap;
                    }
                    else
                    {
                        
                        BitmapImage bi3 = new BitmapImage();
                        bi3.BeginInit();
                        bi3.UriSource = new Uri("images/background.png", UriKind.Relative);
                        bi3.EndInit();
                        this.Image.Stretch = Stretch.Uniform;
                        this.Image.Source = bi3;
                    }
                    
                    CurrentKinect = args.NewSensor;
                    
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
                        kinect_type = "kinect for windows";
                        Log.Info("Using " + kinect_type);
                    }
                    catch (InvalidOperationException)
                    {
                        kinect_type = "xbox kinect";
                        Console.WriteLine(kinect_type);
                        Log.Info("Using " + kinect_type);
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

        public static String hand_state = "released-right";
        private ColorImageFormat colorImageFormat = ColorImageFormat.Undefined;
        private DepthImageFormat depthImageFormat = DepthImageFormat.Undefined;

        public void Dispose()
        {
            this.DestroyFaceTracker();
        }

        private void DestroyFaceTracker()
        {
            if (this.faceTracker != null)
            {
                this.faceTracker.Dispose();
                this.faceTracker = null;
            }
        }

        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (closing)
            {
                return;
            }

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
                        if (this.depthImageFormat != depthFrame.Format)
                        {
                            this.DestroyFaceTracker();
                            //this.depthImage = null;
                            this.depthImageFormat = depthFrame.Format;
                        }

                        if (this.colorImageFormat != colorFrame.Format)
                        {
                            this.DestroyFaceTracker();
                            //this.colorImage = null;
                            this.colorImageFormat = colorFrame.Format;
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

                LifeCycle.LifeLoop();

                if ((colorFrame != null)&&((Config.RunningFacetracking)|| Config.IsDevMode()))
                {
                    DisplayVideo(colorFrame);
                }


                //Get a skeleton
                if (skeletonFrameData != null)
                {
                    //get closest skeleton
                    skeletonFrameData.CopySkeletonDataTo(allSkeletons);
                    Skeleton first = GetPrimarySkeleton(allSkeletons);

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
                        //check if hand is open or closed
                        hand_state = KinectRegion.message_hand;

                        //get joint coordinates
                        float[] coordinates = setCoordinates(first);

                        //Map the skeletal coordinates to the video map
                        MapSkeletonToVideo(first, depthFrame, coordinates);

                        //Handles gestures - eg Swipe gesture
                        Application_Handler.ManageGestures(coordinates);

                        //Managing data send to Node                 
                        Application_Handler.Manage_Skeletal_Data(coordinates, first);
                 
                        
                        if ((coordinates[19] > 0.9)&&(Config.RunningFacetracking))
                        {
                            if (old_first == null)
                            {
                                old_first = first;
                                Log.Debug("first skeleton lock");
                            }
                            else if ((old_first != first))
                            {
                                Console.WriteLine("locked skeleton changed");
                                old_first = first;
                                if ((this.faceTracker != null))
                                {
                                    this.faceTracker.ResetTracking();
                                }
                            }

                            if (this.faceTracker == null)
                            {
                                try
                                {
                                    this.faceTracker = new FaceTracker(CurrentKinect);
                                    Console.WriteLine("initiate new FaceTracker");
                                }
                                catch (InvalidOperationException)
                                {
                                    // During some shutdown scenarios the FaceTracker
                                    // is unable to be instantiated.  Catch that exception
                                    // and don't track a face.                            
                                    this.faceTracker = null;
                                }
                            }

                            if (this.faceTracker != null)
                            {
                                FaceTrackFrame faceTrackFrame = this.faceTracker.Track(
                                    colorImageFormat,
                                    this.colorImage,
                                    depthImageFormat,
                                    this.depthImage,
                                    first);


                                if (faceTrackFrame.TrackSuccessful)
                                {                                    
                                    face_x = (int)(100 * faceTrackFrame.Rotation.X);

                                    //gets the x,y-coordinate of where the user is looking at the screen
                                    face_x = Convert.ToInt32((-700 * coordinates[19] * Math.Tan(faceTrackFrame.Rotation.X * Math.PI / 180)) + (coordinates[6] * 1000));
                                    face_y = Convert.ToInt32((-1000 * coordinates[19] * Math.Tan(faceTrackFrame.Rotation.Y * Math.PI / 180)) + (coordinates[6] * 1000));

                                    //set limits
                                    if (face_x > 700)
                                        face_x = 700;
                                    else if (face_x < -700)
                                        face_x = -700;

                                    if (face_y > 1000)
                                        face_y = 1000;
                                    else if (face_y < -1000)
                                        face_y = -1000;

                                    //set variable indicating whether user is looking at the screen (where the screen 2000mmx1700mm)
                                    if ((face_y < 1000) && (face_y > -1000))
                                        looking_at_screen = true;
                                    else
                                        looking_at_screen = false;

                                    //display ellipse indicating where useris looking
                                    if (Config.IsDevMode())
                                    {
                                        Canvas.SetTop(faceEllipse, ((240 - face_x / 10) - faceEllipse.Height / 2));
                                        Canvas.SetLeft(faceEllipse, ((320 + face_y/2) - faceEllipse.Width / 2));
                                    }

                                    //set x,y-coordinates relative to screen size canvas
                                    face_x = face_x + 700;
                                    face_y = face_y + 1000;


                                    
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
            if (colorFrame != null)
            {
                // Copy the pixel data from the image to a temporary array
                colorFrame.CopyPixelDataTo(this.colorImage);

                if (Config.IsDevMode())
                {
                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorImage,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }               
        }

        
        float[] setCoordinates(Skeleton first)
        {
            //get joint coordinates
            float[] coordinates = new float[24];
            Joint left_hand = first.Joints[JointType.HandLeft];
            Joint right_hand = first.Joints[JointType.HandRight];
            Joint spine_center = first.Joints[JointType.Spine];
            Joint head = first.Joints[JointType.Head];
            Joint shoulder_left = first.Joints[JointType.ShoulderLeft];
            Joint shoulder_right = first.Joints[JointType.ShoulderRight];
            Joint hip_center = first.Joints[JointType.HipCenter];
            Joint shoulder_center = first.Joints[JointType.ShoulderCenter];
            Joint left_elbow = first.Joints[JointType.ElbowLeft];
            Joint right_elbow = first.Joints[JointType.ElbowRight];

            //set joint coordinates into array
            coordinates[0] = left_hand.Position.X;
            coordinates[1] = left_hand.Position.Y;
            coordinates[2] = right_hand.Position.X;
            coordinates[3] = right_hand.Position.Y;
            coordinates[4] = 0;// spine_center.Position.X;
            coordinates[5] = 0;// spine_center.Position.Y;
            coordinates[6] = head.Position.X;
            coordinates[7] = head.Position.Y;
            coordinates[8] = shoulder_left.Position.X;
            coordinates[9] = shoulder_left.Position.Y;
            coordinates[10] = shoulder_right.Position.X;
            coordinates[11] = shoulder_right.Position.Y;
            coordinates[12] = 0;// hip_center.Position.X;
            coordinates[13] = 0;// hip_center.Position.Z;
            coordinates[14] = left_hand.Position.Z;
            coordinates[15] = right_hand.Position.Z;
            coordinates[16] = shoulder_center.Position.X;
            coordinates[17] = shoulder_center.Position.Y;
            coordinates[18] = spine_center.Position.Z;
            coordinates[19] = head.Position.Z;
            coordinates[20] = left_elbow.Position.X;
            coordinates[21] = left_elbow.Position.Y;
            coordinates[22] = right_elbow.Position.X;
            coordinates[23] = right_elbow.Position.Y;

            return coordinates;
        }

        
        //overlaying IR camera and RGB camera video feeds
        void MapSkeletonToVideo(Skeleton first, DepthImageFrame depth, float[] coord)
        {
            
                if (depth == null || sensorChooser.Kinect == null)
                {
                    return;
                }
                depth.CopyPixelDataTo(depthImage);

                CoordinateMapper cm = new CoordinateMapper(CurrentKinect);
                //Map a skeletal point to a point on the color image 
                ColorImagePoint headColorPoint = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.Head].Position, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftColorPoint = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.HandLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightColorPoint = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.HandRight].Position, ColorImageFormat.RgbResolution640x480Fps30);

                int[] depth_coord = Image_Processing.ProcessHands(depthImage);
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

                //only show video HUD when running in dev mode
                if (Config.IsDevMode())
                {
                    Animations(first, headColorPoint, leftColorPoint, rightColorPoint, depth_hand, depth_hand2, coord, depth_coord, hand_selection);
                }
            

        }

        //delete if in production
        public void Animations(Skeleton first, ColorImagePoint headColorPoint, ColorImagePoint leftColorPoint, ColorImagePoint rightColorPoint, ColorImagePoint depth_hand, ColorImagePoint depth_hand2, float[] coord, int[] depth_coord, int hand_selection)
        {
            Joint left_hand = first.Joints[JointType.HandLeft];
            Joint right_hand = first.Joints[JointType.HandRight];
            if (Convert.ToString(left_hand.TrackingState) == "Tracked")
            {
                leftEllipse.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
            }
            else
            {
                leftEllipse.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 255));
            }

            if (Convert.ToString(right_hand.TrackingState) == "Tracked")
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

        //find closest skeleton
        public static bool track_near = true;
        public static bool skeleton_locked = false;
        public static int skeleton_id = 0;
        public static int locked_skeleton_id = 0;
        public static Skeleton locked_skeleton = null;
        public static int skeleton_count = 0;
        private Skeleton GetPrimarySkeleton(Skeleton[] skeletons)
        {
            Skeleton skeleton = null;

            if (skeletons != null)
            {

                //check if any skeletons has been locked on in t-1        
                if (Application_Handler.locked_skeleton == true)
                {
                    for (int i = 0; i < skeletons.Length; i++)
                    {
                        if (skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                        {
                            if (skeletons[i].TrackingId == locked_skeleton_id)
                            {
                                //locked skeletal is still in range
                                skeleton = locked_skeleton;
                                break;
                            }
                            else
                            {
                                Application_Handler.locked_skeleton = false;
                                
                            }
                        }
                    }

                }
                else
                {
                    //Find the closest skeleton        
                    for (int i = 0; i < skeletons.Length; i++)
                    {
                        if (skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                        {
                            if (skeleton == null)
                            {
                                skeleton = skeletons[i];
                            }
                            else
                            {
                                if ((skeleton.Position.Z > skeletons[i].Position.Z))//if not just use the closest skeleton
                                {
                                    skeleton = skeletons[i];
                                }
                            }
                        }
                    }
                }

            }

            //if there is no other users in die fov and the locked skeleton moves out of the field of view
            //stop stopwatch 
            if ((skeleton == null) && (Application_Handler.locked_skeleton == true))
            {
                Application_Handler.locked_skeleton = false;
            }

            return skeleton;
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //this.sensorChooser.Stop();
            StopKinect(sensorChooser.Kinect);
            SocketServer.Stop();
            Log.Info("Shutting down server");
            closing = true; 
        }

        //stop kinect
        public static void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    //stop sensor 
                    sensor.Stop();

                    //stop audio if not null
                    if (sensor.AudioSource != null)
                    {
                        sensor.AudioSource.Stop();
                    }

                }
            }
        }

    }
}
