using Gallifrey.JiraTimers;
using Gallifrey.Settings;
using System;
using System.Diagnostics;
using System.Timers;

namespace Gallifrey.InactiveMonitor
{
    public class ActivityChecker
    {
        internal event EventHandler<int> NoActivityEvent;

        private readonly IJiraTimerCollection timerCollection;
        private readonly ISettingsCollection settingsCollection;
        private readonly ActivityStopwatch activityStopwatch;
        private readonly object lockObject;

        public TimeSpan Elapsed => activityStopwatch.Elapsed;

        internal ActivityChecker(IJiraTimerCollection timerCollection, ISettingsCollection settingsCollection)
        {
            activityStopwatch = new ActivityStopwatch();
            this.timerCollection = timerCollection;
            this.settingsCollection = settingsCollection;
            lockObject = new object();

            var hearbeat = new Timer(500);
            hearbeat.Elapsed += HearbeatOnElapsed;
            hearbeat.Start();
        }

        private void HearbeatOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (lockObject)
            {
                if (!settingsCollection.AppSettings.AlertWhenNotRunning)
                {
                    activityStopwatch.Reset();
                    return;
                }

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

                    if (activityStopwatch.Elapsed >= TimeSpan.FromMilliseconds(settingsCollection.AppSettings.AlertTimeMilliseconds))
                    {
                        NotifyNoActivity();
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
            activityStopwatch.Reset();
            NotifyNoActivity();
        }

        public void SetValue(TimeSpan value)
        {
            activityStopwatch.StartWithSeed(value);
            NotifyNoActivity();
        }

        private void NotifyNoActivity()
        {
            NoActivityEvent?.Invoke(this, (int)activityStopwatch.Elapsed.TotalMilliseconds);
        }

        private class ActivityStopwatch
        {
            private readonly Stopwatch timer;
            private TimeSpan manualAdjustmentValue;

            public TimeSpan Elapsed => timer.Elapsed.Add(manualAdjustmentValue);
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
                    manualAdjustmentValue = manualAdjustmentValue.Add(manualAdjustTimeSpan.Value.Negate());
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

            public void StartWithSeed(TimeSpan value)
            {
                timer.Reset();
                timer.Start();
                manualAdjustmentValue = value;
            }
        }
    }
}
