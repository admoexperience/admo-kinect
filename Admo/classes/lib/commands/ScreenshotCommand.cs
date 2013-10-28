
namespace Admo.classes.lib.commands
{
    public class ScreenshotCommand : BaseCommand
    {
        public override void Perform()
        {
            Config.TakeScreenshot();
        }
    }
}
