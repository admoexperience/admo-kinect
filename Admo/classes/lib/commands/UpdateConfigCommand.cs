namespace Admo.classes.lib.commands
{
    public class UpdateConfigCommand : BaseCommand
    {
        public override void Perform()
        {
            Config.UpdateAndGetConfigCache();
        }

    }
}
