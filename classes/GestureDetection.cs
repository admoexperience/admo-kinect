using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Microsoft.Kinect;

namespace Admo.classes
{
    class GestureDetection
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public double SwipeDistanceInMeters = 0.3;
        public const double SwipeDeltaY = 0.05;
        public const double SwipeHeight = 0.5;
        public bool SwipeInDeltaY = false;
        public const double SwipeTimeInFrames = 10; // 10 * 30ms (Kinect Framerate) = 300ms - time allowed to copmlete a swipe gesture

        

        public static int QueueLength = 20; // 20 * 30ms (Kinect Framerate) = 600ms - coordinates for the last 600ms are recorded and inspected for a swipe gesture
        public Queue<JointCollection> CoordinateHistory = new Queue<JointCollection>(QueueLength);

        public double TimeSwipeCompleted = LifeCycle.GetCurrentTimeInSeconds();
        public double SwipeWaitTime = 1.2;
        public bool SwipeReady = true;

        public JointCollection StartCoordinates;
        public JointCollection EndCoordinates;
        public float SwipeEndX = 0;
        public float SwipePreviousX = -999;
        public double PreviousMove = 0.2;
        public bool MovedFromPreviousArea = false;

        //manage gestures
        public void GestureHandler(JointCollection coordinates, JointType hand)
        {
 
            var count = 0;
          

            SwipeTimeout();

            try
            {
                
                count = CoordinateHistory.Count();
            }
            catch (Exception)
            {
                //TODO: Fix this, catch with empty block is BAD
                //FAIL hard and fail fast this should never cause an error in theory
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
                SwipeEndX = coordinates[hand].Position.X;

                int timeLoop = 0;

                foreach (JointCollection coord in CoordinateHistory.Reverse())
                {
                    timeLoop++;
                    StartCoordinates = coord;
                    double swipeDiff = (StartCoordinates[hand].Position.X - EndCoordinates[hand].Position.X);

                    //checks to see if user is swiping in deltaY relative center to shoulderY
                    double swipeDeltaY = Math.Abs(coord[hand].Position.Y - coordinates[hand].Position.Y);
                    double swipeHeadY = Math.Abs(coord[hand].Position.Y - coord[JointType.Head].Position.Y);

                    if ((swipeDeltaY > SwipeDeltaY) || (swipeHeadY > SwipeHeight))
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
                            if (swipeDiff < 0)
                            {
                                OnGestureDetected("SwipeToRight");
                            }
                            else
                            {
                                OnGestureDetected("SwipeToLeft");
                            }

                            MovedFromPreviousArea = false;
                            SwipePreviousX = coordinates[hand].Position.X;
                            TimeSwipeCompleted = LifeCycle.GetCurrentTimeInSeconds();

                            break;
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
            var currentTime = LifeCycle.GetCurrentTimeInSeconds();
            var timeSinceSwipe = currentTime - TimeSwipeCompleted;

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
