using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Admo.Api.Dto;
using AdmoShared.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Admo.forms
{


    public partial class OfflineConfig : Window
    {
        private MainWindow _mainWindow;
        public OfflineConfig(MainWindow mw )
        {
            _mainWindow = mw;
            InitializeComponent();
            LoginBox.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(LoginBox_OnMouseLeftButtonDown), true);

        }

        private void LoginBox_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int fovCropLeft;
            Int32.TryParse(FovCropLeft.Text, out fovCropLeft);
            int fovCropTop;
            Int32.TryParse(FovCropTop.Text, out fovCropTop);
            int fovCropWidth;
            Int32.TryParse(FovCropWidth.Text, out fovCropWidth);
            int kinectElevation;
            Int32.TryParse(KinectElevation.Text, out kinectElevation);
            bool silhouetteEnabled;
            Boolean.TryParse(SilhouetteEnabled.Text, out silhouetteEnabled);
            var config = new Api.Dto.Config()
            {
                Environment = Environment.Text,
                FovCropLeft = fovCropLeft,
                FovCropWidth = fovCropWidth,
                FovCropTop = fovCropTop,
                KinectElevation = kinectElevation,
                TransformSmoothingType = SmoothingType.Text,
                SilhouetteEnabled = silhouetteEnabled
                //PodFile = PodFle.Text,
                //WebServerBasePath = WebServerPath.Text,
                //WebUiServer = WebUiServer.Text
            };

            var conWrapper = new ConfigWrapper()
            {
                Config = config
            };
            var x = JsonHelper.ConvertToUnderScore(conWrapper);
            //File.WriteAllText(Path.Combine(classes.Config.GetBaseConfigPath(), "configcache.json"),x);
            classes.Config.UpdateConfigCache(x);
        }

     
    }

    public class ConfigWrapper
    {
        public Api.Dto.Config Config;
    }
}
