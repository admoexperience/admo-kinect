using System;
using System.IO;
using NLog;

namespace Admo.classes
{
    class Config
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const bool RunningInDev = false;
 

        //variable dictating whether facetracking is activated
        public static readonly bool RunningFacetracking = false;
        
        private const String StartUpUrl = @"http://localhost:3000";
        private const String BaseDropboxFolder = @"C:\Dropbox\Admo-Units\";

        public static String GetHostName()
        {
            return Environment.MachineName;
        }

        public static Boolean IsDevMode()
        {
            return RunningInDev;
        }

        public static String GetStartUpUrl()
        {
            return StartUpUrl;
        }

        public static String GetBaseConfigPath()
        {
            return BaseDropboxFolder;
        }

        public static String GetCurrentApp()
        {
            StreamReader objReader = new StreamReader(BaseDropboxFolder + @"\\" + Environment.MachineName + @"\App.txt");
            var appName = objReader.ReadLine();
            objReader.Close();
            return appName;
        }

        public static int GetElevationAngle()
        {
            StreamReader objReader = new StreamReader(BaseDropboxFolder+ @"\\" + Environment.MachineName+ @"\Elevation.txt");
            String temp = objReader.ReadLine();
            objReader.Close();
            int elevationAngle = Convert.ToInt32(temp);
            Log.Info("elevation path: " + elevationAngle);
            return elevationAngle;

        }

        public static String GetStatusFile()
        {
            return BaseDropboxFolder +  @"\\" + Environment.MachineName+ @"\Status.txt";
        }
    }
}
