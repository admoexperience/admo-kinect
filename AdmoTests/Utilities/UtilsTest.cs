using System;
using Admo.Utilities;
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
    }
}
