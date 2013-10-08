using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using c = System.Console;


namespace Admo
{
    public class WebServer
    {
        private static String _address;
        private static Thread _listenThread;
        private static HttpListener _listener;

        public WebServer()
        {
            c.WriteLine("[{0:HH:mm}] Initializing", DateTime.Now);

            // the address we want to listen on
            _address = "http://127.0.0.1:80/";

            // setup thread
            _listenThread = new Thread(Worker);
            _listenThread.IsBackground = true;
            _listenThread.Priority = ThreadPriority.Normal;

            // setup listener
            _listener = new HttpListener();
            _listener.Prefixes.Add(_address);

            // Gogogo
            _listenThread.Start(null);

            // prevent the console window from closing
            while (true) c.ReadKey(true);
        }

        private static void Worker(object state)
        {
            // start listening
            _listener.Start();

            c.WriteLine("[{0:HH:mm}] Running", DateTime.Now);

            // request -> response loop
            while (true)
            {
                HttpListenerContext context = _listener.GetContext();
                HttpListenerRequest request = context.Request;
                
                c.WriteLine(
                    "[{0:HH:mm}] Request received from {1}",
                    DateTime.Now,
                    request.LocalEndPoint.Address
                );

                /* respond to the request.
                 * in this case it'll show "Server appears to be working".
                 * regardless of what file/path was requested.
                 */
                using (HttpListenerResponse response = context.Response)
                {
                    string html = "<b>Server appears to be working Well!</b>";
                    byte[] data = Encoding.UTF8.GetBytes(html);

                    response.ContentType = "text/html";
                    response.ContentLength64 = data.Length;

                    using (Stream output = response.OutputStream)
                    {
                        output.Write(data, 0, data.Length);
                    }
                }

                c.WriteLine(
                    "[{0:HH:mm}] Handled request for {1}",
                    DateTime.Now,
                    request.LocalEndPoint.Address
                );
            }
        }
    }
}  