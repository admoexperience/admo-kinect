using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Admo.classes;
using Newtonsoft.Json;
using NLog;
using Alchemy.Classes;
using WebSocketServer = Alchemy.WebSocketServer;

namespace Admo
{
    public class SocketServer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static WebSocketServer _aServer;

        private static Boolean _serverRunning;

        //Concurrent lists arent in c#
        protected static ConcurrentDictionary<string, UserContext> ConnectedClients = new ConcurrentDictionary<string, UserContext>();

        public static void StartServer()
		{
            if (_serverRunning) return;

            Log.Info("Starting SocketIOClient server");

            _aServer = new WebSocketServer(1080, IPAddress.Any)
                {
                    OnReceive = OnReceive,
                    OnSend = OnSend,
                    OnConnected = OnConnect,
                    OnConnect = OnConnect,
                    OnDisconnect = OnDisconnect,
                    TimeOut = new TimeSpan(0, 5, 0)
                };

            _aServer.Start();
            _serverRunning = true;
		}

        public static void SendRawData(String data)
        {
            var properties = new Dictionary<string, object>();
            properties["gesture"] = data;

              // iterates, and updates the value by one
            foreach (var client in ConnectedClients.Values)
            {
                try
                {
                    client.Send(JsonConvert.SerializeObject(properties));
                }
                catch (Exception e)
                {
                    Log.Error("Could not send message to client "+ client.ClientAddress,e);
                }
                
            }
        }

        public static void Stop()
        {
            _aServer.Stop();
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
            ConnectedClients.TryAdd(context.ClientAddress.ToString(), context);
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
                if (obj.type == "alive")
                {
                    LifeCycle.BrowserTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
                    SendRawData("host-"+ Config.GetHostName());
                }

            }
            catch (Exception e) // Bad JSON! For shame.
            {
                Log.Error("Error parsing json from client "+context.ClientAddress,e);
            }
        }

        /// <summary>
        /// Event fired when the Alchemy Websockets server instance sends data to a client.
        /// Logs the data to the console and performs no further action.
        /// </summary>
        /// <param name="context">The user's connection context</param>
        public static void OnSend(UserContext context)
        {
            //Log.Debug("Data Send To : " + context.ClientAddress);
        }

        /// <summary>
        /// Event fired when a client connects from the Alchemy Websockets server instance.
        /// Removes the user from the online users list and broadcasts the disconnection message
        /// to all connected users.
        /// </summary>
        /// <param name="context">The user's connection context</param>
        public static void OnDisconnect(UserContext context)
        {
            Log.Debug("Client Disconnected : " + context.ClientAddress);
            var key = context.ClientAddress.ToString();
            UserContext client = ConnectedClients.Values.Single(o => o.ClientAddress == context.ClientAddress);

            ConnectedClients.TryRemove(key, out client);
        }
    }
}