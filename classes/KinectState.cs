using System;

namespace Admo.classes
{
    public class KinectState
    {
        public Position LeftHand = new Position();
        public Position RightHand = new Position();
        public Position Head = new Position();
        public String HandState;
        public int Phase = 1;

        public void SetHead(int x, int y, int z)
        {
            Head.X = x;
            Head.Y = y;
            Head.Z = z;
        }

        public void SetLeftHand(int x, int y, int z)
        {
            LeftHand.X = x;
            LeftHand.Y = y;
            LeftHand.Z = z;
        }

        public void SetRightHand(int x, int y, int z)
        {
            RightHand.X = x;
            RightHand.Y = y;
            RightHand.Z = z;
        }
    }


    //Wrapper around storing x,y,z co-ords used for each object being sent in the kinnect state
    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }
}