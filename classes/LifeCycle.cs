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
        public static double StartupTime = GetCurrentTimeInSeconds();

        internal enum StartupStage
        {
            Startup,
            ClosingStartup, 
            LaunchingApp,
            AppRunning
        }
        public static StartupStage CurrentStartupStage = StartupStage.Startup;

        internal enum RestartingStage
        {
            None,
            StartupUrl,
            StartupUrlClosed,
            AppStarted
        }
        public static RestartingStage CurrentRestartingStage = RestartingStage.None;



        public static bool RestartStage1 = false;
        public static bool RestartStage2 = false;

        public static double RestartTime = GetCurrentTimeInSeconds();
        private static Boolean RestartingBrowser = false;
        public static double BrowserTime = GetCurrentTimeInSeconds();

        public static bool MonitorWrite = true;
       
        public static String AppName  = Config.GetCurrentApp();

        private static Process StartupProcess;
        private static Process ApplicationBrowserProcess;

        private static double LastMonitorTime = GetCurrentTimeInSeconds();

        public static void LifeLoop()
        {
                //startup chorme in fullscreen and use mouse driver to allow webcam
                Startup();
                //listen for restarting call
                RestartMachine();
                Monitor();
        }

        private static void Startup()
        {
            var currentTime = GetCurrentTimeInSeconds();
            var timeDiff = currentTime - StartupTime;
            
            //Set the kinect and webcam config
            Application_Handler.fov_top = 28*2;
            Application_Handler.fov_left = 26*2;
            Application_Handler.fov_height = 205*2;
            Application_Handler.fov_width = 205*2*4/3;

            SocketServer.StartServer();

            //in dev mode do nothing.
            if (Config.IsDevMode()) return;

            switch (CurrentStartupStage)
            {
                case StartupStage.Startup:
                    if (timeDiff > 1000)
                    {
                        Log.Debug("Starting up browser with startup url");
                        StartupProcess = LaunchBrowser(Config.GetWebServer());
                        CurrentStartupStage = StartupStage.ClosingStartup;
                    }
                    break;
                case StartupStage.ClosingStartup:
                    if(timeDiff > 23000)
                    {
                        Log.Debug("Closing browser with startup url");
                        //Killing the process directly causes the chrome message
                        try
                        {
                            StartupProcess.CloseMainWindow();
                        }
                        catch (Exception e)
                        {
                            Log.Warn("Start up process has failed to close possible reasons are it has already exited",e);
                        }
                        CurrentStartupStage = StartupStage.LaunchingApp;
                    }
                    break;
                case StartupStage.LaunchingApp:
                    if (timeDiff > 25000)
                    {
                        Log.Debug("Launching browser with the "+ AppName);
                        ApplicationBrowserProcess = LaunchBrowser(Config.GetWebServer() + "/" + AppName);
                        CurrentStartupStage = StartupStage.AppRunning;
                        //set the last accessed time to now.
                        BrowserTime = GetCurrentTimeInSeconds();
                    }
                    break;
            }
        }

        private static double GetCurrentTimeInSeconds()
        {
            return Convert.ToDouble(DateTime.Now.Ticks) / 10000;
        }

        private static Process LaunchBrowser(String url)
        {
            return Process.Start(BrowserExe, "--kiosk " + url);
        }

        private static Boolean IsBrowserRunning()
        {
            var currentTime = GetCurrentTimeInSeconds();
            var timeDiff = currentTime - BrowserTime;
            Log.Debug("Browser time and current time diff " + timeDiff + " stage " + CurrentStartupStage);
            //Browser has reported in last 30 seconds
            return timeDiff  < 30000;
        }

        private static void LogStatus()
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


        private static void Monitor()
        {
            //Don't do any of this in dev mode.
            if (Config.IsDevMode()) return;

            //Only monitor every second.
            var temp = GetCurrentTimeInSeconds();
            if (temp - LastMonitorTime > 1000)
            {
                LastMonitorTime = temp;
            }
            else
            {
                return;
            }

            Log.Debug("Monitoring browser " + BrowserTime);

            LogStatus();

            //Only monitor browser stuff when it is actually started
            if (CurrentStartupStage != StartupStage.AppRunning) return;

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
            if (!IsBrowserRunning())
            {
                shouldRestartBrowser = true;
                Log.Debug("Setting shouldRestartBrowser to true");
            }
            
            if (shouldRestartBrowser && !RestartingBrowser)
            {
                RestartTime = GetCurrentTimeInSeconds();
                RestartStage1 = false;
                RestartStage2 = false;
                RestartingBrowser = true;
                Log.Debug("Attempting to restart the browser " + RestartTime);
               
            }
            if (RestartingBrowser)
            {
                RestartBrowser();
            }
        }


        
        private static void RestartBrowser()
        {
            var currentTime = GetCurrentTimeInSeconds();
            var timeDiff = currentTime - RestartTime;
            Log.Info("Restart time difference " + timeDiff);
            switch(CurrentRestartingStage)
            {
                case RestartingStage.None:
                    if (timeDiff > 2000)
                    {
                        StartupProcess = LaunchBrowser(Config.GetWebServer());
                         CurrentRestartingStage = RestartingStage.StartupUrl;
                    }
                    break;
                case RestartingStage.StartupUrl:
                    if (timeDiff > 3000)
                    {
                        //Killing the process directly causes the chrome message
                        try
                        {
                            StartupProcess.CloseMainWindow();
                        }
                        catch (Exception e)
                        {
                            Log.Warn("Start up process has failed to close possible reasons are it has already exited", e);
                        }
                        CurrentRestartingStage = RestartingStage.StartupUrlClosed;
                    }
                    break;
                case RestartingStage.StartupUrlClosed:
                    if (timeDiff > 6000)
                    {
                        Log.Info("Launching [" + AppName + "]");
                        ApplicationBrowserProcess = LaunchBrowser(Config.GetWebServer() + "/" + AppName);
                        //Set the last browser reported time to be now so it doesn't do this loop again before it checks in.
                        BrowserTime = currentTime;
                        RestartingBrowser = false;
                        CurrentRestartingStage = RestartingStage.AppStarted;
                    }
                    break;
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
