using System;
using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Admo.classes;
namespace AdmoTests
{
    [TestClass]
    public class WebServerTest
    {
        //
        public static WebServer _webserver;
        [ClassInitialize]
        public static void SetupWebserverAndFileStructure(TestContext testCon)
        {
            _webserver = new WebServer(Directory.GetCurrentDirectory());
            _webserver.Start();
            Directory.CreateDirectory("current");
            File.WriteAllText("current/index.html",@"<b>Simple IndexFile</b>");
            File.WriteAllText("current/testFile.html", @"<b>TestFile</b>");
        }

        [TestMethod]
        public void TestHttpRequest()
        {
            //  string sURL;
            var url = "https://localhost:5001";
            //WbRequest wrGETURL;
            var webrequest = (HttpWebRequest)WebRequest.Create(url);
            string target = string.Empty;
            var response = (HttpWebResponse)webrequest.GetResponse();

            try
            {
                var streamReader = new StreamReader(response.GetResponseStream(), true);
                try
                {
                    target = streamReader.ReadToEnd();
                }
                finally
                {
                    streamReader.Close();
                }
            }
            finally
            {
                response.Close();
            }

            Assert.AreEqual(@"<b>Simple IndexFile</b>",target);
        }

        [ClassCleanup]
        public static void CloseWebserver()
        {
            _webserver.Close();
            File.Delete("current/index.html");
            File.Delete("current/testFile.html");
            Directory.Delete("current");


        }
    }
}
