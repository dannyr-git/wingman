namespace wingman.Helpers
{
    using Microsoft.UI.Xaml;
    using System;

    public class AudioTimer
    {
        private DispatcherTimer _timer;
        private TimeSpan _elapsedTime;

        public AudioTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _elapsedTime = TimeSpan.Zero;
        }

        public void Start()
        {
            _elapsedTime = TimeSpan.Zero;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
        }

        public int ElapsedTimeInSeconds
        {
            get { return _elapsedTime.Seconds; }
        }

        private void Timer_Tick(object sender, object e)
        {
            _elapsedTime += _timer.Interval;
            // Raise an event to notify subscribers of the elapsed time
            TickEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler TickEvent;
    }

}
