using System;
using System.Globalization;
using System.Timers;
using System.Windows;
using System.Diagnostics;
using System.IO;
using Admo.classes;
using NLog;
using System.Linq;

namespace Admo
{
    class LifeCycle
    {
        private const string Browser = "Chrome";
        private const string BrowserExe = Browser + ".exe";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly double StartupTime = GetCurrentTimeInSeconds();
         

        internal enum StartupStage
        {
            Startup,
            ClosingStartup, 
            LaunchingApp,
            AppRunning
        }
        private static StartupStage _currentStartupStage = StartupStage.Startup;

        internal enum RestartingStage
        {
            None,
            StartupUrl,
            StartupUrlClosed,
            AppStarted
        }

        public static RestartingStage _currentRestartingStage = RestartingStage.None;

        private static double _restartTime = GetCurrentTimeInSeconds();
        public static Boolean _restartingBrowser = false;
        private static double _browserTime = GetCurrentTimeInSeconds();

        private static bool _monitorWrite = true;

        private static String _appName = Config.GetCurrentApp();
        
        private static Process _startupProcess;
        private static Process _applicationBrowserProcess;

        private static double _lastMonitorTime = GetCurrentTimeInSeconds();
        private static double _lastCheckinTime = _lastMonitorTime;
        private static double _lastScreenshotTime = _lastMonitorTime;

        private const double StartupInt = 3000;
        private const double MonitorInt = 2000;
        private readonly Timer _monitorTimer = new Timer(MonitorInt);
        private readonly Timer _startUpTimer= new Timer(StartupInt);
        

        public void ActivateTimers()
        {

            //_lifeCycleStartUpTimer = new System.Timers.Timer(StartupInt);
            _startUpTimer.Elapsed += StartUpTimer;
            _monitorTimer.Elapsed += MonitorTimer;
            _startUpTimer.Start();
            _monitorTimer.Start();


        }

        private void MonitorTimer(object sender, ElapsedEventArgs e)
        {
            Monitor();
        }

        private void StartUpTimer(object sender, ElapsedEventArgs e)
        {
            Startup();
        }

        private  void LifeLoop()
        {
                //startup chorme in fullscreen and use mouse driver to allow webcam
                Startup();
                Monitor();
        }

        private void Startup()
        {
            var currentTime = GetCurrentTimeInSeconds();
            var timeDiff = currentTime - StartupTime;
            /*
            //Set the kinect and webcam config
            Application_Handler.fov_top = 28*2;
            Application_Handler.fov_left = 26*2;
            Application_Handler.fov_height = 205*2;
            Application_Handler.fov_width = 205*2*4/3;
            */
            //in dev mode do nothing.
            if (Config.IsDevMode()) return;

            switch (_currentStartupStage)
            {
                case StartupStage.Startup:
                    if (timeDiff > 10)
                    {
                        //Force kill any chrome windows that may or may not be running.
                        KillAllBrowserWindows();
                        Log.Debug("Starting up browser with startup url");
                        _startupProcess = LaunchBrowser(Config.GetLoadingPage());
                        _currentStartupStage = StartupStage.ClosingStartup;
                    }
                    break;
                case StartupStage.ClosingStartup:
                    if(timeDiff > 23)
                    {
                        Log.Debug("Closing browser with startup url");
                        //Killing the process directly causes the chrome message
                        try
                        {
                            _startupProcess.CloseMainWindow();
                        }
                        catch (Exception e)
                        {
                            Log.Warn("Start up process has failed to close possible reason it has already exited",e);
                        }
                        _currentStartupStage = StartupStage.LaunchingApp;
                    }
                    break;
                case StartupStage.LaunchingApp:
                    if (timeDiff > 25)
                    {
                        Log.Debug("Launching browser with the "+ _appName);
                        _applicationBrowserProcess = LaunchBrowser(Config.GetWebServer() + "/" + _appName);
                        _currentStartupStage = StartupStage.AppRunning;
                        //set the last accessed time to now.
                        _browserTime = GetCurrentTimeInSeconds();
                        MouseDriver.HideTaskBarAndMouse();
                    }
                    break;
            }
        }

        public static double GetCurrentTimeInSeconds()
        {
            return Convert.ToDouble(DateTime.Now.Ticks) / 10000 / 1000;
        }

        private static Process LaunchBrowser(String url)
        {
            return Process.Start(BrowserExe, "--kiosk " + url);
        }

        private static Boolean IsBrowserRunning()
        {
            var currentTime = GetCurrentTimeInSeconds();
            var timeDiff = currentTime - _browserTime;
            //Browser has reported in last 20 seconds
            return timeDiff  < 20;
        }

        private static void Checkin()
        {
            //Only monitor every second.
            var temp = GetCurrentTimeInSeconds();
            if (!(temp - _lastCheckinTime > Config.CheckingInterval)) return;
            
            _lastCheckinTime = temp;
            Config.CheckIn();
           
        }

        private static void Screenshot()
        {
            //Only monitor every second.
            var temp = GetCurrentTimeInSeconds();
            if (!(temp - _lastScreenshotTime > Config.GetScreenshotInterval())) return;

            _lastScreenshotTime = temp;
            Config.TakeScreenshot();

        }


        private void Monitor()
        {
            

            //Only monitor every second.
            var temp = GetCurrentTimeInSeconds();

            if (temp - _lastMonitorTime > 1)
            {
                _lastMonitorTime = temp;
            }
            else
            {
                return;
            }

            Checkin();
            Screenshot();
           
            //Don't do any of this in dev mode.
            if (Config.IsDevMode()) return;

            //Only monitor browser stuff when it is actually started
            if (_currentStartupStage != StartupStage.AppRunning) return;

            var shouldRestartBrowser = false;
            
            //Check if app has changed if it has restart browser
            var tempAppName = Config.GetCurrentApp();
            if (_appName != tempAppName)
            {
                Log.Info("Changing app from [" + _appName + "] to [" + tempAppName + "]");
                _appName = tempAppName;
                shouldRestartBrowser = true;
            }

            //If browser hasn't reported in 30 seconds restart it
            if (!IsBrowserRunning())
            {
                shouldRestartBrowser = true;
                Log.Debug("Setting shouldRestartBrowser to true");
            }
            
            if (shouldRestartBrowser && !_restartingBrowser)
            {
                _restartTime = GetCurrentTimeInSeconds();
                _restartingBrowser = true;
                _currentRestartingStage = RestartingStage.None;
                Log.Debug("Attempting to restart the browser " + _restartTime);
               
            }
            if (_restartingBrowser)
            {
                RestartBrowser();
            }
        }

        private static void KillAllBrowserWindows()
        {
            var proc = Process.GetProcessesByName(Browser);
            Log.Debug("Found "+ proc.Length+ " browser proccessing trying to kill them all");
            foreach (var b in proc)
            {
                Log.Debug("Killing " + b);
                TryKillBrowser(b, true);
                b.WaitForExit();
            }
        }

        private static void TryKillBrowser(Process browerPid, Boolean force)
        {
            if (browerPid == null) return;
            try
            {
                browerPid.CloseMainWindow();
            }
            catch (Exception)
            {
                Log.Warn("Could not close browser");
            }

            //Force killing doesn't gracefully close the browser it hard kills it.

            if (!force) return;

            try
            {
                browerPid.Kill();
            }
            catch (Exception)
            {
                Log.Warn("Could not force close browser");
            }
        }



        private static void RestartBrowser()
        {
            var currentTime = GetCurrentTimeInSeconds();
            var timeDiff = currentTime - _restartTime;
            switch(_currentRestartingStage)
            {
                case RestartingStage.None:
                    if (timeDiff > 2)
                    {
                        //Try kill both before doing any thing
                        TryKillBrowser(_startupProcess,true);
                        TryKillBrowser(_applicationBrowserProcess, true);
                        Log.Info("RestartingStage.None");
                        _startupProcess = LaunchBrowser(Config.GetLoadingPage());
                        _currentRestartingStage = RestartingStage.StartupUrl;
                    }
                    break;
                case RestartingStage.StartupUrl:
                    if (timeDiff > 3)
                    {
                        //Killing the process directly causes the chrome message
                        Log.Info("RestartingStage.StartupUrl");
                        TryKillBrowser(_startupProcess, false);
                        _currentRestartingStage = RestartingStage.StartupUrlClosed;
                    }
                    break;
                case RestartingStage.StartupUrlClosed:
                    if (timeDiff > 6)
                    {
                        Log.Info("ReLaunching [" + _appName + "]");
                        _applicationBrowserProcess = LaunchBrowser(Config.GetWebServer() + "/" + _appName);
                        //Set the last browser reported time to be now so it doesn't do this loop again before it checks in.
                        _browserTime = currentTime;
                        _restartingBrowser = false;
                        _currentRestartingStage = RestartingStage.AppStarted;
                        MouseDriver.HideTaskBarAndMouse();
                    }
                    break;
            }                      
        }

        public static void SetBrowserTimeNow()
        {
            _browserTime = GetCurrentTimeInSeconds();
        }
    }
}
