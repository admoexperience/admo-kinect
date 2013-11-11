using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace Admo.Utilities
{
    public class SoftwareUtils
    {

        public static string GetChromeVersion()
        {
            var path = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", "", null);
            if (path != null)
                return FileVersionInfo.GetVersionInfo(path.ToString()).FileVersion;
            return String.Empty;
        }
    }
}
