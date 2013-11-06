using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Admo.Api;

namespace Admo.classes.lib.commands
{
    public class UpdatePods : BaseCommand
    {
        public override void Perform()
        {
            Config.UpdatePods();
        }
    }
}
