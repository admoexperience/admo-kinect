using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Admo
{
    class Cursor_Handler
    {

        //variables for setting the cursor to invisible in registry
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        private const int SPI_SETCURSORS = 0x0057;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDCHANGE = 0x02;

        public static void Hide()
        {
            //Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Cursors", "Arrow", @"C:\Users\Gys\Documents\Dev\smartroom-gestures\Cursor1.cur");
            Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Cursors", "Arrow", @"");
            SystemParametersInfo(SPI_SETCURSORS, 0, null, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        public static void Show()
        {
            Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Cursors", "Arrow", @"");
            SystemParametersInfo(SPI_SETCURSORS, 0, null, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        public static void Custom()
        {
            //Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Cursors", "Arrow", @"C:\Users\Gys\Documents\Dev\smartroom-gestures\Cursor5.cur");
            Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Cursors", "Arrow", @"");
            SystemParametersInfo(SPI_SETCURSORS, 0, null, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

    }
}
