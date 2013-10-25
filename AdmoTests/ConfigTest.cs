using System;
using Admo.classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AdmoTests
{
    [TestClass]
    public class ConfigTest
    {
        [TestMethod]
        public void ConfigsConlineStatIsDeterminedFromPubnub()
        {
           var list =  Config.ParsePubnubConnection("[\"1\",\"message\",\"api_key\"]");
           Assert.AreEqual("1",list[0]);
        }
    }
}
