﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Admo.Api;
using Admo.Api.Dto;
using Admo.classes.lib;
using Admo.classes.stats;
using Admo.forms;
using AdmoShared.Utilities;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;

namespace Admo.classes
{
    public class Config
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static readonly bool RunningFacetracking = false;
        public const string DefaultCmsApiUrl = "https://cms.admoexperience.com/api/v1";
        private static CmsApi Api { get; set; }

        public static Boolean IsOnline = false;

        public static PushNotification Pusher;

        //Event handler when a config option changed.
        //Currently can't pick up which config event changed.
        public static event ConfigOptionChanged OptionChanged;

        public delegate void ConfigOptionChanged();


        private static Api.Dto.Config _config;

        public static StatsEngine StatsEngine;

        //Used to override the base config location 
        public static string OverrideBaseConfigPath { get; set; }

        public static String GetHostName()
        {
            return Environment.MachineName;
        }

        public static void InitDirs()
        {
            //Create all the directories needed for admo to function
            var baseDir = new DirectoryInfo(GetBaseConfigPath());
            if (!baseDir.Exists)
            {
                baseDir.Create();
            }
            baseDir.CreateSubdirectory("analytics");
            baseDir.CreateSubdirectory("pods");
            baseDir.CreateSubdirectory(Path.Combine("webserver", "current"));
            baseDir.CreateSubdirectory(Path.Combine("webserver", "override"));
        }

        public static string GetPodLocation()
        {
            return Path.Combine(GetBaseConfigPath(), "pods");
        }


        public static void Init()
        {
            
            
            _config = ReadConfig();
            if (!IsLocalOnly())
            {
                Api = new CmsApi(GetApiKey(), GetBaseCmsUrl());
                UpdateAndGetConfigCache();

                //Only connect to pubnub if the key is there
                if (!String.IsNullOrEmpty(GetPubNubSubKey()))
                {
                    Pusher = new PushNotification
                    {
                        Channel = GetApiKey(),
                        SubscribeKey = GetPubNubSubKey(),
                        OnConnection = OnPushNotificationConnection
                    };
                    Pusher.Connect();
                } 

            }
           

            var mixpanel = new Mixpanel(GetMixpanelApiKey(), GetMixpanelApiToken(), GetUnitName());
            var dataCache = new DataCache(Path.Combine(GetBaseConfigPath(), "analytics"));
            StatsEngine = new StatsEngine(dataCache, mixpanel);

            var pod = new PodWatcher(GetPodFile(), _config.WebServerBasePath);
            pod.StartWatcher();
            pod.Changed += NewWebContent;
            OptionChanged += pod.OnConfigChange;

            //Async task to download pods in the background
            UpdatePods();
        }

        public static string GetBaseCmsUrl()
        {
            var file = GetLocalConfig("BaseCmsUrl");
            if (!File.Exists(file))
            {
                return DefaultCmsApiUrl;
            }
            var url = ReadLocalConfig("BaseCmsUrl");
            return String.IsNullOrEmpty(url) ? DefaultCmsApiUrl : url;
        }

        public static void OnPushNotificationConnection(Boolean online)
        {
            IsOnline = online;
            if (!online) return;
            StatsEngine.ProcessOfflineCache();
            UpdateAndGetConfigCache();
          //  UpdateConfigCache();
            Api.CheckIn();
        }

        public static void NewWebContent(String file)
        {
            Logger.Debug("New server data " + file);
            SocketServer.SendReloadEvent();
        }

        public static string GetWebServerBasePath()
        {
            return _config.WebServerBasePath;
        }

        //Production mode by default.
        public static Boolean IsDevMode()
        {
            return _config.Environment.Equals("development");
        }

        public static String GetWebServer()
        {
            return _config.WebUiServer;
        }

        public static String GetBaseConfigPath()
        {
            
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var defaultPath =  Path.Combine(appData, "Admo");

            return String.IsNullOrEmpty(OverrideBaseConfigPath) ? defaultPath : OverrideBaseConfigPath;
        }

        public static int GetScreenshotInterval()
        {
            return _config.ScreenshotInterval;
        }


        public static String GetPodFile()
        {
            var podFile = _config.PodFile;
            if (Path.IsPathRooted(podFile))
            {
                return podFile;
            }

            return Path.Combine(GetPodLocation(), _config.PodFile);
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
            return _config.KinectElevation;
        }

        public static string GetMixpanelApiToken()
        {
            return _config.Analytics.MixpanelApiToken;
        }

        public static string GetMixpanelApiKey()
        {
            return _config.Analytics.MixpanelApiKey;
        }

        public static string GetLocalConfig(String config)
        {
            return Path.Combine(GetBaseConfigPath(), config + ".txt");
        }

        public static string GetCmsConfigCacheFile()
        {
            return Path.Combine(GetBaseConfigPath(), "configcache.json");
        }

        private static string GetApiKey()
        {
            var apiKey = ReadLocalConfig("ApiKey");
            if (apiKey.Equals(String.Empty))
            {
                throw new Exception("ApiKey not found please add it to [" + GetLocalConfig("ApiKey") + "]");
            }
            return apiKey;
        }

        public static bool HasApiKey()
        {
            var fileExsists = File.Exists(GetLocalConfig("ApiKey"));
            if (!fileExsists)
            {
                return false;
            }
            var apiKey = ReadLocalConfig("ApiKey");
            return !String.IsNullOrEmpty(apiKey);
        }

        public static bool IsLocalOnly()
        {
            var fileExsists = File.Exists(GetLocalConfig("BaseCmsUrl"));
            if (!fileExsists)
            {
                return false;
            }
            var url = ReadLocalConfig("BaseCmsUrl");
            return url.Equals("local");
        }


        private static String GetPubNubSubKey()
        {
            return _config.PubnubSubscribeKey;
        }

        private static String ReadLocalConfig(String config)
        {
            var configFile = GetLocalConfig(config);
            return ReadFile(configFile);
        }

        private static String ReadFile(string filePath)
        {
            var temp = File.ReadAllText(filePath);
            return String.IsNullOrWhiteSpace(temp) ? string.Empty : temp.Trim();
        }

        public static JObject GetConfiguration()
        {
            var x = GetJsonConfig()["config"] as JObject;
            if (!IsLocalOnly())
            {
                x.Add("apiKey", GetApiKey());
                x.Add("cmsUri", Api.CmsUrl);
            }
            else
            {
                x.Add("cmsUri", "local");
            }

        
            return x;
        }

        private static Api.Dto.Config ReadConfig()
        {
            var cacheFile = GetCmsConfigCacheFile();
            if (!File.Exists(cacheFile))
            {
                return new Api.Dto.Config();
            }
            var temp = File.ReadAllText(cacheFile);
            return JsonHelper.ConvertFromApiRequest<Api.Dto.Config>(temp);
        }

        private static JObject GetJsonConfig()
        {
            var cacheFile = GetCmsConfigCacheFile();
            var temp = File.ReadAllText(cacheFile);
            var obj = (JObject) JsonConvert.DeserializeObject(temp);
            return obj;
        }

        public static String ReadConfigOption(String option)
        {

            var obj = GetJsonConfig();
            var optionValue = obj["config"][option];
            var val = optionValue == null ? string.Empty : optionValue.ToString().Trim();
            return val;
        }

        public static void CheckIn()
        {
            if (!IsLocalOnly())
            {
                Api.CheckIn();
            }

        }

        public static void CheckInVersion()
        {
            if (!IsDevMode() && !IsLocalOnly())
            {
                var result = Api.RegisterDeviceVersion();
            }
        }

        public static void UpdateConfigCache(string jsonConfig)
        {
            try
            {
                File.WriteAllText(GetCmsConfigCacheFile(), jsonConfig);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to write cache file for [" + "App" + "] to disk", e);
            }
            _config = ReadConfig();

            SocketServer.SendUpdatedConfig();

            if (OptionChanged != null) OptionChanged();
        }
      

        public static void SetPodFile(string podFile)
        {
            _config.PodFile = podFile;
            if (OptionChanged != null) OptionChanged();
        }


        public static List<PodApp> ListInstalledPods()
        {
            var list = Directory.GetFiles(GetPodLocation(), "*.pod.zip");
            var apps = new List<PodApp>(list.Count());
            apps.AddRange(list.Select(app => new PodApp
            {
                PodName = app
            }));
            apps.Sort((x, y) => String.CompareOrdinal(x.PodName, y.PodName));
            return apps;
        }


        public static async void UpdatePods()
        {
            try
            {
                var downloader = new PodDownloader(GetPodLocation());
                var podList = await Api.GetAppList();
                await downloader.Download(podList);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to download pod list",e);
            }
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
            return _config.TransformSmoothingType;
        }

        public static Boolean IsCalibrationActive()
        {
            return _config.CalibrationActive;
        }

        public static string GetUnitName()
        {
            return _config.Name;
        }

        public static int GetFovCropTop()
        {
            return _config.FovCropTop;
        }
        public static int GetFovCropLeft()
        {
            return _config.FovCropLeft;
        }
        public static int GetFovCropWidth()
        {
            return _config.FovCropWidth;
        }

        public static bool SilhouetteEnabled()
        {
            return _config.SilhouetteEnabled;
        }

        public static async void UpdateAndGetConfigCache()
        {
            try
            {
                Logger.Debug("Updating config");

                var responseAsString = await Api.GetConfig();
                //test its valid json
                _config = JsonHelper.ConvertFromApiRequest<Api.Dto.Config>(responseAsString);
             //   var cacheFile = GetCmsConfigCacheFile();
                UpdateConfigCache(responseAsString);
            }
            catch (Exception e)
            {
                //Happens when the unit is offline
                Logger.Warn("Unable to update the cacheconfig file", e);
            }

           
        }

        public static string GetEnvironment()
        {
            return _config.Environment;
        }
    }
}
