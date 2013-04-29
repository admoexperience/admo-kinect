using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using System.Management;
using System.DirectoryServices;
using System.IO;
using Admo.classes;
using Microsoft.Kinect;
using NLog;

namespace Admo
{
    class LifeCycle
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static double StartupTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
        public static bool StartupStage1 = false;
        public static bool StartupStage2 = false;
        public static bool StartupStage3 = false;
        public static bool StartupStage4 = false;
        public static bool StartupStage5 = false;
        public static bool RestartStage1 = false;
        public static bool RestartStage2 = false;
        public static bool RestartStage3 = false;
        public static bool RestartStage4 = false;
        public static bool RestartStage5 = false;


        public static double RestartTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
        public static double BrowserTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
        public static bool MonitorWrite = true;
        public static bool RestartBrowser = false;
        public static String AppName = "demo";
        

        public static void Startup()
        {
            double currentTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
            double timeDiff = currentTime - StartupTime;
            
            SocketServer.StartServer();
            
            if (! Config.IsDevMode())
            {
                if ((timeDiff > 10000)&&(StartupStage1 == false))
                {
                    //Start default browser (Chrome)
                    
                    Process.Start("http://www.google.com");
                    StartupStage1 = true;
                }
                else if ((timeDiff > 20000)&&(StartupStage2 == false))
                {
                    KeyboardDriver.Exit();
                    AppName = Config.GetCurrentApp();
                    StartupStage2 = true;                    
                }
                else if ((timeDiff > 25000) && (StartupStage3 == false))
                {
                    //Start default browser (Chrome)
                    //startup_url = startup_url + "/" + app_name;
                    Process.Start(Config.GetStartUpUrl() + "/" + AppName);
                    //start websocket server
                    StartupStage3 = true;
                    SocketServer.StartServer();           

                    Application_Handler.fov_top = 28 * 2;
                    Application_Handler.fov_left = 26 * 2;
                    Application_Handler.fov_height = 205 * 2;
                    Application_Handler.fov_width = 205 * 2 * 4 / 3;
                }
                else if ((timeDiff > 35000) && (StartupStage4 == false))
                {
                    //fullscreen chrome                
                    KeyboardDriver.FullScreen();
                    StartupStage4 = true;
                }
                else if ((timeDiff > 40000) && (StartupStage5 == false))
                {
                    
                    //use mousedriver to allow for webcam            
                    MouseDriver.AllowCameraAccess();
                    StartupStage5 = true;
                }                                            

                
            }
            else
            {
                if ((timeDiff > 10) && (StartupStage1 == false))
                {
                    Application_Handler.fov_top = 28 * 2;
                    Application_Handler.fov_left = 26 * 2;
                    Application_Handler.fov_height = 205 * 2;
                    Application_Handler.fov_width = 205 * 2 * 4 / 3;
                    StartupStage1 = true;


                    AppName = Config.GetCurrentApp();
                }
                else if ((timeDiff > 20000) && (StartupStage2 == false))
                {
                    StartupStage2 = true;
                }
            }           
                        
            
        }

     


        
        public static void Monitor()
        {
            double currentTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
            double timeDiff = currentTime - BrowserTime;

            String datetime = DateTime.Now.ToString("HH:mm:ss tt");
            int min = Convert.ToInt32(datetime.Substring(3, 2));
            min = min % 3;

            //Monitor whether system is online
            if ((min == 1) && MonitorWrite)
            {
                try
                {
                    var objWriter = new StreamWriter(Config.GetStatusFile());
                    objWriter.WriteLine(DateTime.Now.ToString());
                    objWriter.Close();
                    MonitorWrite = false;
                }
                catch (Exception et)
                {
                    Log.Warn("Could not write status to the status file "+ Config.GetStatusFile());
                }
            }
            else if ((min == 2) && (MonitorWrite == false))
            {
                MonitorWrite = true;
            }


            //only run when system is running
            if (StartupStage5  && (RestartBrowser == false))
            {

                if ((timeDiff > 30000) && (RestartBrowser == false) && (!Config.IsDevMode()))
                {
                    RestartStage1 = false;
                    RestartStage2 = false;
                    RestartStage3 = false;
                    RestartStage4 = false;
                    RestartStage5 = false;
                    RestartTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
                    RestartBrowser = true;
                }

                //Monitor which app is supposed to be run
                try
                {

                    String tempAppName = Config.GetCurrentApp();
                    if (AppName != tempAppName)
                    {
                        Log.Info("Changing app from [" + AppName + "] to [" + tempAppName + "]");
                        AppName = tempAppName;
                        RestartStage1 = false;
                        RestartStage2 = false;
                        RestartStage3 = false;
                        RestartStage4 = false;
                        RestartStage5 = false;
                        RestartTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
                        RestartBrowser = true;
                    }

                }
                catch (Exception ex)
                {
                }
            }

            if (RestartBrowser)
            {
                Restart_Browser();
            }
            
        }


        
        public static void Restart_Browser()
        {
            double currentTime = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
            double timeDiff = currentTime - RestartTime;

            if ((timeDiff > 2000) && (RestartStage1 == false))
            {
                //Start default browser (Chrome)
                RestartStage1 = true;
                Process.Start("http://www.google.com");                
            }
            else if ((timeDiff > 4000) && (RestartStage2 == false))
            {
                RestartStage2 = true;
                KeyboardDriver.Exit();
            }
            else if ((timeDiff > 6000) && (RestartStage3 == false))
            {
                //Start default browser (Chrome)
                RestartStage3 = true;
                //startup_url = startup_url + "/" + app_name;
                Process.Start(Config.GetStartUpUrl() + "/" + AppName);
                //start websocket server                
            }
            else if ((timeDiff > 8000) && (RestartStage4 == false))
            {
                //fullscreen chrome                
                KeyboardDriver.FullScreen();
                RestartStage4 = true;
            }
            else if ((timeDiff > 10000) && (RestartStage5 == false))
            {
                //use mousedriver to allow for webcam            
                MouseDriver.AllowCameraAccess();
                RestartStage5 = true;
                RestartBrowser = false;
            }                                         
        }

        //restart PC
        public static void Restart()
        {
            String datetime = DateTime.Now.ToString("HH:mm:ss tt");
            int hour = Convert.ToInt32(datetime.Substring(0, 2));
            int min = Convert.ToInt32(datetime.Substring(3, 2));

            if ((hour == 20) && (min == 59))
            {
                Process.Start("shutdown.exe", "/r /t 10");
                Application.Current.Windows[0].Close();
                Log.Info(Application.Current.Windows.Count);
            }
        }

    }
}
