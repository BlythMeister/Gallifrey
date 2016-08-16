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
        private readonly Stopwatch noTimerRunning;
        private readonly object lockObject;
        private bool temporaryStopActivityCheck;
        private TimeSpan alertLimit;

        internal ActivityChecker(IJiraTimerCollection timerCollection, IAppSettings appSettings)
        {
            this.timerCollection = timerCollection;
            noTimerRunning = new Stopwatch();
            temporaryStopActivityCheck = false;
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
                noTimerRunning.Reset();
            }
        }

        private void HearbeatOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (lockObject)
            {
                if (temporaryStopActivityCheck)
                {
                    return;
                }

                if (timerCollection.GetRunningTimerId().HasValue)
                {
                    if (noTimerRunning.IsRunning)
                    {
                        Reset();
                    }
                }
                else
                {
                    if (!noTimerRunning.IsRunning)
                    {
                        noTimerRunning.Start();
                    }

                    if (noTimerRunning.Elapsed >= alertLimit)
                    {
                        NoActivityEvent?.Invoke(this, (int)noTimerRunning.ElapsedMilliseconds);
                    }
                }
            }
        }

        public void StopActivityCheckForLockTimer()
        {
            temporaryStopActivityCheck = true;
            noTimerRunning.Stop();
        }

        public void RestartActivityCheckAfterLockTimer()
        {
            temporaryStopActivityCheck = false;
        }

        public void Reset()
        {
            NoActivityEvent?.Invoke(this, 0);
            noTimerRunning.Reset();
        }
    }
}
