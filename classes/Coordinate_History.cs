/*This class uses queues to save coordinates's history.
 *This is done in order to see whether a get the value of coordinates x;y;z(current time-custom time).
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admo
{
    class Coordinate_History
    {       

        //filter x:y coordinates send to Traveller App
        public static Queue<float> x_filter_depth = new Queue<float>(3);
        public static Queue<float> y_filter_depth = new Queue<float>(3);
        public static float[] Filter_Depth(int x, int y)
        {            
            float x_average = 0;
            float y_average = 0;
            double count = 0;
            float[] array = new float[2];

            try
            {
                count = x_filter_depth.Count();
            }
            catch (Exception e)
            {
            }

            if (count < 3) //wait until queue is full
            {
                x_filter_depth.Enqueue(x);
                y_filter_depth.Enqueue(y);

                array[0] = 0;
                array[1] = 0;
            }
            else
            {
                x_filter_depth.Dequeue();
                y_filter_depth.Dequeue();

                x_filter_depth.Enqueue(x);
                y_filter_depth.Enqueue(y);

                float total_x = 0;
                float total_y = 0;

                foreach (float xa in x_filter_depth)
                {
                    total_x = total_x + xa;
                }

                foreach (float ya in y_filter_depth)
                {
                    total_y = total_y + ya;
                }

                x_average = total_x / 3;
                y_average = total_y/3;

                array[0] = x_average;
                array[1] = y_average;
            }

            return array;
        }

        //filter x:y coordinates
        public static int queue_length = 10;
        public static int speedpoints_length = 3;
        public static Queue<float> x_filter = new Queue<float>(queue_length);
        public static Queue<float> y_filter = new Queue<float>(queue_length);
        public static Queue<double> queue_test = new Queue<double>(queue_length);
        
        public static int speed_ratio = 0;
        public static int read_length = 7;
        public static int old_x = 0;
        public static int old_y = 0;
        public static float previous_x = 0;
        public static float previous_y = 0;
        public static void FilterCoordinates(int x, int y)
        {

            float x_average = 0;
            float y_average = 0;
            double count = 0;
            float[] array = new float[2];
            float[,] speed_points = new float[2, speedpoints_length];

            try
            {
                count = x_filter.Count();
            }
            catch (Exception e)
            {
            }

            if (count < queue_length) //wait until queue is full
            {
                x_filter.Enqueue(x);
                y_filter.Enqueue(y);

                array[0] = old_x;
                array[1] = old_y;

                previous_x = (x + old_x)/2;
                previous_y = (y + old_x)/2;
            }
            else
            {

                float temp_x = Math.Abs(x - previous_x);
                float temp_y = Math.Abs(y - previous_y);
                //check whether there is excessive noise
                if ((temp_x < 100) && (temp_y < 100))
                {
                    previous_x = x;
                    previous_y = y;

                    x_filter.Dequeue();
                    y_filter.Dequeue();

                    x_filter.Enqueue(x);
                    y_filter.Enqueue(y);

                    float total_x = 0;
                    float total_y = 0;

                    //to keep track of loop iterations
                    int loop_count_x = 0;
                    int loop_count_y = 0;

                    float sum_read_length = read_length * (read_length + 1) / 2;

                    //looping through the x queue
                    //Considering replacing with exponentially waited moving average less artifacts less memory requirments
                    //Later move to kalman filter integrate the multiple sensors
                    foreach (float xa in x_filter)
                    {
                        loop_count_x++;                        

                        //get the values used to calculate the speed ratio
                        if (loop_count_x > (queue_length - speedpoints_length))
                        {
                            speed_points[0, (loop_count_x - (1 + queue_length - speedpoints_length))] = xa;
                        }

                        //get the values used for the moving average filter
                        if (loop_count_x > (queue_length - read_length))
                        {
                            total_x = total_x + xa * ((float)(loop_count_x - (queue_length - read_length))) / sum_read_length;
                        }   
                    }                    

                    //looping through the y_queue
                    foreach (float ya in y_filter)
                    {
                        loop_count_y++;                        

                        //get the values used to calculate the speed ratio
                        if (loop_count_y > (queue_length - speedpoints_length))
                        {
                            speed_points[1, (loop_count_y - (1+queue_length - speedpoints_length))] = ya;
                        }

                        //get the values used for the moving average filter
                       
                        if (loop_count_y > (queue_length - read_length))
                        {
                            total_y = total_y + ya * ((float)(loop_count_y - (queue_length - read_length))) / sum_read_length;
                        } 
                    }
                    
                    x_average = total_x;
                    y_average = total_y;
                    //Console.WriteLine(total_x + "  .. " + read_length);

                    array[0] = x_average;
                    array[1] = y_average;

                    //calculate the speed of the hand using coordinate history
                    Calculate_SpeedRatio(speed_points);
                }
                //if there are excessive noise, use skeletal tracking values
                else
                {
                    array[0] = old_x;
                    array[1] = old_y;
                }
            }

            Application_Handler.stick_coord[4] = Convert.ToInt32(array[0]);
            Application_Handler.stick_coord[5] = Convert.ToInt32(array[1]);
        }

        public static double max_speed = 80;
        public static double max_read_length = 7;

        public static void Calculate_SpeedRatio(float[,] speed_points)
        {
            float[,] coordinates = new float[2, (speedpoints_length-1)];
            float sum_x = 0;
            float sum_y = 0;

            for (int t = 0; t < (speedpoints_length-1); t++)
            {
                coordinates[0, t] = speed_points[0, (t+1)] - speed_points[0, t];
                sum_x = sum_x + coordinates[0, t];

                coordinates[1, t] = speed_points[1, (t+1)] - speed_points[1, t];
                sum_y = sum_y + coordinates[1, t];
            }

            double speed = Math.Sqrt(sum_x * sum_x + sum_y * sum_y);

            double m = (1 - max_read_length) / max_speed;

            read_length = (int)(Math.Round((m * speed + max_read_length),0));
            if (read_length < 1)
                read_length = 1;

            //Console.WriteLine(read_length);
            //Console.WriteLine("........... " + speed);
        }
        
    }
}
