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
            float trueWidth = TheHacks.UncalibratedCoordinates[4] -
                            TheHacks.UncalibratedCoordinates[2];

            //the circles on the calibration app HUD is 340px appart meusared in Kinect coordinates
            float falseWidth = 340;
            float scalingFactor = trueWidth / falseWidth;

            //this is the uncalibrated value of the left hand relative to the left margin
            float trueLeft = TheHacks.UncalibratedCoordinates[2];
            //this value is set in the calibration app to position the calibration circles from the left margin
            float falseLeft = 150;

            //this is the uncalibrated value of the left hand relative to the top margin
            float trueTop = TheHacks.UncalibratedCoordinates[3];
            //this value is set in the calibration app to position the calibration circles from the top margin
            float falseTop = 200;

            Application_Handler.FovWidth = 640 * scalingFactor;
            Application_Handler.FovHeight = 480 * scalingFactor;

            Application_Handler.FovLeft = trueLeft - (falseLeft * scalingFactor);
            Application_Handler.FovTop = trueTop - (falseTop * scalingFactor);

            Logger.Info("Calibration values changed");
            Logger.Info(Config.Keys.FovCropTop + ": " + Application_Handler.FovTop);
            Logger.Info(Config.Keys.FovCropLeft + ": " + Application_Handler.FovLeft);
            Logger.Info(Config.Keys.FovCropWidth + ": " + Application_Handler.FovWidth);
        }
    }
}
