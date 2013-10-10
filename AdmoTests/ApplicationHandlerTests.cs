using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Admo.classes;
using Admo;
using Microsoft.Kinect;

namespace AdmoTests
{
    [TestClass]
    public class ApplicationHandlerTests
    {
        [TestMethod]
        public void ExponeitialMovingAverage()
        {

            var myAv = ApplicationHandler.ExponentialWheightedMovingAverage((float)10.0, (float)1.0, (float)0.5);

            Assert.IsTrue(Math.Abs(myAv - 5.5) < 0.000001);
        }

        [TestMethod]
        public void SkelePointFilter()
        {
            var point = new SkeletonPoint
            {
                X = (float)10,
                Y = (float)10,
                Z = (float)10
            };

            var fillpoint = new SkeletonPoint
            {
                X = (float)1,
                Y = (float)1,
                Z = (float)1
            };

            var newPoint = ApplicationHandler.FilterPoint(point, fillpoint, (float) 0.5);

            var test = new SkeletonPoint
            {
                X = (float)5.5,
                Y = (float)5.5,
                Z = (float)5.5
            };

            Assert.AreEqual(test,newPoint);
        }
        
        [TestMethod]
        public void Scaling()
        {

            //test will break if default scaling parameters change
            var point = new SkeletonPoint
                {
                    X = (float)0.294321,
                    Y = (float)0.134594,
                    Z = (float)0.6999
                };

            var colorpoint = new ColorImagePoint
                {
                    X = 555,
                    Y = 132,           
                };

            var newPos = ApplicationHandler.ScaleCoordinates(point, colorpoint);

            var checkPos = new Position
                {
                    X = 640,
                    Xmm = (float)0.294321,
                    Y = 99,
                    Ymm = (float)0.13459,
                    Z = 699
                };
            Assert.AreEqual(newPos.X, checkPos.X);
            Assert.AreEqual(newPos.Y, checkPos.Y);
            Assert.AreEqual(newPos.Z, checkPos.Z);


        }
        [TestMethod]
        public void FindPlayer()
        {

            var myAppHandler = new ApplicationHandler();
            var myState = myAppHandler.FindPlayer(
                new short[307200], 480, 640);
            var defaultState = new KinectState();
            Assert.AreEqual(myState.Head.X,defaultState.Head.X);

        }
    }
}
