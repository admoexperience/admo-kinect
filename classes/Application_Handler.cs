﻿using System;
using Admo.classes;
using Microsoft.Kinect;
using NLog;

namespace Admo
{
    internal class Application_Handler
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        //variables for changing the fov when using a webcam instead of the kinect rgb camera

        public static float FovHeight = 640;
        public static float FovWidth = 480;
        const int KinectFovHeight = 480;
        const int KinectFovWidth = 640;
        public static float FovLeft = 0;
        public static float FovTop = 0;
        
        //Skeletal coordinates in meters
        //Find a possible person in the depth image
        public static void FindPlayer(DepthImageFrame depthFrame)
        {
            if (depthFrame == null)
            {
                return;
            }

            //get the raw data from kinect with the depth for every pixel

            short[] rawDepthData = new short[depthFrame.PixelDataLength];

            double timeNow = LifeCycle.GetCurrentTimeInSeconds();
            double timeDelta = timeNow - TimeLostUser;
            const double timeWait = 2.5;

            depthFrame.CopyPixelDataTo(rawDepthData);

            Byte[] pixels = new byte[depthFrame.Height*depthFrame.Width*4];

            int xCoord = 0;
            int yCoord = 0;
            int zCoord = 0;

            //loop through all distances
            //pick a RGB color based on distance
            for (int depthIndex = 0, colorIndex = 0;
                 depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                 depthIndex++, colorIndex += 4)
            {
                //gets the depth value
                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                // Distance user is required to stand
                if ((depth > 400) && (depth < 2500))
                {
                    yCoord = depthIndex/(depthFrame.Width);
                    xCoord = depthIndex - yCoord*(depthFrame.Width);
                    zCoord = depth;
                    break;
                }
            }

            var kinectState = new KinectState();
            kinectState.Phase = 1;

            if ((xCoord > 50) && (xCoord < 590) && (yCoord < 250))
            {
                /*array_xy[] provides the x and y coordinates of the first pixel detected in the depthmap between 450mm and 3000mm, 
                in this case the users head because the closest pixel search in the depthmap starts at the top and left margins, 
                the first pixel (array_xy) would be at the top left corner of the user's head to get the centre of the user's head,
                * we must add a dynamic variable (which change depending on how far away the user is) to the x and y coordinates */

                double xMiddle = 35000 / zCoord;
                double yMiddle = 80000 / zCoord;

                float[] arrayXy = Coordinate_History.Filter_Depth(xCoord, yCoord);
                xCoord = (int)(arrayXy[0] + xMiddle);
                yCoord = (int)(arrayXy[1] + yMiddle);

                kinectState.SetHead(xCoord, yCoord, zCoord, 0, 0);
                kinectState.Phase = 2;
            }


            //checks whether the user was standing in the middle of the fov when tracking of said user was lost
            //if this is the case then in all likelyhood someone walk inbetween the kinect and the user
            
            if ((StandinMiddle) && (PreviousKinectState != null) && (timeDelta < timeWait))
            {
                //since the user is still in the fov, although not visible by the kinect, use the kinectState of when the user was last visible until the user is visible again
                kinectState = PreviousKinectState;
                FirstDetection = false;

                LostUser = true;
                TimeFoundUser = LifeCycle.GetCurrentTimeInSeconds();
            }
            else
            {
                FirstDetection = true;
                LostUser = false;
            }

            SocketServer.SendKinectData(kinectState);
            
        }


      //  public static int[] StickCoord = new int[10];
        public static int[] UncalibratedCoordinates = new int[6];
        public static double TimeStartHud = Convert.ToDouble(DateTime.Now.Ticks)/10000;
        public static bool Detected = false;
        public static bool FirstDetection = true;
        public static bool LockedSkeleton = false;
        public static bool StandinMiddle = false;
        public static bool LostUser = false;
        public static KinectState PreviousKinectState = new KinectState();
        public static double TimeLostUser = LifeCycle.GetCurrentTimeInSeconds();
        public static double TimeFoundUser = LifeCycle.GetCurrentTimeInSeconds();

        //generate string from joint coordinates to send to node server to draw stickman
        public static void Manage_Skeletal_Data(Skeleton first,CoordinateMapper cm)
        {
            int mode = Stages(first);
            var kinectState = new KinectState { Phase = mode };

 
            //Map a skeletal point to a point on the color image 
            
            //Map a skeletal point to a point on the color image 
            ColorImagePoint headColorPoint = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.Head].Position, ColorImageFormat.RgbResolution640x480Fps30);
            ColorImagePoint leftColorPoint = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.HandLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);
            ColorImagePoint rightColorPoint = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.HandRight].Position, ColorImageFormat.RgbResolution640x480Fps30);

            //Sadly nescesary evil before more major refactor
            UncalibratedCoordinates[2] = leftColorPoint.X;
            UncalibratedCoordinates[4] = rightColorPoint.X;
            UncalibratedCoordinates[3] = leftColorPoint.Y;

            kinectState.RightHand = ScaleCoordinates(first.Joints[JointType.HandRight].Position, rightColorPoint);
            kinectState.LeftHand = ScaleCoordinates(first.Joints[JointType.HandLeft].Position, leftColorPoint);
            kinectState.Head = ScaleCoordinates(first.Joints[JointType.Head].Position, headColorPoint);

            double timeNow = LifeCycle.GetCurrentTimeInSeconds();
            double timeDelta = timeNow - TimeFoundUser;
            const double timeWait = 2.5;

            //checks whether the user is standing in die middle of the horizonal axis fov of the kinect with a delta of 400mm 
            const double deltaMiddle = 0.4;
            var headX = first.Joints[JointType.Head].Position.X;
            if ((headX < deltaMiddle) && (headX > -deltaMiddle))
            {
                StandinMiddle = true;
                //remember the kinectState(t-1)
                PreviousKinectState = kinectState;
                TimeLostUser = LifeCycle.GetCurrentTimeInSeconds();
            }
            else
            {
                StandinMiddle = false;
            }

            //he was lost but now he is found
            
            if (LostUser)
            {
                kinectState = PreviousKinectState;
                if ((headX < deltaMiddle) && (headX > -deltaMiddle))
                {
                    LostUser = false;
                }
                else if (timeDelta > timeWait)
                {
                    LostUser = false;
                }

            }

            SocketServer.SendKinectData(kinectState);
        }

        public static Position ScaleCoordinates(SkeletonPoint pos,ColorImagePoint colorImagePoint)
        {

            var admoPos = new Position();

            admoPos.X = (int)((colorImagePoint.X - FovLeft) * (KinectFovWidth / FovWidth));
            admoPos.Y = (int)((colorImagePoint.Y - FovTop) * (KinectFovHeight / FovHeight));
            admoPos.Z = (int)(pos.Z * 1000);

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

        public static int Stages( Skeleton first)
        {
            int mode = 1;

            double timeNow = Convert.ToDouble(DateTime.Now.Ticks)/10000;

            //if user is detected and is in middle of screen
            var headX = first.Joints[JointType.HandRight].Position.X;
            if (Math.Abs(headX)<0.7) // | (detected == true))
            {
                //when user is initialy registred
                if (FirstDetection)
                {
                    FirstDetection = false;
                    TimeStartHud = Convert.ToDouble(DateTime.Now.Ticks)/10000;
                }
                else
                {
                    double timeDelta = timeNow - TimeStartHud;

                    //time the user must stand in the centre in order for the process to start
                    if (timeDelta > 500)
                    {
                        //skeleton lock  
                        if (LockedSkeleton == false)
                        {
                            MainWindow.KinectLib.LockedSkeletonId = first.TrackingId;
                            MainWindow.KinectLib.LockedSkeleton = first;
                            LockedSkeleton = true;
                        }

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
                FirstDetection = true;
                LockedSkeleton = false;
            }

            return mode;
        }

        public static float ExponentialWheightedMovingAverage(float current, float filter, float alpha)
        {
            return current*alpha + filter + (1 - alpha);
        }


        public static void ConfigureCalibrationByConfig()
        {
            //read calibration values from CMS if calibration app has not been set to run
            //use legacy calibration values if there is no calibration values in the CMS
            var tempTop = Config.ReadConfigOption(Config.Keys.FovCropTop, "56");
            var tempLeft = Config.ReadConfigOption(Config.Keys.FovCropLeft, "52");
            var tempWidth = Config.ReadConfigOption(Config.Keys.FovCropWidth, "547");

            //refer to document Calibration Method
            //Dropbox/Admo/Hardware Design/Documents/Sensor Array Calibration Method.docx
            FovTop = Convert.ToInt32(tempTop);
            FovLeft = Convert.ToInt32(tempLeft);
            FovWidth = Convert.ToInt32(tempWidth);
            FovHeight = FovWidth * 3 / 4;
        }

       

    }
}
