using System;
using Admo.classes.lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdmoTests.classes.lib
{
    [TestClass]
    public class PushNotificationTest
    {
        [TestMethod]
        public void ParsingOfMessagesConnection()
        {
            var push = new PushNotification();
            var list = push.ParsePubnubConnection("[\"1\",\"message\",\"api_key\"]");
            Assert.AreEqual("1", list[0]);
        }

        [TestMethod]
        public void OnConnectionIsCalledCorrectly()
        {
            var response = false;
            //This is a hack to check if the method is being called and what the response is
            Action<Boolean> action = (result) =>response = result;
  
            var push = new PushNotification{
                OnConnection = action
            };
            push.OnPubnubConnection("[\"1\",\"message\",\"api_key\"]");
            Assert.AreEqual(true, response);

            push.OnPubnubConnection("[\"0\",\"message\",\"api_key\"]");
            Assert.AreEqual(false, response);
           
        }
    }
}
