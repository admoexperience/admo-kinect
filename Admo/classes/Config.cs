﻿using System;
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
            public const string TransformSmoothingType = "transform_smoothing_type";
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static Pubnub pubnub;
        public const int CheckingInterval = 5 * 60; //Once every 5mins
        private const int ScreenshotInterval = 30 * 60; //Once every 30mins
        

        //variable dictating whether facetracking is activated
        public static readonly bool RunningFacetracking = false;

        private static String _enviroment = null;
        private static String _webServer = null;

        private const String PodFolder = @"C:\smartroom\pods\";
        private const String BaseDropboxFolder = @"C:\Dropbox\Admo-Units\";

        private static CmsApi Api { get; set; }
    

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
            Api = new CmsApi(GetApiKey());
            UpdateConfigCache();

            pubnub = new Pubnub("", GetPubNubSubKey(), "", "", false);
            pubnub.Subscribe<string>(GetApiKey(), OnPubNubMessage, result => OnPubnubConnection(result), OnPubnubError);


            var pod = new PodWatcher(GetPodFile(), PodFolder);
            pod.StartWatcher();
            pod.Changed += NewWebContent;
            OptionChanged += pod.OnConfigChange;

            var mixpanel = new Mixpanel(GetMixpanelApiKey(), GetMixpanelApiToken());
            var dataCache = new DataCache(Path.Combine(GetBaseConfigPath(),"analytics"));
            StatsEngine = new StatsEngine(dataCache, mixpanel);
        }

        private static void OnPubnubError(PubnubClientError obj)
        {
            Logger.Error("Pubnub error "+obj.Description);
        }

        public static void NewWebContent(String file)
        {
            Logger.Debug("New server data "+ file);
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

        public static void OnPubnubConnection(string result)
        {
            var list = ParsePubnubConnection(result);
            var online = list[0].Equals("1");
            IsOnline = online;
            if (online)
            { 
                StatsEngine.ProcessOfflineCache();
                UpdateConfigCache();
                Api.CheckIn();
                Logger.Debug("Pubnub connected [" + list[1]+"]");
            }
            else
            {
                Logger.Debug("Pubnub disconnected [" + list[1] + "]");
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
            _webServer = ReadConfigOption(Keys.WebUiServer, "https://localhost:5001");
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
            var defDir = Path.Combine(GetBaseConfigPath(), "pods");
            if (!Directory.Exists(defDir))
            {
                Directory.CreateDirectory(defDir);
            }
            var defPod = Path.Combine(defDir, "dist.pod.zip");
            var pod = ReadConfigOption(Keys.PodFile, defPod);
            return pod;
        }

        public static String GetLaunchUrl()
        {
            return GetWebServer() + "/" + "index.html";
        }

        public static String GetLoadingPage()
        {
            var exeLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(exeLocation);
            var loadingFile = Path.Combine(path, "resources", "loading.html");
            return "file:///" + loadingFile;
        }

        public static int GetElevationAngle()
        {
            var temp = ReadConfigOption(Keys.KinectElevation,"1");
            var elevationAngle = Convert.ToInt32(temp);
            Logger.Info("elevation path: " + elevationAngle);
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

        public static String GetLocalConfig(String config)
        {
            return Path.Combine(GetBaseConfigPath(), config + ".txt");
        }

        public static String GetCmsConfigCacheFile()
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

        public static Boolean HasApiKey()
        {
            var apiKey = ReadLocalConfig("ApiKey");
            return !apiKey.Equals(String.Empty);

        }

        private static String GetPubNubSubKey()
        {
            var key = ReadConfigOption(Keys.PubnubSubscribeKey, "");
            if (String.IsNullOrEmpty(key))
            {
                Logger.Warn("Pubnubkey not found manually triggering an update; please restart application");
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
                Logger.Debug("Config file not found [" + filePath + "]");
                return String.Empty;
            }
            catch (FileNotFoundException fnfe)
            {
                Logger.Debug("Config file not found [" + filePath + "]");
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
                Logger.Error("Cache file not found [" + cacheFile + "]");
                return new JObject();
            }
            catch (FileNotFoundException fnfe)
            {
                Logger.Error("Cache file not found [" + cacheFile + "]");
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
            Api.CheckIn();
        }

        public static async void UpdateConfigCache()
        {
            try
            {
                Logger.Debug("Updating config");

                var responseAsString = await Api.GetConfig();
                //test its valid json
                dynamic obj = JsonConvert.DeserializeObject(responseAsString);
                var cacheFile = GetCmsConfigCacheFile();
                try
                {
                    File.WriteAllText(cacheFile, responseAsString);
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to write cache file for [" + "App" + "] to disk", e);
                }
            }
            catch (Exception e)
            {
                //Happens when the unit is offline
                Logger.Warn("Unable to update the cacheconfig file",e);
            }

            SocketServer.SendUpdatedConfig();

            if (OptionChanged != null) OptionChanged();
        }

        public static async void TakeScreenshot()
        {
            try
            {
                Logger.Debug("Taking screenshot");
                var sc = new ScreenCapture();
                // capture entire screen
                var img = sc.CaptureScreen();
                
                //Resize it slightly? 
                const int compress = 2;
                var width = (int)(img.Width / compress);
                var height = (int)(img.Height / compress);
                var smallImage = (Image) new Bitmap(img, width, height);
                var result = await Api.PostScreenShot(smallImage);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Logger.Error("Unable to save screen shot", e);
            }
        }

        public static string GetTransformSmoothType()
        {
            return ReadConfigOption(Keys.TransformSmoothingType, "avatar");
        }
    }
}
