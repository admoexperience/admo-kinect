using System;
using System.IO;
using NLog;

namespace Admo.classes
{
    class Config
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        //variable dictating whether facetracking is activated
        public static readonly bool RunningFacetracking = false;

        private static String _enviroment = null;
        private static String _webServer = null;
        private const String BaseDropboxFolder = @"C:\Dropbox\Admo-Units\";

        public static String GetHostName()
        {
            return Environment.MachineName;
        }

        //Production mode by default. 
        //Text field can be used to change enviroment only once per startup
        public static Boolean IsDevMode()
        {
            if (_enviroment == null)
            {
                _enviroment = ReadConfig("Enviroment");
                if (_enviroment.Equals(String.Empty))
                {
                    _enviroment = "production";
                }
            }
            return _enviroment.Equals("development");
        }

        public static String GetWebServer()
        {
            if (_webServer != null) return _webServer;
            _webServer = ReadConfig("WebServer");
            //If the config option isn't there return the default value
            if (_webServer.Equals(String.Empty))
            {
                _webServer = "https://localhost:3001";
            }
            return _webServer;
        }

        public static String GetBaseConfigPath()
        {
            return BaseDropboxFolder;
        }

        public static String GetCurrentApp()
        {
            var appName = ReadConfig("App");
            return appName;
        }

        public static int GetElevationAngle()
        {
            var temp = ReadConfig("Elevation");
            var elevationAngle = Convert.ToInt32(temp);
            Log.Info("elevation path: " + elevationAngle);
            return elevationAngle;

        }

        public static String GetStatusFile()
        {
            return BaseDropboxFolder +  @"\\" + Environment.MachineName+ @"\Status.txt";
        }

        private static String ReadConfig(String config)
        {
            var configFile = BaseDropboxFolder + @"\\" + Environment.MachineName + @"\" + config + ".txt";
            String temp;
            try
            {
                var objReader =
                    new StreamReader(configFile);
                 temp = objReader.ReadLine();
                objReader.Close();
            }
            catch (FileNotFoundException fnfe)
            {
                Log.Debug("Config file not found ["+configFile+"]");
                return String.Empty;
            }
            return temp == null ? string.Empty : temp.Trim();
        }
    }
}
