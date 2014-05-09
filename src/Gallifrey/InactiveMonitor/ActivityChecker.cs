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
        private readonly JiraTimerCollection timerCollection;
        private readonly Timer hearbeat;
        private readonly Stopwatch noTimerRunning;
        private  AppSettings appSettings;
        private int eventsSent;

        internal ActivityChecker(JiraTimerCollection timerCollection, AppSettings appSettings)
        {
            this.timerCollection = timerCollection;           
            noTimerRunning = new Stopwatch();

            hearbeat = new Timer(500);
            hearbeat.Elapsed += HearbeatOnElapsed;

            UpdateAppSettings(appSettings);
        }

        internal void UpdateAppSettings(AppSettings newAppSettings)
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
            if (timerCollection.GetRunningTimerId().HasValue)
            {
                noTimerRunning.Stop();
                noTimerRunning.Reset();
                eventsSent = 0;
            }
            else
            {
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
