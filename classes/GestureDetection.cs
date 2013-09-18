using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

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
        public Queue<float[]> CoordinateHistory = new Queue<float[]>(QueueLength);

        public double TimeSwipeCompleted = LifeCycle.GetCurrentTimeInSeconds();
        public double SwipeWaitTime = 1.2;
        public bool SwipeReady = true;

        public float[] StartCoordinates = new float[24];
        public float[] EndCoordinates = new float[24];
        public float SwipeEndX = 0;
        public float SwipePreviousX = -999;
        public double PreviousMove = 0.2;
        public bool MovedFromPreviousArea = false;

        //manage gestures
        public void GestureHandler(float[] coordinates, BodyPart hand)
        {
 
            var count = 0;
            var handX = BodyCoordinates.LeftHandX;
            var handY = BodyCoordinates.LeftHandY;

            if (hand == BodyPart.LeftHand)
            {
                handX = BodyCoordinates.LeftHandX;
                handY = BodyCoordinates.LeftHandY;
            }
            else if (hand == BodyPart.RightHand)
            {
                handX = BodyCoordinates.RightHandX;
                handY = BodyCoordinates.RightHandY;
            }

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
                SwipeEndX = coordinates[handX];

                int timeLoop = 0;

                foreach (float[] coord in CoordinateHistory.Reverse())
                {
                    timeLoop++;
                    StartCoordinates = coord;
                    double swipeDiff = (StartCoordinates[handX] - EndCoordinates[handX]);

                    //checks to see if user is swiping in deltaY relative center to shoulderY
                    double swipeDeltaY = Math.Abs(coord[handY] - coordinates[handY]);
                    double swipeHeadY = Math.Abs(coord[handY] - coord[BodyCoordinates.HeadY]);
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
                            //TODO this shouldn't be here else every time some one uses the app it is going to push to papertrail
                            Logger.Debug("Swipe!");
                            if (swipeDiff < 0)
                            {
                                OnGestureDetected("SwipeToRight");
                            }
                            else
                            {
                                OnGestureDetected("SwipeToLeft");
                            }

                            MovedFromPreviousArea = false;
                            SwipePreviousX = coordinates[handX];
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
