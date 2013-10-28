using NLog;

namespace Admo.classes.lib.commands
{
    public class UnknownCommand : BaseCommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override void Perform()
        {
            //Null opt command does nohting.
        }
    }
}
