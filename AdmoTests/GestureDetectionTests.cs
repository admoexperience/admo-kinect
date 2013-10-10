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
                    HandX = (float) -0.22,
                    HandY = -(float) -0.05,
                    HeadY = (float) 0.1
                };
            var gestureDetector = new GestureDetection();
            string myString = "";

            for (int i = 1; i <= 21; i++)
            {
                handhead.HandX = handhead.HandX + (float) 0.03;
                handhead.HandY = handhead.HandY - (float) 0.005;
                handhead.HeadY = handhead.HeadY + (float) 0.001;
                myString = myString + gestureDetector.DetectSwipe(handhead);
            }


            Assert.AreEqual("SwipeToRight", myString);
        }
    }
}
