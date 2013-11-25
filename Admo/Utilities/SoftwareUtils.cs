using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace Admo.Utilities
{
    public class SoftwareUtils
    {

        public static string GetChromeVersion()
        {
            //Handle both system wide and user installs of chrome
            var pathLocal = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", "", null);
            var pathMachine = Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", "", null);
            if (pathLocal != null)
            {
                return FileVersionInfo.GetVersionInfo(pathLocal.ToString()).FileVersion;   
            }
            if (pathMachine != null)
            {
                return FileVersionInfo.GetVersionInfo(pathMachine.ToString()).FileVersion;   
            }
            return String.Empty;
        }
    }
}
