using System.Timers;

//Allows for you to perform tasks at a given interval and allowing you to control them.
//Very loose wrapper around built in timer task.
namespace Admo.classes.lib.tasks
{
    public abstract class  BaseTask
    {
        private Timer _timer;

        public void Start(int intervalInSeconds)
        {
            _timer = new Timer(intervalInSeconds * 1000);
            _timer.Elapsed += OnTimer;
            _timer.Start();
        }

        public void Restart(int intervalInSeconds)
        {
            _timer.Enabled = false;
            _timer.Dispose();
            Start(intervalInSeconds);
        }

        private void OnTimer(object sender, ElapsedEventArgs e)
        {
           Perform();
        }

        public abstract void Perform();
    }
}
