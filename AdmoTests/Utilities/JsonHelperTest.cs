using System.Collections.Generic;
using System.IO;
using System.Linq;
using Admo.Api;
using Admo.Api.Dto;
using Admo.classes;
using AdmoShared.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdmoTests.Utilities
{
    [TestClass]
    public class JsonHelperTest
    {
        [TestMethod]
        public void CanParseOutUnderscores()
        {
            const string json = "{\"apps\":[{" +
                                "\"name\": \"name\"," +
                                "\"updated_at\": \"2013-11-04T19:33:57.602Z\"," +
                                "\"pod_checksum\": \"sum\"," +
                                "\"pod_url\": \"http://foo.com/bar/test.zip\"" +
                                "}]" +
                                "}";
            var podList = JsonHelper.ConvertFromApiRequest<List<PodApp>>(json);
            Assert.AreEqual(1, podList.Count);
            var pod = podList.FirstOrDefault();
            Assert.AreEqual("name", pod.Name);
            Assert.AreEqual("2013-11-04T19:33:57.602Z", pod.UpdatedAt);
            Assert.AreEqual("sum", pod.PodChecksum);
            Assert.AreEqual("http://foo.com/bar/test.zip", pod.PodUrl);
        }

        [TestMethod]
        public void ShouldConvertCSharpToJsJson()
        {
            var cont = new DataContainer { Type = "test", Data = "StringData" };
            var json = JsonHelper.ConvertToCamelCase(cont);
            const string expectedJson = "{\"type\":\"test\",\"data\":\"StringData\"}";
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        public void TestDeserializeObject()
        {
            const string json = "{\"type\":\"test\",\"data\":\"StringData\"}";
            var data = JsonHelper.DeserializeObject<DataContainer>(json);
            Assert.AreEqual("test", data.Type);
            Assert.AreEqual("StringData", data.Data);
        }


    }
}