using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admo.classes.lib.tasks
{
    public class BrowserKeepAlive : BaseTask
    {
        private readonly LifeCycle _lifeCycle;

        public BrowserKeepAlive(LifeCycle lifeCycle)
        {
            _lifeCycle = lifeCycle;
        }

        public override void Perform()
        {
            _lifeCycle.CheckBrowserState();
        }
    }
}
