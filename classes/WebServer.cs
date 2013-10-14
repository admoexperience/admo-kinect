using System;
using System.IO;
using System.Net;
using System.Threading;
using NLog;


namespace Admo.classes
{
    public class WebServer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static String _address;
        private static Thread _listenThread;
        private static HttpListener _listener;
        private const string _myPath = @"C:/smartroom/current/";

        public WebServer()
        {
            _address = "https://localhost:9000/";

            // Process.Start("cmd", "/C copy c:\\file.txt lpt1");
            // setup thread
            _listenThread = new Thread(Worker);
            _listenThread.IsBackground = true;
            _listenThread.Priority = ThreadPriority.Normal;

            // setup listener
            _listener = new HttpListener();
            _listener.Prefixes.Add(_address);

            // Gogogo
            _listenThread.Start(null);
        }

        
        private void Worker(object state)
        {
            // start listening


                _listener.Start();

                // request -> response loop
                while (_listener.IsListening)
                {
                    HttpListenerContext context = _listener.GetContext();
                    HttpListenerRequest request = context.Request;

                    /* respond to the request.
                     * in this case it'll show "Server appears to be working".
                     * regardless of what file/path was requested.
                     */
                    var myRequest = request.RawUrl;
                    if(request.RawUrl=="/")
                    {
                        myRequest = "Index.html";
                    }
                    if (myRequest.StartsWith("/"))
                    {
                        myRequest=myRequest.Remove(0, 1);
                    }
                    var myPath2 = Path.Combine(_myPath, myRequest);
                    try
                    {

                        using (HttpListenerResponse response = context.Response)
                        {

                            var file2Serve = File.ReadAllBytes(myPath2);

                            //byte[] data = Encoding.UTF8.GetBytes(html);
                            if (request.ContentType != null)
                                response.ContentType = request.ContentType;

                            response.ContentLength64 = file2Serve.Length;

                            using (Stream output = response.OutputStream)
                            {
                                output.Write(file2Serve, 0, file2Serve.Length);
                            }
                        }
                    }
                    catch (Exception)
                    {

                        Logger.Debug("unable to process" + myPath2);
                    }


                } 

            //c.WriteLine("[{0:HH:mm}] Running", DateTime.Now);
        }
        public void Close()
        {
            _listener.Stop();
        }
    }
}  