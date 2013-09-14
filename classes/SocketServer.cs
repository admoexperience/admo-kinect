using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Admo.classes;
using Newtonsoft.Json;
using NLog;
using Newtonsoft.Json.Serialization;
using Fleck;

namespace Admo
{
    public class SocketServer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //private static WebSocketServer _aServer;

        private static Boolean _serverRunning;
        private static WebSocketServer _server;

        protected static List<IWebSocketConnection> AllSockets;
        public static void StartServer()
		{
            if (_serverRunning) return;

            Logger.Info("Starting SocketIOClient server");
            _server = new WebSocketServer("ws://localhost:1080");
            AllSockets = new List<IWebSocketConnection>();
            _server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Logger.Debug("Client Connection From : " + socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort);
                    AllSockets.Add(socket);
                };
                socket.OnClose = () =>
                    {
                        Logger.Debug("Client Disconnected : " + socket.ConnectionInfo.ClientIpAddress + ":" +
                                     socket.ConnectionInfo.ClientPort);
                    AllSockets.Remove(socket);
                };
                socket.OnMessage = OnReceive;
            });
            _serverRunning = true;
		}

        public static void SendReloadEvent()
        {
            SendToAll(ConvertToJson("reload","reload"));
        }

        public static void SendKinectData(KinectState state)
        {
            SendToAll(ConvertToJson("kinectState",state));
        }

        public static void SendUpdatedConfig()
        {
            Logger.Debug("Sending updated config to clients");
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
            AllSockets.ToList().ForEach(s => s.Send(data));
        }

        public static void Stop()
        {
            _server.Dispose();
            _serverRunning = false;
        }

  
        public static void OnReceive(String message)
        {
            try
            {
                // <3 dynamics
                dynamic obj = JsonConvert.DeserializeObject(message);
                if (obj.type == "alive")
                {
                    MainWindow.LifeCycle.SetBrowserTimeNow();
                   // SendRawData("host-"+ Config.GetHostName());
                }else if (obj.type == "config")
                {
                    Logger.Debug("Client requested config");
                    //Send the config options on each client connect
                    var config = Config.GetConfiguration();
                    var sendData = ConvertToJson("config", config);
                    SendToAll(sendData);
                }

            }
            catch (Exception e) // Bad JSON! For shame.
            {
                Logger.Error("Error parsing json from client ");
                Logger.Error(message);
            }
        }
    }
}