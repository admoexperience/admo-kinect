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
        double screen_width = SystemParameters.PrimaryScreenWidth;
        double screen_height = SystemParameters.PrimaryScreenHeight;
        public static double startup_time = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
        public static bool startup_stage1 = false;
        public static bool startup_stage2 = false;
        public static bool startup_stage3 = false;
        public static bool startup_stage4 = false;
        public static bool startup_stage5 = false;
        public static bool restart_stage1 = false;
        public static bool restart_stage2 = false;
        public static bool restart_stage3 = false;
        public static bool restart_stage4 = false;
        public static bool restart_stage5 = false;


  
        public static double browser_time = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
        public static bool monitor_write = true;
        public static bool restart_browser = false;
        public static String app_name = "demo";
        

        public static void Startup()
        {
            double current_time = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
            double time_diff = current_time - startup_time;
                        
            if (! Config.IsDevMode())
            {
                if ((time_diff > 10000)&&(startup_stage1 == false))
                {
                    //Start default browser (Chrome)
                    
                    Process.Start("http://www.google.com");
                    startup_stage1 = true;
                }
                else if ((time_diff > 20000)&&(startup_stage2 == false))
                {
                    KeyboardDriver.Exit();
                    app_name = Config.GetCurrentApp();
                    startup_stage2 = true;                    
                }
                else if ((time_diff > 25000) && (startup_stage3 == false))
                {
                    //Start default browser (Chrome)
                    //startup_url = startup_url + "/" + app_name;
                    Process.Start(Config.GetStartUpUrl() + "/" + app_name);
                    //start websocket server
                    startup_stage3 = true;
                    SocketServer.Start_SocketIOClient(Config.GetStartUpUrl());                    
                    SocketServer.server_running = true;

                    Application_Handler.fov_top = 28 * 2;
                    Application_Handler.fov_left = 26 * 2;
                    Application_Handler.fov_height = 205 * 2;
                    Application_Handler.fov_width = 205 * 2 * 4 / 3;
                }
                else if ((time_diff > 35000) && (startup_stage4 == false))
                {
                    //fullscreen chrome                
                    KeyboardDriver.FullScreen();
                    startup_stage4 = true;
                }
                else if ((time_diff > 40000) && (startup_stage5 == false))
                {
                    
                    //use mousedriver to allow for webcam            
                    MouseDriver.AllowCameraAccess();
                    startup_stage5 = true;
                }                                            

                
            }
            else
            {
                if ((time_diff > 10) && (startup_stage1 == false))
                {

                    SocketServer.Start_SocketIOClient(Config.GetStartUpUrl());

                    SocketServer.server_running = true;
                    Application_Handler.fov_top = 28 * 2;
                    Application_Handler.fov_left = 26 * 2;
                    Application_Handler.fov_height = 205 * 2;
                    Application_Handler.fov_width = 205 * 2 * 4 / 3;
                    startup_stage1 = true;


                    app_name = Config.GetCurrentApp();
                }
                else if ((time_diff > 20000) && (startup_stage2 == false))
                {
                    startup_stage2 = true;
                }
            }           
                        
            
        }

     


        
        public static void Monitor()
        {
            double current_time = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
            double time_diff = current_time - browser_time;

            String datetime = DateTime.Now.ToString("HH:mm:ss tt");
            int min = Convert.ToInt32(datetime.Substring(3, 2));
            min = min % 3;

            //Monitor whether system is online
            if ((min == 1) && (monitor_write == true))
            {
                try
                {
                    StreamWriter objWriter = new StreamWriter(Config.GetStatusFile());
                    objWriter.WriteLine(DateTime.Now.ToString());
                    objWriter.Close();
                    monitor_write = false;
                }
                catch (Exception et)
                {
                }
            }
            else if ((min == 2) && (monitor_write == false))
            {
                monitor_write = true;
            }


            //only run when system is running
            if ((startup_stage5 == true) && (restart_browser == false))
            {

                if ((time_diff > 30000) && (restart_browser == false) && (!Config.IsDevMode()))
                {
                    restart_stage1 = false;
                    restart_stage2 = false;
                    restart_stage3 = false;
                    restart_stage4 = false;
                    restart_stage5 = false;
                    restart_time = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
                    restart_browser = true;
                }

                //Monitor which app is supposed to be run
                try
                {

                    String tempAppName = Config.GetCurrentApp();
                    if (app_name != tempAppName)
                    {
                        Log.Info("Changing app from [" + app_name + "] to [" + tempAppName + "]");
                        app_name = tempAppName;
                        restart_stage1 = false;
                        restart_stage2 = false;
                        restart_stage3 = false;
                        restart_stage4 = false;
                        restart_stage5 = false;
                        restart_time = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
                        restart_browser = true;
                    }

                }
                catch (Exception ex)
                {
                }
            }

            if (restart_browser == true)
            {
                Restart_Browser();
            }
            
        }


        public static double restart_time = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
        public static void Restart_Browser()
        {
            double current_time = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
            double time_diff = current_time - restart_time;

            if ((time_diff > 2000) && (restart_stage1 == false))
            {
                //Start default browser (Chrome)
                restart_stage1 = true;
                Process.Start("http://www.google.com");                
            }
            else if ((time_diff > 4000) && (restart_stage2 == false))
            {
                restart_stage2 = true;
                KeyboardDriver.Exit();
            }
            else if ((time_diff > 6000) && (restart_stage3 == false))
            {
                //Start default browser (Chrome)
                restart_stage3 = true;
                //startup_url = startup_url + "/" + app_name;
                Process.Start(Config.GetStartUpUrl() + "/" + app_name);
                //start websocket server                
            }
            else if ((time_diff > 8000) && (restart_stage4 == false))
            {
                //fullscreen chrome                
                KeyboardDriver.FullScreen();
                restart_stage4 = true;
            }
            else if ((time_diff > 10000) && (restart_stage5 == false))
            {
                //use mousedriver to allow for webcam            
                MouseDriver.AllowCameraAccess();
                restart_stage5 = true;
                restart_browser = false;
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
