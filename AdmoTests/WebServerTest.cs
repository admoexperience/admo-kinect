using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
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
        public void TestHttpRequestPlain()
        {
            //  string sURL;
            var url = "https://localhost:5001";
            //WbRequest wrGETURL;
            var webrequest = (HttpWebRequest)WebRequest.Create(url);
  
            var response = (HttpWebResponse)webrequest.GetResponse();
           
            Assert.AreEqual(@"<b>Simple IndexFile</b>",GetStringFromStream(response));
        }
        [TestMethod]
        public void TestHttpRequest()
        {
            //  string sURL;
            var url = "https://localhost:5001/testFile.html";
            //WbRequest wrGETURL;
            var webrequest = (HttpWebRequest)WebRequest.Create(url);

            var response = (HttpWebResponse)webrequest.GetResponse();

            Assert.AreEqual(@"<b>TestFile</b>", GetStringFromStream(response));

            Assert.AreEqual(response.Headers["Content-Range"], "bytes 0-14/15");
            Assert.AreEqual(response.Headers["Content-Type"], "text/html");
        }

        [TestMethod]
        public void TestStageredRequest()
        {
            //  string sURL;
            var url = "https://localhost:5001/testFile.html";
            //WbRequest wrGETURL;
            var webrequest = (HttpWebRequest)WebRequest.Create(url);
            webrequest.AddRange(8,20);
            var response = (HttpWebResponse)webrequest.GetResponse();

            Assert.AreEqual(12, response.ContentLength);

            Assert.AreEqual(response.Headers["Content-Range"],"bytes 8-19/15");
            Assert.AreEqual(response.Headers["Content-Type"], "text/html");

        }

        [TestMethod]
        public void TestFileNotFound()
        {
            //  string sURL;
            var url = "https://localhost:5001/amissingfile.html";
            //WbRequest wrGETURL;
            var webrequest = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                var response = (HttpWebResponse)webrequest.GetResponse();
            }
            catch (WebException e )
            {
                var str = e.Message;

                Assert.AreEqual("The remote server returned an error: (404) Not Found.", e.Message);
            }
        

        }


        public static string GetStringFromStream(HttpWebResponse response)
    {
             var target = string.Empty;

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

            return target;
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
