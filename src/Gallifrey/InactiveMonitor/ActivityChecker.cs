using System;
using System.Diagnostics;
using System.Timers;
using Gallifrey.JiraTimers;
using Gallifrey.Settings;

namespace Gallifrey.InactiveMonitor
{
    public class ActivityChecker
    {
        public event EventHandler<int> NoActivityEvent;
        private readonly IJiraTimerCollection timerCollection;
        private readonly Timer hearbeat;
        private readonly Stopwatch noTimerRunning;
        private readonly object lockObject;
        private IAppSettings appSettings;
        private int eventsSent;
        
        internal ActivityChecker(IJiraTimerCollection timerCollection, IAppSettings appSettings)
        {
            this.timerCollection = timerCollection;
            noTimerRunning = new Stopwatch();
            lockObject = new object();

            hearbeat = new Timer(500);
            hearbeat.Elapsed += HearbeatOnElapsed;

            UpdateAppSettings(appSettings);
        }

        internal void UpdateAppSettings(IAppSettings newAppSettings)
        {
            appSettings = newAppSettings;
            if (appSettings.AlertWhenNotRunning)
            {
                hearbeat.Start();
            }
            else
            {
                hearbeat.Stop();
                noTimerRunning.Stop();
                noTimerRunning.Reset();
            }
        }

        private void HearbeatOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (lockObject)
            {
                if (timerCollection.GetRunningTimerId().HasValue)
                {
                    noTimerRunning.Stop();
                    noTimerRunning.Reset();
                    eventsSent = 0;
                }
                else
                {
                    if (!noTimerRunning.IsRunning) noTimerRunning.Start();
                    if (noTimerRunning.ElapsedMilliseconds >= appSettings.AlertTimeMilliseconds)
                    {
                        eventsSent++;
                        noTimerRunning.Reset();
                        var handler = NoActivityEvent;
                        if (handler != null) handler(this, eventsSent * appSettings.AlertTimeMilliseconds);
                    }
                }
            }
        }
    }
}
