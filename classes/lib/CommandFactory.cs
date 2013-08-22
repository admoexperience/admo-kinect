using System;
using System.Collections.Generic;
using Admo.classes.lib.commands;
using NLog;
using Newtonsoft.Json;

namespace Admo.classes.lib
{
    class CommandFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<string, Type> Commmands = new Dictionary<string, Type>()
        {
            { "screenshot", typeof(ScreenshotCommand)},
            { "checkin", typeof(CheckinCommand)},
            { "updateConfig", typeof(UpdateConfigCommand)},
            { "calibrate", typeof(CalibrateCommand)},
        };


        public static BaseCommand ParseCommand(string rawCommand)
        {
            dynamic rawOjbect = JsonConvert.DeserializeObject(rawCommand);
            string cmd = rawOjbect.command;
            if (Commmands.ContainsKey(cmd))
            {
                return (BaseCommand) Activator.CreateInstance(Commmands[cmd]);
            }
            Logger.Error("Unkown command ["+cmd+"]");
            return new UnknownCommand();
        }
    }

}
