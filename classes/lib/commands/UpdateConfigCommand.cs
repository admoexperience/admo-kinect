using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admo.classes.lib.commands
{
    public class UpdateConfigCommand : BaseCommand
    {
        public override void Perform()
        {
            Config.UpdateConfigCache();
        }

    }
}
