using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Admo.Api;
using Admo.classes.lib;
using Admo.classes.stats;
using Admo.Utilities;
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
        private const String PodFolder = @"C:\smartroom\pods\";
        

        private static CmsApi Api { get; set; }
    

        public static Boolean IsOnline = false;

        public static PushNotification Pusher;

        //Event handler when a config option changed.
        //Currently can't pick up which config event changed.
        public static event ConfigOptionChanged OptionChanged;
        public delegate void ConfigOptionChanged();


        private static Api.Dto.Config _config;

        public static StatsEngine StatsEngine;

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
            baseDir.CreateSubdirectory(Path.Combine("webserver","current"));
            baseDir.CreateSubdirectory(Path.Combine("webserver", "override"));
        }

        public static string GetPodLocation()
        {
            return Path.Combine(GetBaseConfigPath(), "pods");
        }


        public static void Init()
        {
            Api = new CmsApi(GetApiKey());
            _config = ReadConfig();
            UpdateConfigCache();
            Pusher = new PushNotification
            {
                Channel = GetApiKey(), 
                SubscribeKey = GetPubNubSubKey(),
                OnConnection = OnPushNotificationConnection
            };
            Pusher.Connect();
            


            var pod = new PodWatcher(GetPodFile(), PodFolder);
            pod.StartWatcher();
            pod.Changed += NewWebContent;
            OptionChanged += pod.OnConfigChange;

            var mixpanel = new Mixpanel(GetMixpanelApiKey(), GetMixpanelApiToken());
            var dataCache = new DataCache(Path.Combine(GetBaseConfigPath(),"analytics"));
            StatsEngine = new StatsEngine(dataCache, mixpanel);

            //Async task to download pods in the background
            //TODO: handle events and not always only on app start up
            UpdatePods();
        }

        public static void OnPushNotificationConnection(Boolean online)
        {
            IsOnline = online;
            if (!online) return;
            StatsEngine.ProcessOfflineCache();
            UpdateConfigCache();
            Api.CheckIn();
        }



        public static void NewWebContent(String file)
        {
            Logger.Debug("New server data "+ file);
            SocketServer.SendReloadEvent();
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
            return Path.Combine(appData, "Admo");
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

            return Path.Combine(GetPodLocation(),_config.PodFile);  
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

        public static String GetMixpanelApiToken()
        {
            return _config.Analytics.MixpanelApiToken;  
        }

        public static String GetMixpanelApiKey()
        {
            return _config.Analytics.MixpanelApiKey;  
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
            x.Add("apiKey", GetApiKey());
            x.Add("cmsUri", CmsApi.CmsUrl);
            return x;
        }

        private static Api.Dto.Config ReadConfig()
        {
            var cacheFile = GetCmsConfigCacheFile();
            var temp = File.ReadAllText(cacheFile);
            return JsonHelper.ConvertFromApiRequest<Api.Dto.Config>(temp);
        }

        private static JObject GetJsonConfig()
        {
            var cacheFile = GetCmsConfigCacheFile();
            var temp = File.ReadAllText(cacheFile);
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
                _config = JsonHelper.ConvertFromApiRequest<Api.Dto.Config>(responseAsString);
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
    }
}
