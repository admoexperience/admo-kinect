using System;
using AdmoShared.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdmoTests.Utilities
{
    [TestClass]
    public class UtilsTest
    {
        [TestMethod]
        public void TestWeCanGenerateACheckSumSha256()
        {
            var sha256 = Utils.Sha256("Resources\\dist-test.pod.zip");
            Assert.AreEqual("0434653354f39cec388c3772fb46accbf9a43ddc3dfa5f34eed8c6c0a06306e6", sha256);
        }

        [TestMethod]
        public void KinectAngleChange()
        {

            Assert.AreEqual(false, Utils.CheckifAngleCanChange(1, 1.1));
            Assert.AreEqual(true, Utils.CheckifAngleCanChange(2, 4));


        }
    }
}
