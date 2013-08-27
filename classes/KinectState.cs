using System;

namespace Admo.classes
{
    public class KinectState
    {
        public Position LeftHand = new Position();
        public Position RightHand = new Position();
        public Position Head = new Position();
        public Position RightElbow = new Position();
        public Position LeftElbow = new Position();
        public String HandState;
        public int Phase = 1;

        public void SetHead(int x, int y, int z, float metricX, float metricY)
        {
            Head.X = x;
            Head.Y = y;
            Head.Z = z;
            Head.MetricX = metricX;
            Head.MetricY = metricY;
        }

        public void SetLeftHand(int x, int y, int z, float metricX, float metricY)
        {
            LeftHand.X = x;
            LeftHand.Y = y;
            LeftHand.Z = z;
            LeftHand.MetricX = metricX;
            LeftHand.MetricY = metricY;
        }

        public void SetRightHand(int x, int y, int z, float metricX, float metricY)
        {
            RightHand.X = x;
            RightHand.Y = y;
            RightHand.Z = z;
            RightHand.MetricX = metricX;
            RightHand.MetricY = metricY;
        }

        public void SetRightElbow(int x, int y, int z, float metricX, float metricY)
        {
            RightElbow.X = x;
            RightElbow.Y = y;
            RightElbow.Z = z;
            RightElbow.MetricX = metricX;
            RightElbow.MetricY = metricY;
        }

        public void SetLeftElbow(int x, int y, int z, float metricX, float metricY)
        {
            LeftElbow.X = x;
            LeftElbow.Y = y;
            LeftElbow.Z = z;
            LeftElbow.MetricX = metricX;
            LeftElbow.MetricY = metricY;
        }
    }


    //Wrapper around storing x,y,z co-ords used for each object being sent in the kinnect state
    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public float MetricX { get; set; }
        public float MetricY { get; set; }
    }
}