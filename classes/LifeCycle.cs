using System;
using System.Globalization;
using System.Windows;
using System.Diagnostics;
using System.IO;
using Admo.classes;
using NLog;

namespace Admo
{
    class LifeCycle
    {
        private const string BrowserExe = "Chrome.exe";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static double StartupTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
        public static bool StartupStage1 = false;
        public static bool StartupStage2 = false;


        public static bool RestartStage1 = false;
        public static bool RestartStage2 = false;

        public static double RestartTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
        private static Boolean RestartingBrowser = false;
        public static double BrowserTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;

        public static bool MonitorWrite = true;
       
        public static String AppName = "demo";

        private static Process StartupProcess;
        private static Process ApplicationBrowserProcess;


        public static void Startup()
        {
            var currentTime = Convert.ToDouble(DateTime.Now.Ticks)/10000;
            var timeDiff = currentTime - StartupTime;
            AppName = Config.GetCurrentApp();
            //Set the kinect and webcam config
            Application_Handler.fov_top = 28*2;
            Application_Handler.fov_left = 26*2;
            Application_Handler.fov_height = 205*2;
            Application_Handler.fov_width = 205*2*4/3;

            SocketServer.StartServer();

            //in dev mode do nothing.
            if (Config.IsDevMode()) return;

            if ((timeDiff > 10000) && (StartupStage1 == false))
            {

                StartupProcess = LaunchBrowser(Config.GetStartUpUrl());
                StartupStage1 = true;
            }
            else if ((timeDiff > 25000) && (StartupStage2 == false))
            {
                //startup_url = startup_url + "/" + app_name;
                ApplicationBrowserProcess = LaunchBrowser(Config.GetStartUpUrl() + "/" + AppName);
                StartupStage2 = true;
            }
        }

        public static Process LaunchBrowser(String url)
        {
            return Process.Start(BrowserExe,"--kiosk "+ url);
        }

        public static Boolean IsBrowserRunning()
        {
            var currentTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
            var timeDiff = currentTime - BrowserTime;
            //Browser has reported in last 30 seconds
            return timeDiff  < 30000;
        }

        public static void LogStatus()
        {
            var datetime = DateTime.Now.ToString("HH:mm:ss tt");
            var min = Convert.ToInt32(datetime.Substring(3, 2));
            min = min % 3;

            //Monitor whether system is online
            if ((min == 1) && MonitorWrite)
            {
                try
                {
                    var objWriter = new StreamWriter(Config.GetStatusFile());
                    objWriter.WriteLine(DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    objWriter.Close();
                    MonitorWrite = false;
                }
                catch (Exception e)
                {
                    Log.Warn("Could not write status to the status file " + Config.GetStatusFile(),e);
                }
            }
            else if ((min == 2) && (MonitorWrite == false))
            {
                MonitorWrite = true;
            }
        }


        public static void Monitor()
        {
            //Don't do any of this in dev mode.
            if (Config.IsDevMode()) return;

            LogStatus();

            var shouldRestartBrowser = false;
            
            //Check if app has changed if it has restart browser
            var tempAppName = Config.GetCurrentApp();
            if (AppName != tempAppName)
            {
                Log.Info("Changing app from [" + AppName + "] to [" + tempAppName + "]");
                AppName = tempAppName;
                shouldRestartBrowser = true;
            }

            //If browser hasn't reported in 30 seconds restart it
            if (!IsBrowserRunning() && StartupStage2)
            {
                shouldRestartBrowser = true;
            }

            if (shouldRestartBrowser)
            {
                RestartTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
                RestartStage1 = false;
                RestartStage2 = false;
                RestartBrowser();
            }
        }


        
        public static void RestartBrowser()
        {
            RestartingBrowser = true;
            var currentTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
            var timeDiff = currentTime - RestartTime;

            if ((timeDiff > 2000) && (RestartStage1 == false))
            {
                //Start default browser (Chrome)
                RestartStage1 = true;
                Log.Info("Browser last reported in at " + BrowserTime + " restarting it");
                StartupProcess =LaunchBrowser(Config.GetStartUpUrl());                
            }
            else if ((timeDiff > 6000) && (RestartStage2 == false))
            {
                //Start default browser (Chrome)
                RestartStage2 = true;
                Log.Info("Launching [" + AppName + "]");
                ApplicationBrowserProcess = LaunchBrowser(Config.GetStartUpUrl() + "/" + AppName);
                RestartingBrowser = false;
            }                         
        }

        //restart PC
        public static void RestartMachine()
        {
            var datetime = DateTime.Now.ToString("HH:mm:ss tt");
            var hour = Convert.ToInt32(datetime.Substring(0, 2));
            var min = Convert.ToInt32(datetime.Substring(3, 2));

            if ((hour == 20) && (min == 59))
            {
                Process.Start("shutdown.exe", "/r /t 10");
                var window = Application.Current.Windows[0];
                if (window != null) window.Close();
                Log.Info(Application.Current.Windows.Count);
            }
        }

    }
}
