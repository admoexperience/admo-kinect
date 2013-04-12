using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Threading;
using System.Windows.Threading;

namespace Admo
{
    class Gestures
    {

        public static double time_start = Convert.ToDouble(Convert.ToDouble(DateTime.Now.Ticks) / 10000);

        public static bool start_m1 = false;
        public static double start_multi_rx = 0;
        public static double start_multi_ry = 0;

        public static double delta_z = 0;
        public static double delta_tempx = 0;
        public static double delta_tempy = 0;
                
        public static bool swipe_stage1 = false;
        public static bool swipe_stage2 = false;

        public static void Multi_Swipe(float[] coordinates)
        {

            double delta_left_z = coordinates[18] - coordinates[14];
            double delta_right_z = coordinates[18] - coordinates[15];

            //height difference between left and right hand
            double hand_y_diff = coordinates[3] - coordinates[1];

            //distance from head to right hand on the Y-axis
            double delta_y = (coordinates[7]-coordinates[3] - 0.55);

            //distance from right hand to shoudler on the X-axis
            double delta_shoulder_x = Math.Abs(coordinates[10] - coordinates[2]);
            //distance from right hand to centrer spine in the Z-axis - equals 0 when right hand is 0.42 in front of spine
            delta_z = (0.2 + coordinates[15])-coordinates[18];                        

            //pause between events
            double time_now = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
            double time_delta = time_now - time_start;

            //&&(coordinates[3] > (coordinates[7]-0.4)) && (delta_right_z > 0.15)

            //stage 1: left hand is in position to activate multi gesture control panel
            if ((time_delta > 900) && (start_m1 == false) && /*(hand_y_diff>0.3) &&*/(delta_y < 0) && (delta_shoulder_x<0.05))
            {
                //stage1 - hand is middle
                //fade HUD in
                //HUD = push hand forward to swipe
                swipe_stage1 = true;

                    if (delta_z < 0.005)
                    {
                        //stage 2 - hand - is in swiping plane
                        //HUD = swipe hand in direction
                        swipe_stage2 = true;

                        start_m1 = true;
                        //set start coordinates (x:y)
                        start_multi_rx = coordinates[2];
                        start_multi_ry = coordinates[3];
                        delta_z = 0;
                    }

            }
            else if ((start_m1 == true) && (time_delta > 900)/* && (hand_y_diff > 0.3)*/)
            {

                double delta_tempxa = Math.Abs(start_multi_rx - coordinates[2]);
                double delta_tempya = Math.Abs(start_multi_ry - coordinates[3]);
                delta_tempx = (start_multi_rx - coordinates[2]);
                delta_tempy = (start_multi_ry - coordinates[3]);


                if (delta_tempx > 0.15)
                {
                    //swipe left
                    Console.WriteLine("Left");
                    start_m1 = false;
                    time_start = Convert.ToDouble(Convert.ToDouble(DateTime.Now.Ticks) / 10000);

                    SocketServer.Set_Gesture("swipeLeft");
                }
                else if (delta_tempx < -0.15)
                {
                    //swipe right
                    Console.WriteLine("Right");
                    start_m1 = false;
                    time_start = Convert.ToDouble(Convert.ToDouble(DateTime.Now.Ticks) / 10000);

                    SocketServer.Set_Gesture("swipeRight");
                }
                else if (/*(delta_tempxa > 0.25) && */(delta_tempya > 0.25))
                {
                    //out of bounds
                    start_m1 = false;
                    time_start = Convert.ToDouble(Convert.ToDouble(DateTime.Now.Ticks) / 10000);
                }
                
            }
            else
            {
                start_m1 = false;
                swipe_stage1 = false;
                swipe_stage2 = false;
            }


        }
        


    }
}
