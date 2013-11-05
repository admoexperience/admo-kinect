using Admo.Api.Dto;
using Admo.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdmoTests.Utilities
{
    [TestClass]
    public class JsonHelperTest
    {
        [TestMethod]
        public void CanParseOutUnderscores()
        {
            const string json = "{"+
                                "\"name\": \"name\","+
                                "\"updated_at\": \"2013-11-04T19:33:57.602Z\","+
                                "\"pod_checksum\": \"sum\","+
                                "\"pod_url\": \"http://foo.com/bar/test.zip\""+
                                "}";
            var pod = JsonHelper.ConvertFromApiRequest<PodApp>(json);
            Assert.AreEqual("name",pod.Name);
            Assert.AreEqual("2013-11-04T19:33:57.602Z", pod.UpdatedAt);
            Assert.AreEqual("sum", pod.PodChecksum);
            Assert.AreEqual("http://foo.com/bar/test.zip", pod.PodUrl);
        }
    }
}
