namespace Admo.classes.lib.tasks
{
    public class CheckInTask : BaseTask
    {
        public override void Perform()
        {
            Config.CheckIn();
        }
    }
}
