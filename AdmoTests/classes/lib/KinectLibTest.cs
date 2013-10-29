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
            Assert.AreSame(KinectLib.VideoAppSmoothingParams,type);
        }

        [TestMethod]
        public void AvatarSmoothingParams()
        {
            var klib = new KinectLib();
            var type = klib.GetTransformSmoothParameters("avatar");
            Assert.AreSame(KinectLib.AvatarAppSmoothingParams, type);
        }

        [TestMethod]
        public void VideoFeedSmoothingParams()
        {
            //Just incase we change it later
            var klib = new KinectLib();
            var type = klib.GetTransformSmoothParameters("video_feed");
            Assert.AreSame(KinectLib.VideoAppSmoothingParams, type);
        }
    }
}
