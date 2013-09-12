using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admo.classes
{
    class GestureDetection
    {
        public static double SwipeDistanceInMeters = 0.3;
        public static double SwipeDeltaY = 0.05;
        public static double SwipeHeight = 0.175;
        public static bool SwipeInDeltaY = false;
        public static double SwipeTimeInFrames = 10; // 10 * 30ms (Kinect Framerate) = 300ms - time allowed to copmlete a swipe gesture

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
        public static double SwipeWaitTime = 1.2;
        public static bool SwipeReady = true;

        public static float[] StartCoordinates = new float[24];
        public static float[] EndCoordinates = new float[24];
        public static float SwipeEndX = 0;
        public static float SwipePreviousX = -999;
        public static double PreviousMove = 0.2;
        public static bool MovedFromPreviousArea = false;

        //manage gestures
        public  void GestureHandler(float[] coordinates)
        {
 
            int count = 0;

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
                SwipeEndX = coordinates[RightHandX];

                int timeLoop = 0;

                foreach (float[] coord in CoordinateHistory.Reverse())
                {
                    timeLoop++;
                    StartCoordinates = coord;
                    double swipeDiff = (StartCoordinates[RightHandX] - EndCoordinates[RightHandX]);

                    //checks to see if user is swiping in deltaY relative center to shoulderY
                    double swipeDeltaY = Math.Abs(coord[RightHandY] - coordinates[RightHandY]);
                    double swipeShoulderY = Math.Abs(coord[RightHandY] - coord[RightShoulderY]);
                    if ((swipeDeltaY > SwipeDeltaY) || (swipeShoulderY > SwipeHeight))
                    {
                        SwipeInDeltaY = false;
                        break;
                    }

                    //checks to see if hand is still in position it ended up with the previous swipe
                    double previousDelta = Math.Abs(SwipePreviousX - SwipeEndX);
                    //Console.WriteLine(previousDelta);
                    if (previousDelta > PreviousMove)
                    {
                        MovedFromPreviousArea = true;
                    }

                    if (!SwipeReady)
                    {
                        PreviousMove = SwipeDistanceInMeters = 0.35;
                    }
                    else
                    {
                        PreviousMove = SwipeDistanceInMeters = 0.2;
                    }


                    if ((Math.Abs(swipeDiff) > SwipeDistanceInMeters) && (timeLoop < SwipeTimeInFrames) && (MovedFromPreviousArea))
                    {
                        if (true)
                        {
                            Console.WriteLine("Swipe!");
                            if (swipeDiff < 0)
                            {
                                OnGestureDetected("SwipeToRight");
                            }
                            else
                            {
                                OnGestureDetected("SwipeToLeft");
                            }

                            MovedFromPreviousArea = false;
                            SwipePreviousX = coordinates[RightHandX];
                            TimeSwipeCompleted = LifeCycle.GetCurrentTimeInSeconds();

                            break;
                        }

                    }
                    
                    
                }

            }
        }

        //handles swipe gesture event
        public void OnGestureDetected(string gesture)
        {
            SocketServer.SendGestureEvent(gesture);
        }

        private  void SwipeTimeout()
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
