using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Admo.classes;
using Newtonsoft.Json;
using NLog;
using Alchemy.Classes;
using Newtonsoft.Json.Serialization;
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

        public static void SendKinectData(KinectState state)
        {
            SendToAll(ConvertToJson("kinectState",state));
        }

        public static void SendUpdatedConfig()
        {
            Log.Debug("Sending updated config to clients");
            var config = Config.GetConfiguration();
            var sendData = ConvertToJson("config", config);
            SendToAll(sendData);
        }

        //Converts data to json
        public static String ConvertToJson(String type, Object data)
        {
            var cont = new DataContainer { Type = type, Data = data };

            var x = JsonConvert.SerializeObject(cont,
                                              Formatting.None,
                                              new JsonSerializerSettings
                                              {
                                                  NullValueHandling = NullValueHandling.Ignore,
                                                  Formatting = Formatting.None,
                                                  ContractResolver = new CamelCasePropertyNamesContractResolver()
                                              });
            return x;
        }


        private static void SendToAll(String data)
        {
            // iterates, and updates the value by one
            foreach (var client in ConnectedClients.Values)
            {
                try
                {
                    client.Send(data);
                }
                catch (Exception e)
                {
                    Log.Error("Could not send message to client " + client.ClientAddress, e);
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
           // Log.Debug("Received Data From :" + context.ClientAddress);
            try
            {
                var json = context.DataFrame.ToString();

                // <3 dynamics
                dynamic obj = JsonConvert.DeserializeObject(json);
                if (obj.type == "alive")
                {
                    LifeCycle.SetBrowserTimeNow();
                   // SendRawData("host-"+ Config.GetHostName());
                }else if (obj.type == "config")
                {
                    Log.Debug("Client requested config");
                    //Send the config options on each client connect
                    var config = Config.GetConfiguration();
                    var sendData = ConvertToJson("config", config);
                    context.Send(sendData);
                }

            }
            catch (Exception e) // Bad JSON! For shame.
            {
                Log.Error("Error parsing json from client "+context.ClientAddress,e);
                Log.Error(context.DataFrame.ToString());
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