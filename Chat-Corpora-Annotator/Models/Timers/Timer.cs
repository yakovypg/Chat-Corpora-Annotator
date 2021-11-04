using System;
using System.Windows.Threading;

namespace ChatCorporaAnnotator.Models.Timers
{
    internal abstract class Timer : ITimer
    {
        protected readonly DispatcherTimer _timer;

        public bool IsActive { get; protected set; }

        public Timer(int interval, DispatcherPriority priority = DispatcherPriority.Background)
        {
            _timer = new DispatcherTimer(priority)
            {
                Interval = new TimeSpan(0, 0, 0, interval)
            };
        }

        public virtual void Start()
        {
            _timer.Start();
            IsActive = true;
        }

        public virtual void Stop()
        {
            _timer.Stop();
            IsActive = false;
        }
    }
}
