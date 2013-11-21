using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Admo.classes;
using Admo;
using Microsoft.Kinect;

namespace AdmoTests
{
    [TestClass]
    public class BodyOutLineTests
    {
      [TestMethod]
        public void TestGetPixel()
      {
          var depthtest = new byte[307200];
          depthtest[300] = 1;
          var startPoint=ApplicationHandler.GetStartPixel(depthtest);
          Assert.AreEqual(0,startPoint[1]+startPoint[0]);

          depthtest = new byte[307200];
          startPoint = ApplicationHandler.GetStartPixel(depthtest);
          Assert.AreEqual(0,startPoint[1] + startPoint[0]);

      }


    }
}