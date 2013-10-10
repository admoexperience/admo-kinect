using System.Threading;
using Admo.classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdmoTests
{
    [TestClass]
    public class GestureDetectionTests
    {
        [TestMethod]
        public void SwipeToRight()
        {
            var handhead = new HandHead
                {
                    HandX = -0.22f,
                    HandY = -0.05f,
                    HeadY = 0.1f
                };
            var gestureDetector = new GestureDetection();
            string myString = "";
            myString = myString + gestureDetector.DetectSwipe(handhead);


            for (var i = 0; i < 20; i++)
            {
                handhead.HandX = handhead.HandX + 0.03f;
                handhead.HandY = handhead.HandY - 0.005f;
                handhead.HeadY = handhead.HeadY + 0.001f;
                myString = myString + gestureDetector.DetectSwipe(handhead);
            }


            Assert.AreEqual("SwipeToRight", myString);
        }


        [TestMethod]
        public void SwipeToLeft()
        {
            var handhead = new HandHead
                {
                    HandX = 0.22f,
                    HandY = -0.05f,
                    HeadY = 0.1f
                };
            var gestureDetector = new GestureDetection();
            string myString = "";
            myString = myString + gestureDetector.DetectSwipe(handhead);

            for (var i = 0; i < 20; i++)
            {
                handhead.HandX = handhead.HandX - 0.03f;
                handhead.HandY = handhead.HandY - 0.005f;
                handhead.HeadY = handhead.HeadY + 0.001f;
                myString = myString + gestureDetector.DetectSwipe(handhead);
            }


            Assert.AreEqual("SwipeToLeft", myString);
        }
    }
}
