using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Microsoft.Kinect;

namespace Admo.classes
{
    public class HandHead
    {
        public HandHead(float a, float b, float c)
        {
            HandX = a;
            HandY = b;
            HeadY = c;
        }
        public float HandX = 0;
        public float HandY = 0;
        public float HeadY = 0;
    }
    class GestureDetection
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public double SwipeDistanceInMeters = 0.3;
        public const double SwipeDeltaY = 0.075;
        public const double SwipeHeight = 0.6;
        public bool SwipeInDeltaY = false;
        public const double SwipeTimeInFrames = 10; // 10 * 30ms (Kinect Framerate) = 300ms - time allowed to copmlete a swipe gesture

        public static int QueueLength = 20; // 20 * 30ms (Kinect Framerate) = 600ms - coordinates for the last 600ms are recorded and inspected for a swipe gesture

        public Queue<HandHead> CoordHist=new Queue<HandHead>(QueueLength); 

        public double TimeSwipeCompleted = LifeCycle.GetCurrentTimeInSeconds();
        public double SwipeWaitTime = 1.2;
        public bool SwipeReady = true;

        public float SwipeEndX = 0;
        public float SwipePreviousX = -999;
        public double PreviousMove = 0.2;
        public bool MovedFromPreviousArea = false;

        //manage gestures
        public string GestureHandler(HandHead mycoords)
        {
 
            var count = 0;
          
            SwipeTimeout();

            count = CoordHist.Count();


            if (count < QueueLength) //wait until queue is full
            {
                CoordHist.Enqueue(mycoords);
            }
            else
            {
                CoordHist.Dequeue();

                CoordHist.Enqueue(mycoords);


                var endCoordinates = mycoords;
                SwipeEndX = mycoords.HandX;

                int timeLoop = 0;

                foreach (HandHead coord in CoordHist.Reverse())
                {
                    timeLoop++;

                    double swipeDiff = (coord.HandX - endCoordinates.HandX);

                    //checks to see if user is swiping in deltaY relative center to shoulderY
                    double swipeDeltaY = Math.Abs(coord.HandY - endCoordinates.HandY);
                    double swipeHeadY = Math.Abs(coord.HandY - coord.HeadY);

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

                    if ((Math.Abs(swipeDiff) > SwipeDistanceInMeters) && (timeLoop < SwipeTimeInFrames) &&
                        (MovedFromPreviousArea))
                    {

                        MovedFromPreviousArea = false;
                        SwipePreviousX = mycoords.HandX;
                        TimeSwipeCompleted = LifeCycle.GetCurrentTimeInSeconds();

                        if (swipeDiff < 0)
                        {
                            return "SwipeToRight";
                        }
                        return "SwipeToLeft";

                    }

                }

            }

            return "";
        }

        private  void SwipeTimeout()
        {
            var currentTime = LifeCycle.GetCurrentTimeInSeconds();
            var timeSinceSwipe = currentTime - TimeSwipeCompleted;

            SwipeReady = !(timeSinceSwipe < SwipeWaitTime);
        }

        
    }
}
