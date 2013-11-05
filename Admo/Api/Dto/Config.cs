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
        public String MixpanelApiToken { get; set; }
        public String MixpanelApiKey { get; set;  }
    }

    public class Config
    {
        public const int CheckingInterval = 5 * 60; //Once every 5mins
        public String Environment { get; set; }
        public String WebUiServer { get; set; }
        public String PodFile { get; set; }
        public int KinectElevation { get; set; }
        public String PubnubSubscribeKey { get; set; }
        public int ScreenshotInterval { get; set; }
        public Boolean CalibrationActive { get; set; }
        public String Name { get;set; }
        public ConfigAnalytics Analytics { get; set; }
        public String TransformSmoothingType { get; set; }
        public String FovCropTop { get; set; }
        public String FovCropLeft { get; set; }
        public String FovCropWidth { get; set; }


        public Config()
        {
            //Put default values here
            Environment = "production";
            PodFile = Path.Combine(classes.Config.GetBaseConfigPath(),"pods", "default.pod.zip");
            KinectElevation = 1;
            //PubnubSubscribeKey doesn't have a default
            ScreenshotInterval = 30*60; //every 30mins
            TransformSmoothingType = "avatar";
        }

    }
}
