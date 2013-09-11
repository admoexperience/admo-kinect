using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admo.classes
{
    class GestureDetection
    {
        public static double SwipeDistance = 0.2;
        public static double SwipeDeltaY = 0.05;
        public static double SwipeHeight = 0.175;
        public static bool SwipeInDeltaY = false;
        public static double SwipeTime = 10; // 10 * 30ms (Kinect Framerate) = 300ms - time allowed to copmlete a swipe gesture

        public static int LeftHandX = 0;
        public static int LeftHandY = 1;
        public static int RightHandX = 2;
        public static int RightHandY = 3;
        public static int LeftShoulderX = 8;
        public static int LeftShoulderY = 9;
        public static int RightShoulderX = 10;
        public static int RightShoulderY = 11;
        public static int HeadX = 6;
        public static int HeadY = 7;

        public static int QueueLength = 20; // 20 * 30ms (Kinect Framerate) = 600ms - coordinates for the last 600ms are recorded and inspected for a swipe gesture
        public static Queue<float[]> CoordinateHistory = new Queue<float[]>(QueueLength);

        public static double TimeSwipeCompleted = LifeCycle.GetCurrentTimeInSeconds();
        public static double SwipeWaitTime = 1.5;
        public static bool SwipeReady = true;

        public static float[] StartCoordinates = new float[24];
        public static float[] EndCoordinates = new float[24];

        //manage gestures
        public static void GestureHandler(float[] coordinates)
        {
 
            int count = 0;
            double swipeDiff = 0;

            SwipeTimeout();

            try
            {
                count = CoordinateHistory.Count();
            }
            catch (Exception e)
            {
            }

            if (count < QueueLength) //wait until queue is full
            {
                CoordinateHistory.Enqueue(coordinates);
            }
            else
            {
                var oldCoordinates = CoordinateHistory.Dequeue();
                CoordinateHistory.Enqueue(coordinates);

                EndCoordinates = coordinates;

                int timeLoop = 0;

                foreach (float[] coord in CoordinateHistory.Reverse())
                {
                    timeLoop++;
                    StartCoordinates = coord;
                    swipeDiff = (StartCoordinates[RightHandX] - EndCoordinates[RightHandX]);

                    //checks to see if user is swiping in deltaY relative center to shoulderY
                    double swipeDeltaY = Math.Abs(coord[RightHandY] - coordinates[RightHandY]);
                    double swipeShoulderY = Math.Abs(coord[RightHandY] - coord[RightShoulderY]);
                    if ((swipeDeltaY > SwipeDeltaY) || (swipeShoulderY > SwipeHeight))
                    {
                        SwipeInDeltaY = false;
                        break;
                    }

                    if ((Math.Abs(swipeDiff) > SwipeDistance) && (timeLoop < SwipeTime))
                    {
                        if (SwipeReady)
                        {
                            if (swipeDiff < 0)
                            {
                                OnGestureDetected("SwipeToRight");
                            }
                            else
                            {
                                OnGestureDetected("SwipeToLeft");
                            }
                            TimeSwipeCompleted = LifeCycle.GetCurrentTimeInSeconds();
                            break;
                        }

                    }
                    
                    
                }

            }
        }

        //handles swipe gesture event
        public static void OnGestureDetected(string gesture)
        {
            SocketServer.SendGestureEvent(gesture);
        }

        public static void SwipeTimeout()
        {
            double currentTime = LifeCycle.GetCurrentTimeInSeconds();
            double timeSinceSwipe = currentTime - TimeSwipeCompleted;

            if (timeSinceSwipe < SwipeWaitTime)
            {
                SwipeReady = false;
            }
            else
            {
                SwipeReady = true;
            }
        }


    }
}
