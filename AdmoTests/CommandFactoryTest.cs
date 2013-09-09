using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Admo.classes.lib;
using Admo.classes.lib.commands;
namespace AdmoTests
{
    [TestClass]
    public class CommandFactoryTest
    {
        [TestMethod]
        public void TestComamndFactoryParsingUnknown()
        {
            TestParse("doesntmatter", typeof(UnknownCommand));
        }

        [TestMethod]
        public void TestParsingScrenshot()
        {
            TestParse("screenshot", typeof(ScreenshotCommand));
        }

        [TestMethod]
        public void TestParsingCheckin()
        {
            TestParse("checkin", typeof(CheckinCommand));
        }

        [TestMethod]
        public void TestParsinCalibrate()
        {
            TestParse("calibrate", typeof(CalibrateCommand));
        }

        [TestMethod]
        public void TestParsingUpdateConfig()
        {
            TestParse("updateConfig", typeof(UpdateConfigCommand));
        }


        private void TestParse(String command, Type t)
        {
            var jsonCmd = createJsonCommand(command);
            var x = CommandFactory.ParseCommand(jsonCmd);
            Assert.AreEqual(t, x.GetType());
        }

        private static String createJsonCommand(String command)
        {
            var dict = new Dictionary<string, string>()
            {
                {"command",command}
            };
            return JsonConvert.SerializeObject(dict);
        }
    }
}
