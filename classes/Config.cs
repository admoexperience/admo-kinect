using System;
using System.IO;
using System.Net.Http;
using NLog;
using Newtonsoft.Json;

namespace Admo.classes
{
    class Config
    {
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

        //Production mode by default. 
        //Text field can be used to change enviroment only once per startup
        public static Boolean IsDevMode()
        {
            if (_enviroment == null)
            {
                _enviroment = ReadConfig("Enviroment");
                if (_enviroment.Equals(String.Empty))
                {
                    _enviroment = "production";
                }
            }
            return _enviroment.Equals("development");
        }

        public static String GetWebServer()
        {
            if (_webServer != null) return _webServer;
            _webServer = ReadConfig("WebServer");
            //If the config option isn't there return the default value
            if (_webServer.Equals(String.Empty))
            {
                _webServer = "https://localhost:3001";
            }
            return _webServer;
        }

        public static String GetBaseConfigPath()
        {
            return BaseDropboxFolder;
        }

        public static String GetCurrentApp()
        {
            var appName = ReadConfig("App");
            return appName;
        }

        public static int GetElevationAngle()
        {
            var temp = ReadConfig("Elevation");
            var elevationAngle = Convert.ToInt32(temp);
            Log.Info("elevation path: " + elevationAngle);
            return elevationAngle;

        }

        public static String GetStatusFile()
        {
            return BaseDropboxFolder +  @"\\" + Environment.MachineName+ @"\Status.txt";
        }

        private static String GetConfigCacheLocation(String configOption)
        {
            return BaseDropboxFolder + @"\" + Environment.MachineName + @"\" + configOption + ".txt"; ;
        }

        private static String GetApiKey()
        {
            var apiKey = ReadConfig("ApiKey");
            if (apiKey.Equals(String.Empty))
            {
                throw new Exception("ApiKey not found please add it to [" + GetConfigCacheLocation("ApiKey")+"]");
            }
            return apiKey;
        }

        private static String ReadConfig(String config)
        {
            var configFile = GetConfigCacheLocation(config);
            String temp;
            try
            {
                var objReader =
                    new StreamReader(configFile);
                 temp = objReader.ReadLine();
                objReader.Close();
            }
            catch (FileNotFoundException fnfe)
            {
                Log.Debug("Config file not found ["+configFile+"]");
                return String.Empty;
            }
            return temp == null ? string.Empty : temp.Trim();
        }

        public static async void UpdateConfigCache()
        {
            var httpClient = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://admo-cms.herokuapp.com/unit/checkin.json");
            // Add our custom headers
            requestMessage.Headers.Add("Api-Key", GetApiKey());

            // Send the request to the server
            var response = await httpClient.SendAsync(requestMessage);

            // Just as an example I'm turning the response into a string here
            var responseAsString = await response.Content.ReadAsStringAsync();

            dynamic obj = JsonConvert.DeserializeObject(responseAsString);
            var appName = obj.unit.current_app;
            Log.Debug(appName);
            var cacheFile = GetConfigCacheLocation("App");
            try
            {
                var streamWriter = new StreamWriter(cacheFile);
                streamWriter.Write(appName);
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
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://admo-cms.herokuapp.com/unit/checkin.json");
            // Add our custom headers
            requestMessage.Headers.Add("Api-Key", GetApiKey());

            // Send the request to the server
            var response = await httpClient.SendAsync(requestMessage);

            // Just as an example I'm turning the response into a string here
            var responseAsString = await response.Content.ReadAsStringAsync();
            Log.Debug(responseAsString);
            /*var cacheFile = GetConfigCacheLocation(config);
            try
            {
                var streamWriter = new StreamWriter(cacheFile);
                streamWriter.Write(responseAsString);
                streamWriter.Close();
            }
            catch (Exception e)
            {
                Log.Error("Failed to write cache file for [" + config + "] to disk", e);
            }*/
        }
    }
}
