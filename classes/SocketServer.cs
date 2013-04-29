using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Admo.classes;
using Microsoft.Kinect;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using Fleck;
using System.IO;
using System.Threading;
//using System.Windows.Forms.Integration;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SocketIOClient.Messages;
using SocketIOClient;
using SocketIOClient.Eventing;
using Newtonsoft.Json.Linq;
using NLog;
using Alchemy;
using Alchemy.Classes;
using WebSocketServer = Alchemy.WebSocketServer;

namespace Admo
{
    public class SocketServer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static WebSocketServer aServer;

        private static Boolean _serverRunning = false;

        private static UserContext lastUserContext = null;

        //Websocket client setup for communication between Node server and .NET
        public static void StartServer()
		{
            if (_serverRunning) return;

            Log.Info("Starting SocketIOClient server");

            aServer = new Alchemy.WebSocketServer(1080, IPAddress.Any)
                {
                    OnReceive = OnReceive,
                    OnSend = OnSend,
                    OnConnected = OnConnect,
                    OnConnect = OnConnect,
                    OnDisconnect = OnDisconnect,
                    TimeOut = new TimeSpan(0, 5, 0)
                };

            aServer.Start();
            _serverRunning = true;
		}

        private  static int x = 0;

        public static void SendRawData(String data)
        {
           
            if (lastUserContext != null)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();
                if (++x % 50 == 0)
                {
                    Log.Debug(data);
                    Log.Debug(lastUserContext);
                    Log.Debug(JsonConvert.SerializeObject(properties));
                    properties["gesture"] = data;

                    lastUserContext.Send(JsonConvert.SerializeObject(properties));
                }
                
              
            }
    }

        public static void Stop()
        {
            aServer.Stop();
            _serverRunning = false;
        }

        /// <summary>
        /// Event fired when a client connects to the Alchemy Websockets server instance.
        /// Adds the client to the online users list.
        /// </summary>
        /// <param name="context">The user's connection context</param>
        public static void OnConnect(UserContext context)
        {
            Log.Debug("Client Connection From : " + context.ClientAddress);
            lastUserContext = context;

        }

        /// <summary>
        /// Event fired when a data is received from the Alchemy Websockets server instance.
        /// Parses data as JSON and calls the appropriate message or sends an error message.
        /// </summary>
        /// <param name="context">The user's connection context</param>
        public static void OnReceive(UserContext context)
        {
            Log.Debug("Received Data From :" + context.ClientAddress);

            try
            {
                var json = context.DataFrame.ToString();

                // <3 dynamics
                dynamic obj = JsonConvert.DeserializeObject(json);

               Console.WriteLine("Received Data From :" + json);
            }
            catch (Exception e) // Bad JSON! For shame.
            {
                var r = new Response {Type = ResponseType.Error, Data = new {e.Message}};

                context.Send(JsonConvert.SerializeObject(r));
            }
        }

        /// <summary>
        /// Event fired when the Alchemy Websockets server instance sends data to a client.
        /// Logs the data to the console and performs no further action.
        /// </summary>
        /// <param name="context">The user's connection context</param>
        public static void OnSend(UserContext context)
        {
            Log.Debug("Data Send To : " + context.ClientAddress);
        }

        /// <summary>
        /// Event fired when a client disconnects from the Alchemy Websockets server instance.
        /// Removes the user from the online users list and broadcasts the disconnection message
        /// to all connected users.
        /// </summary>
        /// <param name="context">The user's connection context</param>
        public static void OnDisconnect(UserContext context)
        {
            Log.Debug("Client Disconnected : " + context.ClientAddress);
            lastUserContext = null;
        }

       

       
        

        /// <summary>
        /// Broadcasts an error message to the client who caused the error
        /// </summary>
        /// <param name="errorMessage">Details of the error</param>
        /// <param name="context">The user's connection context</param>
        private static void SendError(string errorMessage, UserContext context)
        {
            var r = new Response {Type = ResponseType.Error, Data = new {Message = errorMessage}};

            context.Send(JsonConvert.SerializeObject(r));
        }

      

      

      
        /// <summary>
        /// Defines the type of response to send back to the client for parsing logic
        /// </summary>
        public enum ResponseType
        {
            Connection = 0,
            Disconnect = 1,
            Message = 2,
            NameChange = 3,
            UserCount = 4,
            Error = 255
        }

        /// <summary>
        /// Defines the response object to send back to the client
        /// </summary>
        public class Response
        {
            public ResponseType Type { get; set; }
            public dynamic Data { get; set; }
        }

        /// <summary>
        /// Holds the name and context instance for an online user
        /// </summary>
        public class User
        {
            public string Name = String.Empty;
            public UserContext Context { get; set; }
        }
       

        /// <summary>
        /// Defines a type of command that the client sends to the server
        /// </summary>
        public enum CommandType
        {
            Register = 0,
            Message,
            NameChange
        }
    }
}