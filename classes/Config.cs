using System;
using System.IO;
using System.Net.Http;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;

namespace Admo.classes
{
    class Config
    {

        private static Pubnub pubnub;
        public const double CheckingInterval = 5 * 60; //Once every 5mins
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        //variable dictating whether facetracking is activated
        public static readonly bool RunningFacetracking = false;

        private static String _enviroment = null;
        private static String _webServer = null;
        private const String BaseDropboxFolder = @"C:\Dropbox\Admo-Units\";

        public static String GetHostName()
        {
            return Environment.MachineName;
        }

        public static void InitPubNub()
        {
            pubnub = new Pubnub("", GetPubNubSubKey(), "", "", false);
            pubnub.Subscribe<string>(GetApiKey(), OnPubNubMessage, OnPubNubConnect);
        }

        private static void OnPubNubConnect(string result)
        {
            UpdateConfigCache();
            Log.Debug("Pubnub connected "+ result);
        }

        private static void OnPubNubMessage(string result)
        {
            UpdateConfigCache();
            Log.Debug("Pubnub message " + result);
        }

        //Production mode by default. 
        //Text field can be used to change enviroment only once per startup
        public static Boolean IsDevMode()
        {
            if (_enviroment == null)
            {
                _enviroment = ReadConfigOption("environment","production");
            }
            return _enviroment.Equals("development");
        }

        public static String GetWebServer()
        {
            if (_webServer != null) return _webServer;
            _webServer = ReadConfigOption("web_ui_server","https://localhost:3001");
            return _webServer;
        }

        public static String GetBaseConfigPath()
        {
            return BaseDropboxFolder;
        }

        public static String GetCurrentApp()
        {
            var appName = ReadConfigOption("app","demo");
            return appName;
        }

        public static int GetElevationAngle()
        {
            var temp = ReadConfigOption("kinect_elevation","1");
            var elevationAngle = Convert.ToInt32(temp);
            Log.Info("elevation path: " + elevationAngle);
            return elevationAngle;

        }

        private static String GetLocalConfig(String configOption)
        {
            return BaseDropboxFolder + @"\" + GetHostName() + @"\" + configOption + ".txt"; ;
        }

        private static String GetCmsConfigCacheFile()
        {
            return BaseDropboxFolder + @"\" + GetHostName() + @"\configcache.json"; ;
        }

        private static String GetApiKey()
        {
            var apiKey = ReadLocalConfig("ApiKey");
            if (apiKey.Equals(String.Empty))
            {
                throw new Exception("ApiKey not found please add it to [" + GetLocalConfig("ApiKey")+"]");
            }
            return apiKey;
        }
        
        private static String GetPubNubSubKey()
        {
            var key = ReadFile(BaseDropboxFolder +"\\config\\"+"PubNubSubKey.txt");
            if (key.Equals(String.Empty))
            {
                 throw new Exception("PubNubSubKey not found please add it");
            }
            return key;
        }

        private static String ReadLocalConfig(String config)
        {
            var configFile = GetLocalConfig(config);
            return ReadFile(configFile);
        }

        private static String ReadFile(string filePath)
        {
            String temp;
            try
            {
                var objReader =
                    new StreamReader(filePath);
                temp = objReader.ReadLine();
                objReader.Close();
            }
            catch (DirectoryNotFoundException dnfe){
                Log.Debug("Config file not found [" + filePath + "]");
                return String.Empty;
            }
            catch (FileNotFoundException fnfe)
            {
                Log.Debug("Config file not found [" + filePath + "]");
                return String.Empty;
            }
            return temp == null ? string.Empty : temp.Trim();
        }

        public static String ReadConfigOption(String option, string defaultOption)
        {
            var val = ReadConfigOption(option);
            //If the config option isn't there return the default value
            if (val.Equals(String.Empty))
            {
                val = defaultOption;
            }
            return val;
        }

        public static String ReadConfigOption(String option)
        {
            var cacheFile = GetCmsConfigCacheFile();
            String temp = null;
            try
            {
                var objReader =
                    new StreamReader(cacheFile);
                temp = objReader.ReadLine();
                objReader.Close();
            }
            catch (DirectoryNotFoundException dnfe)
            {
                Log.Debug("Cache file not found [" + cacheFile + "]");
                return string.Empty;
            }
            catch (FileNotFoundException fnfe)
            {
                Log.Debug("Cache file not found [" + cacheFile + "]");
                return string.Empty;
            }
            var obj = (JObject)JsonConvert.DeserializeObject(temp);
           
            object optionValue = obj["config"][option];

            var val =  optionValue == null ? string.Empty : optionValue.ToString().Trim();
            return val;
    
        }

        public static async void UpdateConfigCache()
        {
            Log.Debug("Updating config");
            var httpClient = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://cms.admo.co/api/v1/unit/config.json");
            // Add our custom headers
            requestMessage.Headers.Add("Api-Key", GetApiKey());

            // Send the request to the server
            var response = await httpClient.SendAsync(requestMessage);

            // Just as an example I'm turning the response into a string here
            var responseAsString = await response.Content.ReadAsStringAsync();

            dynamic obj = JsonConvert.DeserializeObject(responseAsString);

            var cacheFile = GetCmsConfigCacheFile();
            try
            {
                var streamWriter = new StreamWriter(cacheFile);
                streamWriter.Write(responseAsString);
                streamWriter.Close();
            }
            catch (Exception e)
            {
                Log.Error("Failed to write cache file for [" + "App" + "] to disk", e);
            }
        } 


        public static async void CheckIn()
        {
            Log.Debug("Checking into the CMS");
            // You need to add a reference to System.Net.Http to declare client.
            var httpClient = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://cms.admo.co/api/v1/unit/checkin.json");
            // Add our custom headers
            requestMessage.Headers.Add("Api-Key", GetApiKey());

            // Send the request to the server
            var response = await httpClient.SendAsync(requestMessage);


            var responseAsString = await response.Content.ReadAsStringAsync();
        }
    }
}
