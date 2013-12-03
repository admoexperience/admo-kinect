using System;
using Admo.classes;
using Admo.Utilities;
using Microsoft.Kinect;
using NLog;
using System.Collections;
using System.Globalization;
using System.Text;

namespace Admo
{
    public class ApplicationHandler
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        //variables for changing the fov when using a webcam instead of the kinect rgb camera

        public const int KinectFovHeight = Constants.KinectHeight;
        public const int KinectFovWidth = Constants.KinectWidth;

        /// <summary>
        /// Gets usert outline from an image that has undergone background removal
        /// </summary>
        /// <param name="userData">Raw bytes of RGBA 640*480 image that has undergon background removal</param>
        public string GetUserOutline(byte[] userData)
        {

            if (userData == null) throw new ArgumentNullException("userData");

            StringBuilder builder = new StringBuilder();

            int width = 640;
            int height = 480;

            int stringCount = 0;

            int pixelCount = 2500;

            var startPixel = GetStartPixel(userData);
            int[] currentPixel = new int[2];
            int[] previousPixel = new int[2];
            int[,] outlinePixels = new int [pixelCount,2];
            int[,] blob = new int[3, 3];

            int boundaries = 15;

            int countIter = 0;

            var vectorArray = "";

            //get start pixel above head
            //GetStartPixel(userData, height, width, startPixel);

            outlinePixels[0, 0] = currentPixel[0] = startPixel[0];
            outlinePixels[0, 1] = currentPixel[1] = startPixel[1] + 1;

            //the previous pixel would be that of y + 1 - due to vertical upwards search
            previousPixel[0] = currentPixel[0];
            previousPixel[1] = currentPixel[1] + 1;

            //generating svg
            //http://raphaeljs.com/reference.html#Paper.path


            builder.Append("M").Append(startPixel[0]).Append(",").Append(500).Append(" R");


            for (int count = 1; count < pixelCount; count++)
            {
                int index = (currentPixel[1] - 1) * width + (currentPixel[0] - 1);

                if (((currentPixel[1] + 1) > (Constants.KinectWidth - boundaries)) || ((currentPixel[1] - 1) < boundaries) || ((currentPixel[0] + 1) > (Constants.KinectWidth - boundaries)) || ((currentPixel[0] - 1) < boundaries))
                {
                    break;
                }

                for (int blobX = 0; blobX < 3; blobX++)
                {
                    for (int blobY = 0; blobY < 3; blobY++)
                    {
                        int tempIndex = index + blobY * width + blobX;
                        try
                        {
                            blob[blobX, blobY] = userData[tempIndex];

                        }
                        catch (Exception e)
                        {

                        }
                        
                    }
                }

                int[] previousPixelRelative = new int[2];
                previousPixelRelative[0] = previousPixel[0] - currentPixel[0];
                previousPixelRelative[1] = previousPixel[1] - currentPixel[1];

                int[] nextPixelRelative = GetNextPixel(blob, previousPixelRelative);

                int[] nextPixel = new int[2];
                nextPixel[0] = nextPixelRelative[0] + currentPixel[0];
                nextPixel[1] = nextPixelRelative[1] + currentPixel[1];

                previousPixel = currentPixel;
                currentPixel = nextPixel;

                countIter++;

                outlinePixels[countIter, 0] = currentPixel[0];
                outlinePixels[countIter, 1] = currentPixel[1];

                int delta = 15;

                if ((countIter % delta) == 0)
                {
                    int temX = 0;
                    int temY = 0;
                    int iter = (int)(countIter / delta);
                    iter = iter * delta;
                    //Console.WriteLine(iter);

                    for (int i = -delta; i < 1; i++)
                    {
                        temX = temX + outlinePixels[(i + iter), 0];
                        temY = temY + outlinePixels[(i + iter), 1];
                    }

                    temX = temX / delta;
                    temY = temY / delta;

                    builder.Append(temX).Append(",").Append(temY).Append(" ");
                }

                int diffX = Math.Abs(currentPixel[0] - startPixel[0]);
                int diffY = Math.Abs(currentPixel[1] - startPixel[1]);

                //check whether current coordinate set is close to the start coordinate set - if so, exit the loop and close the vector
                if ((diffX < 5) && (diffY < 5) && (count > 250))
                {
                    builder.Append("z");
                    break;
                }
            }

            return builder.ToString();
        }
        /// <summary>
        /// Get fist pixel of a person
        /// </summary>
        /// <param name="userData">Raw depth data</param>
        public static int[] GetStartPixel(byte[] userData)
        {
            //To do loop other way round for speed
            for (var ycoord = (Constants.KinectHeight - 1); ycoord >= 0; ycoord--)
            {
                for (var xcoord = (Constants.KinectWidth - 1); xcoord >= 0; xcoord--)
                {
                    int player = userData[ycoord * Constants.KinectWidth + xcoord];

                    if ((xcoord > (Constants.KinectWidth - 5)) || (xcoord < 5) ||
                        (ycoord > (Constants.KinectHeight - 5)) || (ycoord < 5))
                    {
                        player = 0;
                    }

                    if (player > 0)
                    {
                        int[] startPixel = { xcoord, ycoord };
                        return startPixel;
                    }
                }
            }
            return new int[2];
        }

        /// <summary>
        /// analyze 3x3 blog to find the next pixel for the vector
        /// </summary>
        /// <param name="rightBlob"></param>
        /// <param name="previousPixelRelative"></param>
        public int[] GetNextPixel(int[,] rightBlob, int[] previousPixelRelative)
        {
            int[] nextPixel = {0, 0};
            int[,] list = { { -1, -1 }, { 0, -1 }, { 1, -1 }, { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 }, { -1, 0 }, { -1, -1 }, { 0, -1 }, { 1, -1 }, { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 }, { -1, 0 } };
            int count = 0;
            int check = 1;

            //identify which one of the 8 surrounding pixel was the origin for the previous blob
            for (count = 0; count < 8; count++)
            {
                if ((previousPixelRelative[0] == list[count, 0]) && (previousPixelRelative[1] == list[count, 1]))
                {
                    break;
                }
            }

            //check which one of the 8 surrounding pixels will be the new center pixel
            for (check = 1; check < 10; check++)
            {
                //start looking at n+1, where n is where the previous pixel was located
                int index = check + count;

                int[] pixel = {list[index, 0], list[index, 1] };
                

                if(rightBlob[(pixel[0] + 1), (pixel[1] +1)] > 0)
                {
                    index--;
                    nextPixel[0] = list[index, 0];
                    nextPixel[1] = list[index, 1];

                    break;
                }
                else if (check == 9)
                {
                    nextPixel[0] = list[index, 0];
                    nextPixel[1] = list[index, 1];
                }
            }

            return nextPixel;
        }

        /// <summary>
        /// Find a possible person in the depth image
        /// </summary>
        /// <param name="rawDepthData">The skeleton from the kinect</param>
        /// <param name="height">The coordinate mapper</param>
        /// <param name="width">The coordinate mapper</param>
        public KinectState FindPlayer(short[] rawDepthData, int height, int width)
        {

            var timeNow = Utils.GetCurrentTimeInSeconds();
            var timeDelta = timeNow - _timeLostUser;
            const double timeWait = 2.5;


            var zCoord = 0;
            var xCoord = 0;
            var yCoord = 0;

            //MUST loop through it row by row instead of column
            //If not it swtiches states continuously
            for (var ycoord = 0; ycoord < height; ycoord++) 
            {
                for (var xcoord = 0; xcoord < width; xcoord++)
                {
                    //bitshift conversion for some reason the kinect needs it
                    var currDepth = rawDepthData[xcoord + ycoord * width] >> DepthImageFrame.PlayerIndexBitmaskWidth; 
                    if ((currDepth > 400) && (currDepth < 2500))
                    {
                        xCoord = xcoord;
                        yCoord = ycoord;
                        zCoord = currDepth;
                        xcoord = width;
                        ycoord = height;
                    }
                }
            }

            var kinectState = new KinectState { Phase = 1 };

            if ((xCoord > 50) && (xCoord < 590) && (yCoord < 250))
            {
                /*array_xy[] provides the x and y coordinates of the first pixel detected in the depthmap between 450mm and 3000mm, 
                in this case the users head because the closest pixel search in the depthmap starts at the top and left margins, 
                the first pixel (array_xy) would be at the top left corner of the user's head to get the centre of the user's head,
                * we must add a dynamic variable (which change depending on how far away the user is) to the x and y coordinates */

                double xMiddle = 35000 / zCoord;
                double yMiddle = 80000 / zCoord;


                xCoord = (int)(xCoord + xMiddle);
                yCoord = (int)(yCoord + yMiddle);

                kinectState.SetHead(xCoord, yCoord, zCoord, 0, 0);
                kinectState.Phase = 2;
            }


            //checks whether the user was standing in the middle of the fov when tracking of said user was lost
            //if this is the case then in all likelyhood someone walk inbetween the kinect and the user

            if ((_standinMiddle) && (_previousKinectState != null) && (timeDelta < timeWait))
            {
                //since the user is still in the fov, although not visible by the kinect, use the kinectState of when the user was last visible until the user is visible again
                kinectState = _previousKinectState;
                _firstDetection = false;

                _lostUser = true;
                _timeFoundUser = Utils.GetCurrentTimeInSeconds();
            }
            else
            {
                _firstDetection = true;
                _lostUser = false;
            }

            return kinectState;

        }

        private double _timeStartHud = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
        public bool Detected = false;
        private bool _firstDetection = true;

        private bool _standinMiddle;
        private bool _lostUser;
        private KinectState _previousKinectState = new KinectState();
        private double _timeLostUser = Utils.GetCurrentTimeInSeconds();
        private double _timeFoundUser = Utils.GetCurrentTimeInSeconds();
        private InternKinectState _filteredKinectState;
        //Value between 0 and 1 indicating the degree of filtering
        public const float FilterConst = (float)0.7;
        private bool _isFirstExecute = true;

        /// <summary>
        /// Post processes the Skeletal data to a form usable by the browser
        /// </summary>
        /// <param name="first">The skeleton from the kinect</param>
        /// <param name="cm">The coordinate mapper</param>
        public void Manage_Skeletal_Data(Skeleton first, CoordinateMapper cm)
        {
            int mode = GetStage(first.Joints[JointType.Head].Position.X);

            if (mode == 3)
            {
                if (TheHacks.LockedSkeleton == false)
                {
                    MainWindow.KinectLib.LockedSkeletonId = first.TrackingId;
                    MainWindow.KinectLib.LockedSkeleton = first;
                    TheHacks.LockedSkeleton = true;
                }
            }

            var kinectState = new KinectState { Phase = mode };

            var currState = new InternKinectState
            {
                Head = first.Joints[JointType.Head].Position,
                HandRight = first.Joints[JointType.HandRight].Position,
                HandLeft = first.Joints[JointType.HandLeft].Position,
                ElbowRight = first.Joints[JointType.ElbowRight].Position,
                ElbowLeft = first.Joints[JointType.ElbowLeft].Position

            };

            if (_isFirstExecute)
            {
                _filteredKinectState = currState;
                _isFirstExecute = false;
            }
            //Applies filter to the state of Kinect
            currState = FilterState(currState, _filteredKinectState, FilterConst);

            _filteredKinectState = currState;
            //Map a skeletal point to a point on the color image 
            var headColorPoint = cm.MapSkeletonPointToColorPoint(currState.Head,
                                                                             ColorImageFormat.RgbResolution640x480Fps30);
            var leftColorPoint = cm.MapSkeletonPointToColorPoint(currState.HandLeft,
                                                                             ColorImageFormat.RgbResolution640x480Fps30);
            var rightColorPoint = cm.MapSkeletonPointToColorPoint(currState.HandRight,
                                                                              ColorImageFormat.RgbResolution640x480Fps30);
            var elbowLeft = cm.MapSkeletonPointToColorPoint(currState.ElbowLeft,
                                                                             ColorImageFormat.RgbResolution640x480Fps30);
            var elbowRight = cm.MapSkeletonPointToColorPoint(currState.ElbowRight,
                                                                              ColorImageFormat.RgbResolution640x480Fps30);

            //Sadly nescesary evil before more major refactor
            TheHacks.UncalibratedCoordinates[2] = leftColorPoint.X;
            TheHacks.UncalibratedCoordinates[4] = rightColorPoint.X;
            TheHacks.UncalibratedCoordinates[3] = leftColorPoint.Y;

            kinectState.RightHand = ScaleCoordinates(currState.HandRight, rightColorPoint);
            kinectState.LeftHand = ScaleCoordinates(currState.HandLeft, leftColorPoint);
            kinectState.Head = ScaleCoordinates(currState.Head, headColorPoint);
            kinectState.RightElbow = ScaleCoordinates(currState.ElbowRight, elbowRight);
            kinectState.LeftElbow = ScaleCoordinates(currState.ElbowLeft, elbowLeft);

            double timeNow = Utils.GetCurrentTimeInSeconds();
            double timeDelta = timeNow - _timeFoundUser;
            const double timeWait = 2.5;

            //checks whether the user is standing in die middle of the horizonal axis fov of the kinect with a delta of 400mm 
            const double deltaMiddle = 0.4;
            float headX = first.Joints[JointType.Head].Position.X;
            if ((headX < deltaMiddle) && (headX > -deltaMiddle))
            {
                _standinMiddle = true;
                //remember the kinectState(t-1)
                _previousKinectState = kinectState;
                _timeLostUser = Utils.GetCurrentTimeInSeconds();
            }
            else
            {
                _standinMiddle = false;
            }

            //he was lost but now he is found

            if (_lostUser)
            {
                kinectState = _previousKinectState;
                if ((headX < deltaMiddle) && (headX > -deltaMiddle))
                {
                    _lostUser = false;
                }
                else if (timeDelta > timeWait)
                {
                    _lostUser = false;
                }
            }

            SocketServer.SendKinectData(kinectState);
        }

        /// <summary>
        /// Scales the coordinates so they can be used on a larger screen. With additinal edge case checking
        /// </summary>
        /// <param name="pos">A point on the skeleton</param>
        /// <param name="colorImagePoint">The ColorImagePoint image from a point on the skeleton</param>
        public static Position ScaleCoordinates(SkeletonPoint pos, ColorImagePoint colorImagePoint)
        {
            var admoPos = new Position
            {
                X = (int)((colorImagePoint.X - TheHacks.FovLeft) * (KinectFovWidth / TheHacks.FovWidth)),
                Y = (int)((colorImagePoint.Y - TheHacks.FovTop) * (KinectFovHeight / TheHacks.FovHeight)),
                Z = (int)(pos.Z * 1000)
            };

            if (admoPos.X < 0)
            {
                admoPos.X = 0;
            }
            else if (admoPos.X > KinectFovWidth)
            {
                admoPos.X = KinectFovWidth;
            }

            if (admoPos.Y < 0)
            {
                admoPos.Y = 0;
            }
            else if (admoPos.Y > KinectFovHeight)
            {
                admoPos.Y = KinectFovHeight;
            }
            admoPos.Xmm = pos.X;
            admoPos.Ymm = pos.Y;

            return admoPos;
        }
        /// <summary>
        /// Determines the user stage from the head X cooridinate
        /// </summary>
        /// <param name="headX">The X coordinate of the skeletons head</param>
        public int GetStage(float headX)
        {
            int mode = 1;

            double timeNow = Convert.ToDouble(DateTime.Now.Ticks) / 10000;

            //if user is detected and is in middle of screen
            if (Math.Abs(headX) < 0.7) // | (detected == true))
            {
                //when user is initialy registred
                if (_firstDetection)
                {
                    _firstDetection = false;
                    _timeStartHud = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
                }
                else
                {
                    double timeDelta = timeNow - _timeStartHud;

                    //time the user must stand in the centre in order for the process to start
                    if (timeDelta > 500)
                    {
                        //skeleton lock  

                        mode = 3;
                    }
                    else
                    {
                        mode = 2;
                    }
                }
            }
            else //if user is detected and not in the middle of the screen
            {
                mode = 2;
                _firstDetection = true;
                TheHacks.LockedSkeleton = false;
            }

            return mode;
        }
        /// <summary>
        /// Performs a exponential wheigthed moving average xhat(k)=x(k)*alpha+x(k-1)*(1-alpha)
        /// </summary>
        /// <param name="current">The current unfiltered value</param>
        /// <param name="filter">The previous filtered value</param>
        /// <param name="alpha">The degree of filtering</param>
        public static float ExponentialWheightedMovingAverage(float current, float filter, float alpha)
        {

            return current * alpha + filter * (1 - alpha);
        }

        /// <summary>
        /// Performs a exponential wheigthed moving average xhat(k)=x(k)*alpha+x(k-1)*(1-alpha) on a kinect state
        /// </summary>
        /// <param name="currState">The current unfiltered value</param>
        /// <param name="filteredState">The previous filtered value</param>
        /// <param name="filterConst">The degree of filtering</param>
        public static InternKinectState FilterState(InternKinectState currState, InternKinectState filteredState, float filterConst)
        {
            currState.HandLeft = FilterPoint(currState.HandLeft, filteredState.HandLeft, filterConst);
            currState.HandRight = FilterPoint(currState.HandRight, filteredState.HandRight, filterConst);
            currState.Head = FilterPoint(currState.Head, filteredState.Head, filterConst);
            currState.ElbowLeft = FilterPoint(currState.ElbowLeft, filteredState.ElbowLeft, filterConst);
            currState.ElbowRight = FilterPoint(currState.ElbowRight, filteredState.ElbowRight, filterConst);


            return currState;
        }


        /// <summary>
        /// Performs a exponential wheigthed moving average xhat(k)=x(k)*alpha+x(k-1)*(1-alpha) on a 3d point
        /// </summary>
        /// <param name="currPoint">The current unfiltered value</param>
        /// <param name="filteredPoint">The previous filtered value</param>
        /// <param name="filterConst">The degree of filtering</param>
        public static SkeletonPoint FilterPoint(SkeletonPoint currPoint, SkeletonPoint filteredPoint, float filterConst)
        {
            currPoint.X = ExponentialWheightedMovingAverage(currPoint.X, filteredPoint.X, filterConst);
            currPoint.Y = ExponentialWheightedMovingAverage(currPoint.Y, filteredPoint.Y, filterConst);
            currPoint.Z = ExponentialWheightedMovingAverage(currPoint.Z, filteredPoint.Z, filterConst);
            return currPoint;
        }

        public static void ConfigureCalibrationByConfig()
        {
            //refer to document Calibration Method
            //Dropbox/Admo/Hardware Design/Documents/Sensor Array Calibration Method.docx
            TheHacks.FovTop = Config.GetFovCropTop();
            TheHacks.FovLeft = Config.GetFovCropLeft();
            TheHacks.FovWidth = Config.GetFovCropWidth();
            TheHacks.FovHeight = TheHacks.FovWidth * 3 / 4;
        }
    }
}