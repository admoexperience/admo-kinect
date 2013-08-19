using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Admo.classes;
using Microsoft.Kinect;
using System.Windows;
using NLog;

namespace Admo
{
    internal class Application_Handler
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public static String toggle = "gestures";
        public static String stickman = " ";

        //variables for changing the fov when using a webcam instead of the kinect rgb camera
        public static double fov_top = 0;
        public static double fov_left = 0;
        public static double fov_height = 640;
        public static double fov_width = 480;


        //manage gestures
        public static void ManageGestures(float[] coordinates)
        {
        }


        //Find a possible person in the depth image
        public static void FindPlayer(DepthImageFrame depthFrame)
        {
            if (depthFrame == null)
            {
                return;
            }

            //get the raw data from kinect with the depth for every pixel

            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            int[,] array = new int[depthFrame.Height,depthFrame.Width]; //(480,640)

            double timeNow = LifeCycle.GetCurrentTimeInSeconds();
            double timeDelta = timeNow - timeLostUser;
            double timeWait = 2.5;

            depthFrame.CopyPixelDataTo(rawDepthData);

            Byte[] pixels = new byte[depthFrame.Height*depthFrame.Width*4];

            int x_coord = 0;
            int y_coord = 0;
            int z_coord = 0;

            //loop through all distances
            //pick a RGB color based on distance
            for (int depthIndex = 0, colorIndex = 0;
                 depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                 depthIndex++, colorIndex += 4)
            {
                //get the player (requires skeleton tracking enabled for values)
                int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;

                //gets the depth value
                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if ((depth > 400) && (depth < 2500))
                {
                    y_coord = depthIndex/(depthFrame.Width);
                    x_coord = depthIndex - y_coord*(depthFrame.Width);
                    z_coord = depth;
                    break;
                }
            }

            int mode = 1;
            var kinectState = new KinectState();
            kinectState.Phase = 1;
            String video_coord = "0^0^0^0^0^0^0^0^0";

            

            if ((x_coord > 50) && (x_coord < 590) && (y_coord < 250))
            {
                //array_xy[] provides the x and y coordinates of the first pixel detected in the depthmap between 450mm and 3000mm, in this case the users head
                //because the closest pixel search in the depthmap starts at the top and left margins, the first pixel (array_xy) would be at the top left corner of the user's head
                //to get the centre of the user's head, we must add a dynamic variable (which change depending on how far away the user is) to the x and y coordinates
                double xMiddle = 35000 / z_coord;
                double yMiddle = 80000 / z_coord;

                float[] array_xy = Coordinate_History.Filter_Depth(x_coord, y_coord);
                x_coord = (int)(array_xy[0] + xMiddle);
                y_coord = (int)(array_xy[1] + yMiddle);

                
                String head = Convert.ToString(x_coord) + "^" + Convert.ToString(y_coord);
                String depth_head = Convert.ToString(z_coord);
                video_coord = head + "^" + depth_head + "^0^0^0^0^0^0";
                kinectState.SetHead(x_coord, y_coord, z_coord);
                kinectState.Phase = 2;
                mode = 2;
            }

            stickman = Convert.ToString(mode) + "-" + video_coord;

            //checks whether the user was standing in the middle of the fov when tracking of said user was lost
            //if this is the case then in all likelyhood someone walk inbetween the kinect and the user
            
            if ((standinMiddle) && (previousKinectState != null) && (timeDelta < timeWait))
            {
                //since the user is still in the fov, although not visible by the kinect, use the kinectState of when the user was last visible until the user is visible again
                kinectState = previousKinectState;
                first_detection = false;

                lostUser = true;
                timeFoundUser = LifeCycle.GetCurrentTimeInSeconds();
            }
            else
            {
                first_detection = true;
                lostUser = false;
            }

            SocketServer.SendKinectData(kinectState);
            
        }


        public static int[] stick_coord = new int[6];
        public static int[] UncalibratedCoordinates = new int[6];
        public static double time_start_hud = Convert.ToDouble(DateTime.Now.Ticks)/10000;
        public static bool detected = false;
        public static bool first_detection = true;
        public static bool locked_skeleton = false;
        public static bool standinMiddle = false;
        public static bool lostUser = false;
        public static KinectState previousKinectState = new KinectState();
        public static double timeLostUser = LifeCycle.GetCurrentTimeInSeconds();
        public static double timeFoundUser = LifeCycle.GetCurrentTimeInSeconds();

        //generate string from joint coordinates to send to node server to draw stickman
        public static void Manage_Skeletal_Data(float[] coordinates, Skeleton first)
        {
            int[] coord = new int[24];
            int right_hand_z = (int) (coordinates[15]*1000);
            int left_hand_z = (int) (coordinates[14]*1000);
            int head_z = (int) (coordinates[19]*1000);

            double timeNow = LifeCycle.GetCurrentTimeInSeconds();
            double timeDelta = timeNow - timeFoundUser;
            double timeWait = 2.5;

            int kinectFovHeight = 480;
            int kinectFovWidth = 640;

            //adjust skeletal coordinates for kinect and webcam fov difference
            for (int t = 0; t < 6; t = t + 2)
            {
                stick_coord[t] = (int)((stick_coord[t] - fov_left) * (kinectFovWidth / fov_width));
                stick_coord[t + 1] = (int)((stick_coord[t + 1] - fov_top) * (kinectFovHeight / fov_height));

                if (stick_coord[t] < 0)
                {
                    stick_coord[t] = 0;
                }
                else if (stick_coord[t] > kinectFovWidth)
                {
                    stick_coord[t] = kinectFovWidth;
                }

                if (stick_coord[t + 1] < 0)
                {
                    stick_coord[t + 1] = 0;
                }
                else if (stick_coord[t + 1] > kinectFovHeight)
                {
                    stick_coord[t + 1] = kinectFovHeight;
                }
            }

            int mode = Stages(coordinates, first);

            var kinectState = new KinectState {Phase = mode};

            kinectState.SetHead(stick_coord[0], stick_coord[1], head_z);
            kinectState.SetLeftHand(stick_coord[2], stick_coord[3], left_hand_z);
            kinectState.SetRightHand(stick_coord[4], stick_coord[5], right_hand_z);

            //checks whether the user is standing in die middle of the horizonal axis fov of the kinect with a delta of 400mm 
            const double deltaMiddle = 0.4;
            if ((coordinates[6] < deltaMiddle) && (coordinates[6] > -deltaMiddle))
            {
                standinMiddle = true;
                //remember the kinectState(t-1)
                previousKinectState = kinectState;
                timeLostUser = LifeCycle.GetCurrentTimeInSeconds();
            }
            else
            {
                standinMiddle = false;
            }

            //he was lost but now he is found
            
            if (lostUser)
            {
                kinectState = previousKinectState;
                if ((coordinates[6] < deltaMiddle) && (coordinates[6] > -deltaMiddle))
                {
                    lostUser = false;
                }
                else if (timeDelta > timeWait)
                {
                    lostUser = false;
                }

            }

            SocketServer.SendKinectData(kinectState);
        }

        public static int Stages(float[] coordinates, Skeleton first)
        {
            int mode = 1;

            double time_now = Convert.ToDouble(DateTime.Now.Ticks)/10000;

            //if user is detected and is in middle of screen
            if ((((coordinates[6] < 0.7) && (coordinates[6] > -0.7)))) // | (detected == true))
            {
                //when user is initialy registred
                if (first_detection == true)
                {
                    first_detection = false;
                    time_start_hud = Convert.ToDouble(DateTime.Now.Ticks)/10000;
                }
                else
                {
                    double time_delta = time_now - time_start_hud;

                    //time the user must stand in the centre in order for the process to start
                    if (time_delta > 500)
                    {
                        //skeleton lock  
                        if (locked_skeleton == false)
                        {
                            MainWindow.locked_skeleton_id = first.TrackingId;
                            MainWindow.locked_skeleton = first;
                            locked_skeleton = true;
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
                first_detection = true;
                locked_skeleton = false;
            }

            return mode;
        }

        //get the coordinates relative to a standing upright person
        public static double RelativeCoordinates(double length_y, double length_z)
        {
            //anglular variables
            double angle_kinect = MainWindow.KinectElevationAngle;
            double angle_zero;
            double angle_y;
            double angle_k;
            //distance variables
            double length_relative_z = 1;

            if (length_y < 0)
            {
                length_y = length_y*(-1);

                angle_zero = ((90 + angle_kinect)*Math.PI)/180;

                angle_k = Math.Asin(length_y*Math.Sin(angle_zero)/length_z);

                angle_y = Math.PI - angle_zero - angle_k;

                length_relative_z = (length_z*Math.Sin(angle_y)/Math.Sin(angle_zero));
            }
            else if (length_y >= 0)
            {
                angle_zero = ((90 - angle_kinect)*Math.PI)/180;

                angle_k = Math.Asin(length_y*Math.Sin(angle_zero)/length_z);

                angle_y = Math.PI - angle_zero - angle_k;

                length_relative_z = (length_z*Math.Sin(angle_y)/Math.Sin(angle_zero));
            }

            return length_relative_z;
        }

        public static int Select_Hand(double relative_z_head, double relative_z_righthand, double relative_z_lefthand)
        {
            int selection = 1;

            if ((relative_z_righthand < (relative_z_head - 0.2)) && (relative_z_righthand < relative_z_lefthand))
            {
                selection = 2;
            }

            return selection;
        }

        public static double moving_sum_x = 0;
        public static double moving_sum_y = 0;

        public static void Filter()
        {
        }

        public static void ChangeAngle(KinectSensor kinect)
        {
            int elevation_angle = 0;

            if (locked_skeleton == true)
            {
                //for short person
                if (stick_coord[1] > 200)
                {
                    elevation_angle = kinect.ElevationAngle;
                    Log.Debug("going down : " + elevation_angle);
                    kinect.ElevationAngle = elevation_angle - 5;
                }
                    //for tall person
                else if (stick_coord[1] < 50)
                {
                    elevation_angle = kinect.ElevationAngle;
                    Log.Debug("going up : " + elevation_angle);
                    kinect.ElevationAngle = elevation_angle + 5;
                }
            }
        }

        //Decides whether to use skeletal tracking for hands or use depth analysis
        public static int ChooseHand(float[] coordinates, int depth)
        {
            int real_hand = 0;

            double min_depth = Convert.ToDouble(depth)/1000;

            if (((coordinates[19] - 0.1) > min_depth) &&
                ((coordinates[19] > coordinates[14]) | (coordinates[19] > coordinates[15])))
            {
                //left hand closer than right hand
                if (coordinates[14] < coordinates[15])
                {
                    real_hand = 1;
                }
                else
                {
                    real_hand = 1;
                }
            }
            else
            {
                if (coordinates[14] < coordinates[15])
                {
                    real_hand = 3;
                }
                else
                {
                    real_hand = 4;
                }
            }

            return real_hand;
        }
    }
}
