using System;
using System.Collections.Generic;
using Admo.classes.lib.commands;
using NLog;
using Newtonsoft.Json;

namespace Admo.classes.lib
{
    public class CommandFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<string, BaseCommand> Commmands = new Dictionary<string, BaseCommand>()
        {
            { "screenshot", new ScreenshotCommand()},
            { "checkin", new CheckinCommand()},
            { "updateConfig", new UpdateConfigCommand()},
            { "calibrate", new CalibrateCommand()},
        };

        public static BaseCommand ParseCommand(string rawCommand)
        {
            dynamic rawOjbect = JsonConvert.DeserializeObject(rawCommand);
            string cmd = rawOjbect.command;
            if (Commmands.ContainsKey(cmd))
            {
                return Commmands[cmd];
            }
            Logger.Error("Unkown command ["+cmd+"]");
            return new UnknownCommand();
        }
    }

}
