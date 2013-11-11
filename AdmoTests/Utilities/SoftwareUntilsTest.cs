using System;
using Admo.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdmoTests.Utilities
{
    [TestClass]
    public class SoftwareUntilsTest
    {
        [TestMethod]
        public void TestChromeIsInstalledAndHasVersion()
        {
            //This test returns chromes version
            //Returns String.empty if chrome can't be found
            //This unit test requires chrome to be installed.
            //not sure this should be an actual unit test
            var x = SoftwareUtils.GetChromeVersion();
            Assert.IsFalse(String.IsNullOrEmpty(x));
        }
    }
}
