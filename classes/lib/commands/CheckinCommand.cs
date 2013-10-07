
namespace Admo.classes.lib.commands
{
    public class CheckinCommand : BaseCommand
    {
        public override void Perform()
        {
            Config.CheckIn();
        }
    }
}
