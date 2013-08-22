using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Admo.classes.lib;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;

namespace Admo.classes
{
    class Config
    {
        public class Keys
        {
            public const string Environment = "environment";
            public const string WebUiServer = "web_ui_server";
            public const string PodFile = "pod_file";
            public const string App = "app";
            public const string LoadingPage = "loading_page";
            public const string KinectElevation = "kinect_elevation";
            public const string PubnubSubscribeKey = "pubnub_subscribe_key";
            public const string ScreenshotInterval = "screenshot_interval";
            public const string FOVcropTop = "fov_crop_top";
            public const string FOVcropLeft = "fov_crop_left";
            public const string FOVcropWidth = "fov_crop_width";
            public const string CalibrationActive = "calibration_active";
        }

        private static Pubnub pubnub;
        public const double CheckingInterval = 5 * 60; //Once every 5mins
        private const double ScreenshotInterval = 30 * 60; //Once every 30mins
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        //variable dictating whether facetracking is activated
        public static readonly bool RunningFacetracking = false;

        private static String _enviroment = null;
        private static String _webServer = null;
        private const String BaseDropboxFolder = @"C:\Dropbox\Admo-Units\";

        private const String PodFolder = @"C:\smartroom\pods\";

        private static CmsApi _api;

        //Event handler when a config option changed.
        //Currently can't pick up which config event changed.
        public static event ConfigOptionChanged OptionChanged;
        public delegate void ConfigOptionChanged();

        public static String GetHostName()
        {
            return Environment.MachineName;
        }


        public static void Init()
        {
            _api = new CmsApi(GetApiKey());
            UpdateConfigCache();

            pubnub = new Pubnub("", GetPubNubSubKey(), "", "", false);
            pubnub.Subscribe<string>(GetApiKey(), OnPubNubMessage, OnPubNubConnection);

            var pod = new PodWatcher(GetPodFile(), PodFolder);
            pod.StartWatcher();
            pod.Changed += NewWebContent;
            OptionChanged += pod.OnConfigChange;
        }

        public static void NewWebContent(String file)
        {
            Log.Debug("New server data "+ file);
            SocketServer.SendReloadEvent();
        }


        private static void OnPubNubConnection(string result)
        {
            //List order is  
            // 0,1 connected disconnected
            //message
            //api key
            var list = JsonConvert.DeserializeObject<List<object>>(result);
            if (list[0].ToString().Equals("1"))
            {
                UpdateConfigCache();
                _api.CheckIn();
                Log.Debug("Pubnub connected [" + list[1]+"]");
            }
            else
            {
                Log.Debug("Pubnub disconnected [" + list[1] + "]");
            }
        }

        
        private static void OnPubNubMessage(string result)
        {
            var list = JsonConvert.DeserializeObject<List<String>>(result);
            var command = CommandFactory.ParseCommand(list[0]);
            //Performs the command
            command.Perform();
        }

        //Production mode by default. 
        //Text field can be used to change enviroment only once per startup
        public static Boolean IsDevMode()
        {
            if (_enviroment == null)
            {
                _enviroment = ReadConfigOption(Keys.Environment,"production");
            }
            return _enviroment.Equals("development");
        }

        public static String GetWebServer()
        {
            if (_webServer != null) return _webServer;
            _webServer = ReadConfigOption(Keys.WebUiServer, "https://localhost:3001");
            return _webServer;
        }

        public static String GetBaseConfigPath()
        {
            return BaseDropboxFolder;
        }

        public static double GetScreenshotInterval()
        {
            var val = ReadConfigOption(Keys.ScreenshotInterval, ScreenshotInterval.ToString());
            var result = ScreenshotInterval;
            double.TryParse(val, out result);
            return result;
        }


        public static String GetPodFile()
        {
            var def = Path.Combine(BaseDropboxFolder, "pods", "dist.pod.zip");
            var pod = ReadConfigOption(Keys.PodFile, def);
            return pod;
        }

        public static String GetCurrentApp()
        {
            var appName = ReadConfigOption(Keys.App,"demo");
            return appName;
        }

        public static String GetLoadingPage()
        {
            var loading = ReadConfigOption(Keys.LoadingPage, "loading.html");
            return GetWebServer() + "/" + loading;
        }

        public static int GetElevationAngle()
        {
            var temp = ReadConfigOption(Keys.KinectElevation,"1");
            var elevationAngle = Convert.ToInt32(temp);
            Log.Info("elevation path: " + elevationAngle);
            return elevationAngle;

        }

        private static String GetLocalConfig(String configOption)
        {
            return Path.Combine(BaseDropboxFolder, GetHostName(), configOption + ".txt");
        }

        private static String GetCmsConfigCacheFile()
        {
            return Path.Combine(BaseDropboxFolder, GetHostName(), "configcache.json");
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
            var key = ReadConfigOption(Keys.PubnubSubscribeKey, "");
            if (String.IsNullOrEmpty(key))
            {
                Log.Warn("Pubnubkey not found manually triggering an update; please restart application");
                //TODO: This should throw some sort of exception.
                //We need to make sure the config is bootstraped from the app before starting.
                UpdateConfigCache();
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
            if (String.IsNullOrEmpty(val))
            {
                val = defaultOption;
            }
            return val;
        }

        public static JObject GetConfiguration()
        {
            var x = GetJsonConfig()["config"] as JObject;
            x.Add("hostname",GetHostName());
            x.Add("apiKey", GetApiKey());
            x.Add("cmsUri", CmsApi.CmsUrl);
            return x;
        }

        private static JObject GetJsonConfig()
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
                Log.Error("Cache file not found [" + cacheFile + "]");
                return new JObject();
            }
            catch (FileNotFoundException fnfe)
            {
                Log.Error("Cache file not found [" + cacheFile + "]");
                return new JObject();
            }
            var obj = (JObject)JsonConvert.DeserializeObject(temp);
            return obj;
        }

        public static String ReadConfigOption(String option)
        {

            var obj = GetJsonConfig();
            object optionValue = obj["config"][option];

            var val =  optionValue == null ? string.Empty : optionValue.ToString().Trim();
            return val;
    
        }

        public static void CheckIn()
        {
            _api.CheckIn();
        }

        public static async void UpdateConfigCache()
        {
            try
            {
                Log.Debug("Updating config");

                var responseAsString = await _api.GetConfig();

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
            catch (Exception e)
            {
                //Happens when the unit is offline
                Log.Warn("Unable to update the cacheconfig file",e);
            }

            try
            {
                SocketServer.SendUpdatedConfig();
                //Hack for checking config changes. this SHOULD be done via an interface so lots of classes can read callbacks.
                if (OptionChanged != null) OptionChanged();
            }
            catch (Exception e)
            {
                Log.Warn("Unable to do config callbacks", e);
            }
        }

        public static async void TakeScreenshot()
        {
            try
            {
                Log.Debug("Taking screenshot");
                var sc = new ScreenCapture();
                // capture entire screen
                var img = sc.CaptureScreen();
                
                //Resize it slightly? 
                const int compress = 2;
                var width = (int)(img.Width / compress);
                var height = (int)(img.Height / compress);
                var smallImage = (Image) new Bitmap(img, width, height);
                var result = await _api.PostScreenShot(smallImage);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Error("Unable to save screen shot", e);
            }
        }

    }
}
