using System;
using System.Diagnostics;
using System.Timers;
using Gallifrey.JiraTimers;
using Gallifrey.Settings;

namespace Gallifrey.InactiveMonitor
{
    public class ActivityChecker
    {
        internal event EventHandler<int> NoActivityEvent;
        private readonly IJiraTimerCollection timerCollection;
        private readonly Timer hearbeat;
        private readonly ActivityStopwatch activityStopwatch;
        private readonly object lockObject;
        private TimeSpan alertLimit;

        internal ActivityChecker(IJiraTimerCollection timerCollection, IAppSettings appSettings)
        {
            activityStopwatch = new ActivityStopwatch();
            this.timerCollection = timerCollection;
            lockObject = new object();

            hearbeat = new Timer(500);
            hearbeat.Elapsed += HearbeatOnElapsed;

            UpdateAppSettings(appSettings);
        }

        internal void UpdateAppSettings(IAppSettings appSettings)
        {
            alertLimit = TimeSpan.FromMilliseconds(appSettings.AlertTimeMilliseconds);

            if (appSettings.AlertWhenNotRunning)
            {
                hearbeat.Start();
            }
            else
            {
                hearbeat.Stop();
                activityStopwatch.Reset();
            }
        }

        private void HearbeatOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (lockObject)
            {
                if (activityStopwatch.IsPaused)
                {
                    return;
                }

                if (timerCollection.GetRunningTimerId().HasValue)
                {
                    if (activityStopwatch.IsRunning)
                    {
                        Reset();
                    }
                }
                else
                {
                    if (!activityStopwatch.IsRunning)
                    {
                        activityStopwatch.Start();
                    }

                    if (activityStopwatch.Elapsed >= alertLimit)
                    {
                        NoActivityEvent?.Invoke(this, (int)activityStopwatch.Elapsed.TotalMilliseconds);
                    }
                }
            }
        }

        public void PauseForLockTimer(TimeSpan? manualAdjustTimeSpan)
        {
            activityStopwatch.Pause(manualAdjustTimeSpan);
        }

        public void ResumeAfterLockTimer()
        {
            activityStopwatch.Resume();
        }

        public void Reset()
        {
            NoActivityEvent?.Invoke(this, 0);
            activityStopwatch.Reset();
        }

        private class ActivityStopwatch
        {
            private readonly Stopwatch timer;
            private TimeSpan manualAdjustmentValue;

            public TimeSpan Elapsed => timer.Elapsed.Subtract(manualAdjustmentValue);
            public bool IsRunning => timer.IsRunning;
            public bool IsPaused { get; private set; }

            public ActivityStopwatch()
            {
                timer = new Stopwatch();
                manualAdjustmentValue = TimeSpan.Zero;
            }

            public void Reset()
            {
                timer.Reset();
                manualAdjustmentValue = TimeSpan.Zero;
            }

            public void Pause(TimeSpan? manualAdjustTimeSpan)
            {
                IsPaused = true;
                timer.Stop();
                if (manualAdjustTimeSpan.HasValue)
                {
                    manualAdjustmentValue = manualAdjustmentValue.Add(manualAdjustTimeSpan.Value);
                }
            }

            public void Resume()
            {
                IsPaused = false;
                timer.Start();
            }

            public void Start()
            {
                Reset();
                timer.Start();
            }
        }
    }
}
