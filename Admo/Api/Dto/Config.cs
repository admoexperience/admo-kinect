using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admo.Api.Dto
{
    public class ConfigAnalytics
    {
        public string MixpanelApiToken { get; set; }
        public string MixpanelApiKey { get; set; }

        public ConfigAnalytics()
        {
            MixpanelApiKey = string.Empty;
            MixpanelApiToken = string.Empty;
        }
    }

    public class Config : BaseApiResult
    {
        public const int CheckingInterval = 5 * 60; //Once every 5mins
        public string Environment { get; set; }
        public string WebUiServer { get; set; }
        public string PodFile { get; set; }
        public int KinectElevation { get; set; }
        public string PubnubSubscribeKey { get; set; }
        public int ScreenshotInterval { get; set; }
        public bool CalibrationActive { get; set; }
        public string Name { get; set; }
        public ConfigAnalytics Analytics { get; set; }
        public string TransformSmoothingType { get; set; }
        public int FovCropTop { get; set; }
        public int FovCropLeft { get; set; }
        public int FovCropWidth { get; set; }


        public Config()
        {
            //Put default values here
            Environment = "production";
            Analytics = new ConfigAnalytics();
            //Use bundled default app untill they publish a new one.
            var exeLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(exeLocation);
            PodFile = Path.Combine(path,"resources", "default.pod.zip");
            KinectElevation = 1;
            WebUiServer = "https://localhost:5001";
            //PubnubSubscribeKey doesn't have a default
            ScreenshotInterval = 30*60; //every 30mins
            TransformSmoothingType = "avatar";
            FovCropTop = 56;
            FovCropLeft = 52;
            FovCropWidth = 547;
        }
    }
}
