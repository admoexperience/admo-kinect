using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using NLog;
using System.Web;

namespace Admo.classes
{
    public class WebServer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static String _address;
        private static Thread _listenThread;
        private static HttpListener _listener;

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

        private static void Worker(object state)
        {
            // start listening


                _listener.Start();

                // request -> response loop

                    HttpListenerContext context = _listener.GetContext();
                    HttpListenerRequest request = context.Request;

                    /* respond to the request.
                     * in this case it'll show "Server appears to be working".
                     * regardless of what file/path was requested.
                     */
                    using (HttpListenerResponse response = context.Response)
                    {
                        string html = "<b>Server appears to be working in https!</b>";
                        byte[] data = Encoding.UTF8.GetBytes(html);

                        response.ContentType = "text/html";
                        response.ContentLength64 = data.Length;

                        using (Stream output = response.OutputStream)
                        {
                            output.Write(data, 0, data.Length);
                        }
                    }
                
   

            //c.WriteLine("[{0:HH:mm}] Running", DateTime.Now);
        }
    }
}  