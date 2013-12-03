using System;
using System.Collections.Generic;
using System.Linq;
using Admo.Utilities;
using NLog;

namespace Admo.classes
{
    public struct HandHead
    {
        public HandHead(float handX, float handY, float headY)
        {
            HandX = handX;
            HandY = handY;
            HeadY = headY;
        }

        public float HandX;
        public float HandY;
        public float HeadY;
    }

    public class GestureDetection
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public const String SwipeRightGesture = "SwipeToRight";
        public const String SwipeLeftGesture = "SwipeToLeft";

        public double SwipeDistanceInMeters = 0.3;
        private const double SwipeDeltaY = 0.075;
        private const double SwipeHeight = 0.6;
        private bool SwipeInDeltaY = false;

        private const double SwipeTimeInFrames = 10;
        // 10 * 30ms (Kinect Framerate) = 300ms - time allowed to copmlete a swipe gesture

        private const int QueueLength = 20;
        // 20 * 30ms (Kinect Framerate) = 600ms - coordinates for the last 600ms are recorded and inspected for a swipe gesture

        private readonly Queue<HandHead> CoordHist = new Queue<HandHead>(QueueLength);

        private double _timeSwipeCompleted = Utils.GetCurrentTimeInSeconds();
        private const double FramesWaitTime = 36;
        private bool _swipeReady = true;

        private float _swipeEndX = 0;
        private float _swipePreviousX = -999;
        private double _previousMove = 0.2;
        private bool _movedFromPreviousArea = false;

        //manage gestures
        /// <summary>
        /// Performs a exponential wheigthed moving average xhat(k)=x(k)*alpha+x(k-1)*(1-alpha)
        /// </summary>
        /// <param name="mycoords">Struct containing the values nescessary to perform swipe detection</param>
        /// <returns >String containing SwipeToRight or SwipeToLeft or empty</returns>
        public string DetectSwipe(HandHead mycoords)
        {
            int count = 0;

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


                HandHead endCoordinates = mycoords;
                _swipeEndX = mycoords.HandX;

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
                    double previousDelta = Math.Abs(_swipePreviousX - _swipeEndX);
                    //Console.WriteLine(previousDelta);
                    if (previousDelta > _previousMove)
                    {
                        _movedFromPreviousArea = true;
                    }

                    if (!_swipeReady)
                    {
                        _previousMove = SwipeDistanceInMeters = 0.35;
                    }
                    else
                    {
                        _previousMove = SwipeDistanceInMeters = 0.2;
                    }

                    if ((Math.Abs(swipeDiff) > SwipeDistanceInMeters) && (timeLoop < SwipeTimeInFrames) &&
                        (_movedFromPreviousArea))
                    {
                        _movedFromPreviousArea = false;
                        _swipePreviousX = mycoords.HandX;
                        _timeSwipeCompleted = 0;

                        if (swipeDiff < 0)
                        {
                            return "SwipeToRight";
                        }
                        return "SwipeToLeft";
                    }
                    _timeSwipeCompleted++;
                }
            }

            return "";
        }

        private void SwipeTimeout()
        {
            _swipeReady = !(_timeSwipeCompleted < FramesWaitTime);
        }
    }
}