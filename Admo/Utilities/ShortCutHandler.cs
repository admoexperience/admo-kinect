using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Class handles shortcuts and sending them
using System.Windows;
using System.Windows.Input;
using Admo.classes;
using NLog;

namespace Admo.Utilities
{
    public class ShortCutHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Window _mainWindow;
        private readonly List<HotKey> _hotKeys = new List<HotKey>();
        public ShortCutHandler(Window main)
        {
            _mainWindow = main;
            _hotKeys.Add(CreateKey(Key.F1));
            _hotKeys.Add(CreateKey(Key.F2));
            _hotKeys.Add(CreateKey(Key.F3));
            _hotKeys.Add(CreateKey(Key.F4));
            _hotKeys.Add(CreateKey(Key.F5));
            _hotKeys.Add(CreateKey(Key.F6));
            _hotKeys.Add(CreateKey(Key.F7));
            _hotKeys.Add(CreateKey(Key.F8));
            _hotKeys.Add(CreateKey(Key.F9));
            _hotKeys.Add(CreateKey(Key.F10));
        }


        ~ShortCutHandler()
        {
            foreach (var key in _hotKeys)
            {
                key.Dispose();
            }
        }
        private HotKey CreateKey(Key x)
        {
            var hotKey = new HotKey(ModifierKeys.Control | ModifierKeys.Shift, x, _mainWindow);
            hotKey.HotKeyPressed += OnKeyPress;
            return hotKey;
        }

        private void OnKeyPress(HotKey key)
        {
             Logger.Debug("Key Pressed "+ (int)key.Key);
            //Magic to Convert F1 into index 0 and F10 into index 9
            var index =((int)key.Key - (int) Key.F1);
            var pods = Config.ListInstalledPods();
            if (index >= 0 && index < pods.Count)
            {
                Config.SetPodFile(pods[index].PodName);
            }
            else
            { 
                Logger.Warn("Key combo is not linked to a pod file");
            }
        }
    }
}
