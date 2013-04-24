using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Admo.classes
{
    class Config
    {
        private Logger log = LogManager.GetCurrentClassLogger();
        private const bool RunningInDev = false;
        public static StreamWriter objWriter;
        public static StreamReader objReader;
        
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
            objReader = new StreamReader(BaseDropboxFolder+@"App.txt");
            var appName = objReader.ReadLine();
            objReader.Close();
            return appName;
        }
    }
}
