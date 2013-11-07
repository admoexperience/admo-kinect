using System;
using System.Timers;
using System.Diagnostics;
using Admo.classes;
using  Admo.classes.lib.tasks;
using Admo.Utilities;
using NLog;

namespace Admo
{
    public class LifeCycle
    {
        private const string Browser = "Chrome";
        private const string BrowserExe = Browser + ".exe";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly double _startupTime = Utils.GetCurrentTimeInSeconds();
         

        internal enum StartupStage
        {
            Startup,
            ClosingStartup, 
            LaunchingApp,
            AppRunning
        }
        private  StartupStage _currentStartupStage = StartupStage.Startup;

        internal enum RestartingStage
        {
            None,
            StartupUrl,
            StartupUrlClosed,
            AppStarted
        }

        private  RestartingStage _currentRestartingStage = RestartingStage.None;

        private double _restartTime = Utils.GetCurrentTimeInSeconds();
        private  Boolean _restartingBrowser = false;
        private double _browserTime = Utils.GetCurrentTimeInSeconds();

        private  Process _startupProcess;
        private  Process _applicationBrowserProcess;


        private const double StartupInt = 1000;
        private const double MonitorInt = 1000;
        
        //TODO: Put these on a single task that triggers other tasks
        private readonly Timer _monitorTimer = new Timer(MonitorInt);
        private readonly Timer _startUpTimer= new Timer(StartupInt);


        private readonly CheckInTask _checkInTask = new CheckInTask();
        private  readonly ScreenshotTask _screenshotTask = new ScreenshotTask();
        

        public void ActivateTimers()
        {
            _startUpTimer.Elapsed += StartUpTimer;
            _monitorTimer.Elapsed += MonitorTimer;
            _startUpTimer.Start();
            _monitorTimer.Start();

            //TODO: Handle config changes to the tasks screenshot interval can be configured
            _checkInTask.Start(Api.Dto.Config.CheckingInterval);
            _screenshotTask.Start(Config.GetScreenshotInterval());
        }

        private void StartUpTimer(object sender, ElapsedEventArgs eNotUsed)
        {
            var currentTime = Utils.GetCurrentTimeInSeconds();
            var timeDiff = currentTime - _startupTime;
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
                        Log.Debug("Launching browser with the " + Config.GetLaunchUrl());
                        _applicationBrowserProcess = LaunchBrowser(Config.GetLaunchUrl());
                        _currentStartupStage = StartupStage.AppRunning;
                        //set the last accessed time to now.
                        _browserTime = Utils.GetCurrentTimeInSeconds();
                        MouseDriver.HideTaskBarAndMouse();
                    }
                    break;
            }
        }

        private static Process LaunchBrowser(String url)
        {
            return Process.Start(BrowserExe, "--kiosk " + url);
        }

        private  Boolean IsBrowserRunning()
        {
            var currentTime = Utils.GetCurrentTimeInSeconds();
            var timeDiff = currentTime - _browserTime;
            //Browser has reported in last 20 seconds
            return timeDiff  < 20;
        }

        
         private void MonitorTimer(object sender, ElapsedEventArgs eNotUsed)
         {
            //Don't do any of this in dev mode.
            if (Config.IsDevMode()) return;

            //Only monitor browser stuff when it is actually started
            if (_currentStartupStage != StartupStage.AppRunning) return;

            var shouldRestartBrowser = false;
           

            //If browser hasn't reported in 30 seconds restart it
            if (!IsBrowserRunning())
            {
                shouldRestartBrowser = true;
                Log.Debug("Setting shouldRestartBrowser to true");
            }
            
            if (shouldRestartBrowser && !_restartingBrowser)
            {
                _restartTime = Utils.GetCurrentTimeInSeconds();
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



        private  void RestartBrowser()
        {
            var currentTime = Utils.GetCurrentTimeInSeconds();
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
                        Log.Info("ReLaunching [" + Config.GetLaunchUrl() + "]");
                        _applicationBrowserProcess = LaunchBrowser(Config.GetLaunchUrl());
                        //Set the last browser reported time to be now so it doesn't do this loop again before it checks in.
                        _browserTime = currentTime;
                        _restartingBrowser = false;
                        _currentRestartingStage = RestartingStage.AppStarted;
                        MouseDriver.HideTaskBarAndMouse();
                    }
                    break;
            }                      
        }

        public void SetBrowserTimeNow()
        {
            _browserTime = Utils.GetCurrentTimeInSeconds();
        }
    }
}
