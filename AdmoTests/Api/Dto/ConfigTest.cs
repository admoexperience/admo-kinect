using System;
using Admo.Api.Dto;
using AdmoShared.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdmoTests.Api.Dto
{
    [TestClass]
    public class ConfigTest
    {
        [TestMethod]
        public void TestParsingApiJsonToObject()
        {
            const String configJson = @"{""config"":
                {""pubnub_subscribe_key"":""subkey"",
                ""web_ui_server"":""https://localhost:4001"",
                ""screenshot_interval"":60000,
                ""kinect_elevation"":""9"",
                ""environment"":""development"",
                ""pod_file"":""C://pods/dist-static-image.pod.zip"",
                ""calibration_active"":true,
                ""name"":""TestUnit"",
                ""analytics"":{
                    ""mixpanel_api_key"":""mixpanel_api_key"",
                    ""mixpanel_api_token"":""mixpanel_api_token""}
                    }
                }";
            var conf = JsonHelper.ConvertFromApiRequest<Config>(configJson);
            Assert.AreEqual("subkey", conf.PubnubSubscribeKey);
            Assert.AreEqual("https://localhost:4001", conf.WebUiServer);
            Assert.AreEqual(60000, conf.ScreenshotInterval);
            Assert.AreEqual(9, conf.KinectElevation);
            Assert.AreEqual("development", conf.Environment);
            Assert.AreEqual("C://pods/dist-static-image.pod.zip", conf.PodFile);
            Assert.AreEqual(true, conf.CalibrationActive);
            Assert.AreEqual("TestUnit", conf.Name);
            Assert.AreEqual("mixpanel_api_key", conf.Analytics.MixpanelApiKey);
            Assert.AreEqual("mixpanel_api_token", conf.Analytics.MixpanelApiToken);
        }
    }
}