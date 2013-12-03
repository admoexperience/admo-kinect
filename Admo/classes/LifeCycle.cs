using System;
using System.Diagnostics;
using System.Timers;
using Admo.classes.lib.tasks;
using AdmoShared.Utilities;
using NLog;

namespace Admo.classes
{
    public class LifeCycle
    {
        private const string Browser = "Chrome";
        private const string BrowserExe = Browser + ".exe";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
      
        private double _browserTime = Utils.GetCurrentTimeInSeconds();

        private readonly CheckInTask _checkInTask = new CheckInTask();
        private readonly ScreenshotTask _screenshotTask = new ScreenshotTask();
        private BrowserKeepAlive _browserKeepAlive;
        

        public void ActivateTimers()
        {
            //Do not launch browser in dev mode
            if (!Config.IsDevMode())
            {
                //Start the browser
                KillAllBrowserWindows();
                LaunchBrowser(Config.GetLaunchUrl());
                //Once its started use a monitor to check it is still connected and alive.
                _browserKeepAlive = new BrowserKeepAlive(this);
                _browserKeepAlive.Start(30);
            }

            //TODO: Handle config changes to the tasks screenshot interval can be configured
            _checkInTask.Start(Api.Dto.Config.CheckingInterval);
            _screenshotTask.Start(Config.GetScreenshotInterval());
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

        
         public void CheckBrowserState()
         { 
            //If browser hasn't reported in 30 seconds restart it
            if (!IsBrowserRunning() && Config.GetPodFile() != new Api.Dto.Config().PodFile)
            {

                Log.Debug("Attempting to restart the browser");
                _browserTime = Utils.GetCurrentTimeInSeconds();
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

        private void RestartBrowser()
        {
            KillAllBrowserWindows();
            LaunchBrowser(Config.GetLaunchUrl());                  
        }

        public void SetBrowserTimeNow()
        {
            _browserTime = Utils.GetCurrentTimeInSeconds();
        }
    }
}
