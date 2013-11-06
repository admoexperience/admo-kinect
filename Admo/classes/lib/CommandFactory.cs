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
            { "updatePods",new UpdatePods()},
        };

        //Null opt command that does nothing is returned when we cant parse the command
        private static readonly UnknownCommand  UnknownCommand = new UnknownCommand();

        public static BaseCommand ParseCommand(string rawCommand)
        {
            dynamic rawOjbect = JsonConvert.DeserializeObject(rawCommand);
            string cmd = rawOjbect.command;
            if (Commmands.ContainsKey(cmd))
            {
                return Commmands[cmd];
            }
            Logger.Warn("Unkown command ["+cmd+"]");
            return UnknownCommand;
        }
    }

}
