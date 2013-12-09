using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Admo.Api;
using Admo.Api.Dto;
using Admo.classes;
using Admo.classes.lib;
using Admo.forms;
using AdmoShared.Utilities;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace Admo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public App()
        {
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var errorMessage = string.Format("An unhandled exception occurred: {0}", e.Exception.Message);
            Logger.Error(errorMessage,e.Exception);
            Logger.Error(e.Exception);
            e.Handled = true;
        }


       
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            var systeminfo = HardwareUtils.GetSystemInfo();
            Logger.Debug("====HARDWAREINFO===");
            Version version = Assembly.GetEntryAssembly().GetName().Version;
             Logger.Debug("Version: "+version);

            Logger.Debug(String.Format("Manufacturer: {0}, Model: {1}, CPU: {2}",systeminfo.Manufacturer,systeminfo.Model,systeminfo.ProcessorType));
            Logger.Debug(String.Format("OSVersion: {0}, Raw: {1}, Is64Bit: {2}",systeminfo.OsVersion,systeminfo.OsVersionRaw, systeminfo.Is64BitOperatingSystem));
            Logger.Debug(String.Format("TotalMemory: {0}",Utils.BytesToHuman(systeminfo.TotalMemory)));
            Logger.Debug("====HARDWAREINFO===");
            classes.Config.InitDirs();
            
            var hasConfig = classes.Config.HasApiKey();
            hasConfig = hasConfig || classes.Config.IsBaseCmsUrlLocal();
            if (hasConfig)
            {
                var mainWindow = new MainWindow();

                mainWindow.WindowState = WindowState.Minimized;
                mainWindow.Show(); 
            }
            else
            {
                var bootstrapWindow = new BootstrapUnit();

                Logger.Info("ApiKey not found loading bootstrapscreen");
                bootstrapWindow.Show();
            }
        }
    }
}
