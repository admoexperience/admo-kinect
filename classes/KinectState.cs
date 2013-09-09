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

        public void SetHead(int x, int y, int z, float mmX, float mmY)
        {
            Head.X = x;
            Head.Y = y;
            Head.Z = z;
            Head.mmX = mmX;
            Head.mmY = mmY;
        }

        public void SetLeftHand(int x, int y, int z, float mmX, float mmY)
        {
            LeftHand.X = x;
            LeftHand.Y = y;
            LeftHand.Z = z;
            LeftHand.mmX = mmX;
            LeftHand.mmY = mmY;
        }

        public void SetRightHand(int x, int y, int z, float mmX, float mmY)
        {
            RightHand.X = x;
            RightHand.Y = y;
            RightHand.Z = z;
            RightHand.mmX = mmX;
            RightHand.mmY = mmY;
        }

        public void SetRightElbow(int x, int y, int z, float mmX, float mmY)
        {
            RightElbow.X = x;
            RightElbow.Y = y;
            RightElbow.Z = z;
            RightElbow.mmX = mmX;
            RightElbow.mmY = mmY;
        }

        public void SetLeftElbow(int x, int y, int z, float mmX, float mmY)
        {
            LeftElbow.X = x;
            LeftElbow.Y = y;
            LeftElbow.Z = z;
            LeftElbow.mmX = mmX;
            LeftElbow.mmY = mmY;
        }
    }


    //Wrapper around storing x,y,z co-ords used for each object being sent in the kinnect state
    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public float mmX { get; set; }
        public float mmY { get; set; }
    }
}