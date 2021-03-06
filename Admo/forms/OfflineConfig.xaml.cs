﻿using System;
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
     
        public OfflineConfig()
        {
            InitializeComponent();


            classes.Config.UpdateAndGetConfigCache();
            Environment.Text = classes.Config.GetEnvironment();
            WebUiServer.Text = classes.Config.GetWebServer();
            WebServerPath.Text = classes.Config.GetWebServerBasePath();
            PodFle.Text = classes.Config.GetPodFile();
            KinectElevation.Text = classes.Config.GetElevationAngle().ToString();
            SmoothingType.Text = classes.Config.GetTransformSmoothType();
            FovCropTop.Text = classes.Config.GetFovCropTop().ToString();
            FovCropLeft.Text = classes.Config.GetFovCropLeft().ToString();
            FovCropWidth.Text = classes.Config.GetFovCropWidth().ToString();
            SilhouetteEnabled.IsChecked = classes.Config.SilhouetteEnabled();
            PublishButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(PublishButton_OnMouseLeftButtonDown), true);
            PodFle.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(PodFle_OnMouseLeftButtonDown), true);

        }

        private void PodFle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".pod.zip",
                Filter = "Admo Pods (.pod.zip)|*.pod.zip"
            };

            // Show open file dialog box
            var result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                WebServerPath.Text = dlg.FileName;
            }
        }

        private void PublishButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var x = JsonHelper.ConvertToUnderScore(GetConfigObj());
            classes.Config.UpdateConfigCache(x);
        }

        public ConfigWrapper GetConfigObj()
        {
            int fovCropLeft;
            Int32.TryParse(FovCropLeft.Text, out fovCropLeft);
            int fovCropTop;
            Int32.TryParse(FovCropTop.Text, out fovCropTop);
            int fovCropWidth;
            Int32.TryParse(FovCropWidth.Text, out fovCropWidth);
            int kinectElevation;
            Int32.TryParse(KinectElevation.Text, out kinectElevation);

            var silhouetteEnabled = SilhouetteEnabled.IsChecked == true;
            var podFile = PodFle.Text;

            var config = new Api.Dto.Config()
            {
                Environment = Environment.Text,
                FovCropLeft = fovCropLeft,
                FovCropWidth = fovCropWidth,
                FovCropTop = fovCropTop,
                KinectElevation = kinectElevation,
                TransformSmoothingType = SmoothingType.Text,
                SilhouetteEnabled = silhouetteEnabled,
                PodFile = podFile,
                WebServerBasePath = WebServerPath.Text,
                WebUiServer = WebUiServer.Text
            };

            return new ConfigWrapper
            {
                Config = config
            };
        }
     
    }

    public class ConfigWrapper
    {
        public Api.Dto.Config Config;
    }
}
