using System;
using System.Collections.Generic;
using Admo.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdmoTests.Utilities
{
    [TestClass]
    public class HardwareUtilsTest
    {
        [TestMethod]
        public void TestForKinectPresent()
        {
            //Kinect for Windows Audio Array Control
            //Kinect for Windows Camera
            //Kinect for Windows Security Control
            var list = new List<HardwareUtils.UsbDevice> {new HardwareUtils.UsbDevice(){Description = "mooo"}};
            Assert.IsFalse(HardwareUtils.IsWindowsKinectPresent(list), "Kinect should not be present");
            var kinectList = new List<HardwareUtils.UsbDevice> { new HardwareUtils.UsbDevice() { Description = "Kinect for Windows Security Control" } };
            Assert.IsTrue(HardwareUtils.IsWindowsKinectPresent(kinectList), "Kinect should be present");
        }
    }
}
