using System;
using System.Windows.Threading;

namespace ChatCorporaAnnotator.Models.Timers
{
    internal class MemoryCleaninigTimer : Timer
    {
        private const int DEFAULT_INTERVAL = 10 * 1;

        public MemoryCleaninigTimer(int interval = DEFAULT_INTERVAL, DispatcherPriority priority = DispatcherPriority.Background) : base(interval, priority)
        {
            _timer.Tick += (object sender, EventArgs e) => CleanMemory();
        }

        public virtual void CleanNow()
        {
            CleanMemory();
        }

        public virtual void FastCleanNow()
        {
            GC.Collect(0);
        }

        protected virtual void CleanMemory()
        {
            GC.Collect();
        }
    }
}
