using ChatCorporaAnnotator.Infrastructure.AppEventArgs;
using ChatCorporaAnnotator.Infrastructure.Enums;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ChatCorporaAnnotator.Models.Timers
{
    internal class SavingTimer : Timer
    {
        private const int MIN_SAVING_TIME = 1500;
        private const int DEFAULT_INTERVAL = 60 * 1;

        public delegate void TickHandler();
        public event TickHandler Tick;

        public delegate void SuccessfulIterationHandler();
        public event SuccessfulIterationHandler SuccessfulIteration;

        public delegate void FailedIterationHandler(Exception ex);
        public event FailedIterationHandler FailedIteration;

        public delegate void SavingStateChangedHandler(ProjectSavingStateEventArgs e);
        public event SavingStateChangedHandler SavingStateChanged;

        private SaveProjectState _savingState;
        public SaveProjectState SavingState
        {
            get => _savingState;
            set
            {
                var oldState = _savingState;
                var newState = value;

                _savingState = value;

                var args = new ProjectSavingStateEventArgs(oldState, newState);
                SavingStateChanged?.Invoke(args);
            }
        }

        public SavingTimer(int interval = DEFAULT_INTERVAL, DispatcherPriority priority = DispatcherPriority.Background) : base(interval, priority)
        {
            _timer.Tick += (object sender, EventArgs e) => _ = SaveAsync();
        }

        public void SaveNow()
        {
            _ = SaveAsync();
        }

        protected virtual async Task SaveAsync()
        {
            if (SavingState == SaveProjectState.InProcess)
                return;

            SavingState = SaveProjectState.InProcess;

            await Task.Run(delegate
            {
                try
                {
                    var stopwatch = new Stopwatch();

                    stopwatch.Start();
                    Tick?.Invoke();
                    stopwatch.Stop();

                    if (stopwatch.ElapsedMilliseconds < MIN_SAVING_TIME)
                        Thread.Sleep((int)(MIN_SAVING_TIME - stopwatch.ElapsedMilliseconds));

                    SuccessfulIteration?.Invoke();
                }
                catch (Exception ex)
                {
                    SavingState = SaveProjectState.ChangesNotSaved;
                    FailedIteration?.Invoke(ex);
                }
            });
        }
    }
}
