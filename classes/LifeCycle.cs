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
using Microsoft.Kinect;

namespace Admo
{
    class LifeCycle
    {
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
        public static String startup_url = "http://localhost:3000";
        public static bool running_in_dev = false;

        public static String newDropboxFolder = @"C:\Dropbox\Admo-Units\";

        public static void Startup(String mode)
        {
            double current_time = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
            double time_diff = current_time - startup_time;
                        
            if (mode != "dev")
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
                    objReader = new StreamReader(app_path);
                    app_name = objReader.ReadLine();
                    objReader.Close();
                    startup_stage2 = true;                    
                }
                else if ((time_diff > 25000) && (startup_stage3 == false))
                {
                    //Start default browser (Chrome)
                    //startup_url = startup_url + "/" + app_name;
                    Process.Start(startup_url + "/" + app_name);
                    //start websocket server
                    startup_stage3 = true;
                    SocketServer.Start_SocketIOClient(startup_url);                    
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
                running_in_dev = true;
                if ((time_diff > 10) && (startup_stage1 == false))
                {

                    SocketServer.Start_SocketIOClient(startup_url);

                    SocketServer.server_running = true;
                    Application_Handler.fov_top = 28 * 2;
                    Application_Handler.fov_left = 26 * 2;
                    Application_Handler.fov_height = 205 * 2;
                    Application_Handler.fov_width = 205 * 2 * 4 / 3;
                    startup_stage1 = true;

                    objReader = new StreamReader(app_path);
                    app_name = objReader.ReadLine();
                    objReader.Close();
                }
                else if ((time_diff > 20000) && (startup_stage2 == false))
                {
                    startup_stage2 = true;
                }
            }           
                        
            
        }

        public static StreamWriter objWriter;
        public static StreamReader objReader;
        public static String status_path;
        public static String elevation_path;
        public static String app_path;
        public static void Activate_Monitor()
        {
            var dbPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dropbox\\host.db");
            var dbBase64Text = Convert.FromBase64String(System.IO.File.ReadAllText(dbPath));
            var folderPath = System.Text.ASCIIEncoding.ASCII.GetString(dbBase64Text);
            String mac_path = Environment.MachineName;
            String temp_path = Convert.ToString(folderPath) + @"\Interactive Advertising\Software\Monitoring\" + mac_path;
            int path_start = temp_path.IndexOf("C");
            temp_path = temp_path.Substring(path_start);

            bool IsExists = System.IO.Directory.Exists(temp_path);
            if (!IsExists)
                System.IO.Directory.CreateDirectory(temp_path);

            bool NewSystem = System.IO.Directory.Exists(newDropboxFolder + mac_path);
            if (NewSystem)
            {
                Console.WriteLine("Using new dropbox location at [" + newDropboxFolder + "]");
                temp_path = newDropboxFolder + mac_path;
            }
            else 
            {
                Console.WriteLine("Using old dropbox location at [" + temp_path + "]");
            }

            status_path = temp_path + @"\Status.txt";
            app_path = temp_path + @"\App.txt";
            elevation_path = temp_path + @"\Elevation.txt";
            
            
        }

        public static double browser_time = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
        public static bool monitor_write = true;
        public static bool restart_browser = false;
        public static String app_name = "demo";
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
                    objWriter = new StreamWriter(status_path);
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

                if ((time_diff > 30000) && (restart_browser == false) && (running_in_dev==false))
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
                    objReader = new StreamReader(app_path);
                    String temp_app_name = objReader.ReadLine();
                    objReader.Close();
                    if (app_name != temp_app_name)
                    {
                        Console.WriteLine("Changing app from ["+app_name+"] to ["+temp_app_name+"]");
                        app_name = temp_app_name;
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
                Process.Start(startup_url + "/" + app_name);
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
                Console.WriteLine(Application.Current.Windows.Count);
            }
        }

    }
}
