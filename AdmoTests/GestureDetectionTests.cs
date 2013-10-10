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
                    HandY =  -0.05f,
                    HeadY =  0.1f
                };
            var gestureDetector = new GestureDetection();
            var myString = "";
            myString = myString + gestureDetector.DetectSwipe(handhead);
            System.Threading.Thread.Sleep(400);

            for (var i = 0; i < 20; i++)
            {
                handhead.HandX = handhead.HandX +  0.03f;
                handhead.HandY = handhead.HandY -  0.005f;
                handhead.HeadY = handhead.HeadY + 0.001f;
                System.Threading.Thread.Sleep(40);
                myString = myString + gestureDetector.DetectSwipe(handhead);
            }


            Assert.AreEqual("SwipeToRight", myString);
        }
    }
}
