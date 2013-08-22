using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
