using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Admo.classes;
using Admo;

namespace AdmoTests
{
    [TestClass]
    public class TestAlgorithms
    {
        [TestMethod]
        public void TestMethod1()
        {

            var myAv = ApplicationHandler.ExponentialWheightedMovingAverage((float)10.0, (float)1.0, (float)0.5);

            Assert.IsTrue(Math.Abs(myAv - 5.5) < 0.000001);
        }
    }
}
