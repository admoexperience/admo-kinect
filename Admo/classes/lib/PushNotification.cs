using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NLog;
using PubNubMessaging.Core;

namespace Admo.classes.lib
{
    public class PushNotification
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public String SubscribeKey { get; set;}
        public String Channel { get; set; }

        private Pubnub _pubnub;
        public Action<Boolean> OnConnection { get; set; }

        public void Connect()
        {
            _pubnub = new Pubnub("", SubscribeKey, "", "", false);
            _pubnub.Subscribe<string>(Channel, OnPubNubMessage, OnPubnubConnection, OnPubnubError);
        }

        private void OnPubnubError(PubnubClientError obj)
        {
            Logger.Error("Pubnub error " + obj.Description);
        }

        public void OnPubnubConnection(string result)
        {
            var list = ParsePubnubConnection(result);
            var online = list[0].Equals("1");
            OnConnection(online);
            if (online)
            {
                Logger.Debug("Pubnub connected [" + list[1] + "]");
            }
            else
            {
                Logger.Debug("Pubnub disconnected [" + list[1] + "]");
            }
        }

        public List<String> ParsePubnubConnection(string result)
        {
            //List order is  
            // 0,1 connected disconnected
            //message
            //api key
            var list = JsonConvert.DeserializeObject<List<String>>(result);
            return list;
        }

        private static void OnPubNubMessage(string result)
        {
            var list = JsonConvert.DeserializeObject<List<String>>(result);
            var command = CommandFactory.ParseCommand(list[0]);
            //Performs the command
            command.Perform();
        }

    }
}
