/*This class uses queues to save coordinates's history.
 *This is done in order to see whether a get the value of coordinates x;y;z(current time-custom time).
 */
using System;
using System.Collections.Generic;
using System.Linq;


namespace Admo
{
    class Coordinate_History
    {       

        //filter x:y coordinates send to Traveller App
        public static Queue<float> XFilterDepth = new Queue<float>(3);
        public static Queue<float> YFilterDepth = new Queue<float>(3);
        public static float[] Filter_Depth(int x, int y)
        {
            double count = 0;
            float[] array = new float[2];

            try
            {
                count = XFilterDepth.Count();
            }
            catch (Exception e)
            {
            }

            if (count < 3) //wait until queue is full
            {
                XFilterDepth.Enqueue(x);
                YFilterDepth.Enqueue(y);

                array[0] = 0;
                array[1] = 0;
            }
            else
            {
                XFilterDepth.Dequeue();
                YFilterDepth.Dequeue();

                XFilterDepth.Enqueue(x);
                YFilterDepth.Enqueue(y);

                float totalX = 0;
                float totalY = 0;

                foreach (float xa in XFilterDepth)
                {
                    totalX = totalX + xa;
                }

                foreach (float ya in YFilterDepth)
                {
                    totalY = totalY + ya;
                }

                float xAverage = totalX / 3;
                float yAverage = totalY/3;

                array[0] = xAverage;
                array[1] = yAverage;
            }

            return array;
        }

        //filter x:y coordinates
        public static int QueueLength = 10;
        public static int SpeedpointsLength = 3;
        public static Queue<float> XFilter = new Queue<float>(QueueLength);
        public static Queue<float> YFilter = new Queue<float>(QueueLength);
        public static Queue<double> QueueTest = new Queue<double>(QueueLength);
        
        public static int SpeedRatio = 0;
        public static int ReadLength = 7;
        public static int OldX = 0;
        public static int OldY = 0;
        public static float PreviousX = 0;
        public static float PreviousY = 0;
        public static void FilterCoordinates(int x, int y)
        {
            double count = 0;
            float[] array = new float[2];
            float[,] speedPoints = new float[2, SpeedpointsLength];

            try
            {
                count = XFilter.Count();
            }
            catch (Exception e)
            {
            }

            if (count < QueueLength) //wait until queue is full
            {
                XFilter.Enqueue(x);
                YFilter.Enqueue(y);

                array[0] = OldX;
                array[1] = OldY;

                PreviousX = (x + OldX)/2;
                PreviousY = (y + OldX)/2;
            }
            else
            {

                float tempX = Math.Abs(x - PreviousX);
                float tempY = Math.Abs(y - PreviousY);
                //check whether there is excessive noise
                if ((tempX < 100) && (tempY < 100))
                {
                    PreviousX = x;
                    PreviousY = y;

                    XFilter.Dequeue();
                    YFilter.Dequeue();

                    XFilter.Enqueue(x);
                    YFilter.Enqueue(y);

                    float totalX = 0;
                    float totalY = 0;

                    //to keep track of loop iterations
                    int loopCountX = 0;
                    int loopCountY = 0;

                    float sumReadLength = ReadLength * (ReadLength + 1) / 2;

                    //looping through the x queue
                    //Considering replacing with exponentially waited moving average less artifacts less memory requirments
                    //Later move to kalman filter integrate the multiple sensors
                    foreach (float xa in XFilter)
                    {
                        loopCountX++;                        

                        //get the values used to calculate the speed ratio
                        if (loopCountX > (QueueLength - SpeedpointsLength))
                        {
                            speedPoints[0, (loopCountX - (1 + QueueLength - SpeedpointsLength))] = xa;
                        }

                        //get the values used for the moving average filter
                        if (loopCountX > (QueueLength - ReadLength))
                        {
                            totalX = totalX + xa * ((float)(loopCountX - (QueueLength - ReadLength))) / sumReadLength;
                        }   
                    }                    

                    //looping through the y_queue
                    foreach (float ya in YFilter)
                    {
                        loopCountY++;                        

                        //get the values used to calculate the speed ratio
                        if (loopCountY > (QueueLength - SpeedpointsLength))
                        {
                            speedPoints[1, (loopCountY - (1+QueueLength - SpeedpointsLength))] = ya;
                        }

                        //get the values used for the moving average filter
                       
                        if (loopCountY > (QueueLength - ReadLength))
                        {
                            totalY = totalY + ya * ((float)(loopCountY - (QueueLength - ReadLength))) / sumReadLength;
                        } 
                    }
                    
                    float xAverage = totalX;
                    float yAverage = totalY;
                    //Console.WriteLine(total_x + "  .. " + read_length);

                    array[0] = xAverage;
                    array[1] = yAverage;

                    //calculate the speed of the hand using coordinate history
                    Calculate_SpeedRatio(speedPoints);
                }
                //if there are excessive noise, use skeletal tracking values
                else
                {
                    array[0] = OldX;
                    array[1] = OldY;
                }
            }

            Application_Handler.StickCoord[4] = Convert.ToInt32(array[0]);
            Application_Handler.StickCoord[5] = Convert.ToInt32(array[1]);
        }

        public static double MaxSpeed = 80;
        public static double MaxReadLength = 7;

        public static void Calculate_SpeedRatio(float[,] speedPoints)
        {
            float[,] coordinates = new float[2, (SpeedpointsLength-1)];
            float sumX = 0;
            float sumY = 0;

            for (int t = 0; t < (SpeedpointsLength-1); t++)
            {
                coordinates[0, t] = speedPoints[0, (t+1)] - speedPoints[0, t];
                sumX = sumX + coordinates[0, t];

                coordinates[1, t] = speedPoints[1, (t+1)] - speedPoints[1, t];
                sumY = sumY + coordinates[1, t];
            }

            double speed = Math.Sqrt(sumX * sumX + sumY * sumY);

            double m = (1 - MaxReadLength) / MaxSpeed;

            ReadLength = (int)(Math.Round((m * speed + MaxReadLength),0));
            if (ReadLength < 1)
                ReadLength = 1;

            //Console.WriteLine(read_length);
            //Console.WriteLine("........... " + speed);
        }
        
    }
}
