﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admo.classes.lib.tasks
{
    public class ScreenshotTask: BaseTask
    {
        public override void Perform()
        {
            Config.TakeScreenshot();
        }
    }
}
