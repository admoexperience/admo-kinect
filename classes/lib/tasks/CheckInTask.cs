using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Admo.classes.lib.tasks
{
    public class CheckInTask : BaseTask
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public override void Perform()
        {
            Logger.Debug("Timer checkin");
            Config.CheckIn();
        }
    }
}
