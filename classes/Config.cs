using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Admo.classes.lib;
using Admo.classes.stats;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;

namespace Admo.classes
{
    public class Config
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
            public const string FovCropTop = "fov_crop_top";
            public const string FovCropLeft = "fov_crop_left";
            public const string FovCropWidth = "fov_crop_width";
            public const string CalibrationActive = "calibration_active";
            public const string UnitName = "name";
            public const string MixpanelApiToken = "mixpanel_api_token";
            public const string MixpanelApiKey = "mixpanel_api_key";
        }

        private static Pubnub pubnub;
        public const int CheckingInterval = 5 * 60; //Once every 5mins
        private const int ScreenshotInterval = 30 * 60; //Once every 30mins
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        //variable dictating whether facetracking is activated
        public static readonly bool RunningFacetracking = false;

        private static String _enviroment = null;
        private static String _webServer = null;

        private const String PodFolder = @"C:\smartroom\pods\";
        private const String BaseDropboxFolder = @"C:\Dropbox\Admo-Units\";

        private static CmsApi _api;

        public static Boolean IsOnline = false;

        //Event handler when a config option changed.
        //Currently can't pick up which config event changed.
        public static event ConfigOptionChanged OptionChanged;
        public delegate void ConfigOptionChanged();

        public static StatsEngine StatsEngine;

        public static String GetHostName()
        {
            return Environment.MachineName;
        }


        public static void Init()
        {
            // 09/09/2013 -- Config was moved from dropbox to local storage folder.
            // This is only a temp code change to allow for automagic folder and migration to new folder
            MigratedLegacyConfig();

            _api = new CmsApi(GetApiKey());
            UpdateConfigCache();

            pubnub = new Pubnub("", GetPubNubSubKey(), "", "", false);
            pubnub.Subscribe<string>(GetApiKey(), OnPubNubMessage, OnPubNubConnection);


            var pod = new PodWatcher(GetPodFile(), PodFolder);
            pod.StartWatcher();
            pod.Changed += NewWebContent;
            OptionChanged += pod.OnConfigChange;

            var mixpanel = new Mixpanel(GetMixpanelApiKey(), GetMixpanelApiToken());
            var dataCache = new DataCache(Path.Combine(GetBaseConfigPath(),"analytics"));
            StatsEngine = new StatsEngine(dataCache, mixpanel);
        }

        private static void MigratedLegacyConfig()
        {
            if (Directory.Exists(GetBaseConfigPath())) return;
            Log.Info(GetBaseConfigPath() + " Doesn't exsist creating it");

            Directory.CreateDirectory(GetBaseConfigPath());

            //Move the config from the old dropbox folder to the new folder.
            File.Move(Path.Combine(BaseDropboxFolder,GetHostName(),"ApiKey.txt"), Path.Combine(GetBaseConfigPath(),"ApiKey.txt"));
            File.Move(Path.Combine(BaseDropboxFolder, GetHostName(), "configcache.json"), Path.Combine(GetBaseConfigPath(), "configcache.json"));
        }

        public static void NewWebContent(String file)
        {
            Log.Debug("New server data "+ file);
            SocketServer.SendReloadEvent();
        }

        public static List<String> ParsePubnubConnection(string result)
        {
            //List order is  
            // 0,1 connected disconnected
            //message
            //api key
            var list = JsonConvert.DeserializeObject<List<String>>(result);
            return list;
        }

        public static void OnPubNubConnection(string result)
        {
            var list = ParsePubnubConnection(result);
            var online = list[0].Equals("1");
            IsOnline = online;
            if (online)
            { 
                StatsEngine.ProcessOfflineCache();
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
            _webServer = ReadConfigOption(Keys.WebUiServer, "https://localhost:4001");
            return _webServer;
        }

        public static String GetBaseConfigPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "Admo");
        }

        public static int GetScreenshotInterval()
        {
            var val = ReadConfigOption(Keys.ScreenshotInterval, ScreenshotInterval.ToString());
            var result = ScreenshotInterval;
            int.TryParse(val, out result);
            return result;
        }


        public static String GetPodFile()
        {
            var def = Path.Combine(GetBaseConfigPath(), "pods", "dist.pod.zip");
            var pod = ReadConfigOption(Keys.PodFile, def);
            return pod;
        }

        public static String GetLaunchUrl()
        {
            return GetWebServer() + "/" + "index.html";
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

        public static String GetMixpanelApiToken()
        {
            return ReadAnalyticConfigOption(Keys.MixpanelApiToken);
        }

        public static String GetMixpanelApiKey()
        {
            return ReadAnalyticConfigOption(Keys.MixpanelApiKey);
        }

        private static String GetLocalConfig(String config)
        {
            return Path.Combine(GetBaseConfigPath(), config + ".txt");
        }

        private static String GetCmsConfigCacheFile()
        {
            return Path.Combine(GetBaseConfigPath(), "configcache.json");
        }

        private static String GetApiKey()
        {
            var apiKey = ReadLocalConfig("ApiKey");
            if (apiKey.Equals(String.Empty))
            {
                throw new Exception("ApiKey not found please add it to [" + GetLocalConfig("ApiKey") + "]");
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
            //This value has been deprecated infavour of using the units name.
            //Legacy units/apps should still pass in the correct param even though the value will change.
            //2013-10-09
            x.Add("hostname",ReadConfigOption(Keys.UnitName, GetHostName()));
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
                temp = objReader.ReadToEnd();
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
            var optionValue = obj["config"][option];

            var val =  optionValue == null ? string.Empty : optionValue.ToString().Trim();
            return val;
        }

        public static String ReadAnalyticConfigOption(String option)
        {

            var obj = GetJsonConfig();
            var analytics = obj["config"]["analytics"];

            if (analytics == null)
            {
                return String.Empty;
            }

            var optionValue = analytics[option];

            var val = optionValue == null ? string.Empty : optionValue.ToString().Trim();
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
                //test its valid json
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

            SocketServer.SendUpdatedConfig();

            if (OptionChanged != null) OptionChanged();
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
