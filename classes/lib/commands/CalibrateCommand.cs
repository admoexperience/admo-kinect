using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Admo.classes.lib.commands
{
    public class CalibrateCommand :BaseCommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public override void Perform()
        {
            //get the distance the user's hands is appart
            //the user should be hovering his hands over the circles on the HUD at this stage
            float trueWidth = Application_Handler.UncalibratedCoordinates[4] -
                            Application_Handler.UncalibratedCoordinates[2];

            //the circles on the calibration app HUD is 340px appart meusared in Kinect coordinates
            float falseWidth = 340;
            float scalingFactor = trueWidth / falseWidth;

            //this is the uncalibrated value of the left hand relative to the left margin
            float trueLeft = Application_Handler.UncalibratedCoordinates[2];
            //this value is set in the calibration app to position the calibration circles from the left margin
            float falseLeft = 150;

            //this is the uncalibrated value of the left hand relative to the top margin
            float trueTop = Application_Handler.UncalibratedCoordinates[3];
            //this value is set in the calibration app to position the calibration circles from the top margin
            float falseTop = 200;

            Application_Handler.fov_width = 640 * scalingFactor;
            Application_Handler.fov_height = 480 * scalingFactor;

            Application_Handler.fov_left = trueLeft - (falseLeft * scalingFactor);
            Application_Handler.fov_top = trueTop - (falseTop * scalingFactor);

            Logger.Info("Calibration values changed");
            Logger.Info(Config.Keys.FovCropTop + ": " + Application_Handler.fov_top);
            Logger.Info(Config.Keys.FovCropLeft + ": " + Application_Handler.fov_left);
            Logger.Info(Config.Keys.FovCropWidth + ": " + Application_Handler.fov_width);
        }
    }
}
