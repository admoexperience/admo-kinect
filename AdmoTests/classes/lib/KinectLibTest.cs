using System;
using Admo.classes.lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdmoTests.classes.lib
{
    [TestClass]
    public class KinectLibTest
    {
        [TestMethod]
        public void DefaultSmoothingParams()
        {
            var klib = new KinectLib();
            var type = klib.GetTransformSmoothParameters("doestmatter");
            Assert.AreEqual(KinectLib.AvatarAppSmoothingParams,type);
        }

        [TestMethod]
        public void AvatarSmoothingParams()
        {
            var klib = new KinectLib();
            var type = klib.GetTransformSmoothParameters("cursor");
            Assert.AreEqual(KinectLib.CursorAppSmoothingParams, type);
        }

        [TestMethod]
        public void VideoFeedSmoothingParams()
        {
            //Just incase we change it later
            var klib = new KinectLib();
            var type = klib.GetTransformSmoothParameters("avatar");
            Assert.AreEqual(KinectLib.AvatarAppSmoothingParams, type);
        }
    }
}
