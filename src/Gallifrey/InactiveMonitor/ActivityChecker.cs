using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Gallifrey.JiraTimers;
using Gallifrey.Settings;

namespace Gallifrey.InactiveMonitor
{
    public class ActivityChecker
    {
        public event EventHandler NoActivityEvent;
        private readonly JiraTimerCollection timerCollection;
        private readonly Timer hearbeat;
        private  AppSettings appSettings;
        private Stopwatch noTimerRunning;

        internal ActivityChecker(JiraTimerCollection timerCollection, AppSettings appSettings)
        {
            this.timerCollection = timerCollection;
            this.appSettings = appSettings;
            
            noTimerRunning = new Stopwatch();

            hearbeat = new Timer(500);
            hearbeat.Elapsed += HearbeatOnElapsed;

            if (appSettings.AlertWhenNotRunning)
            {
                hearbeat.Start();    
            }            
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
            }
            else
            {
                if (noTimerRunning.ElapsedMilliseconds >= appSettings.AlertTimeMilliseconds)
                {
                    noTimerRunning.Reset();
                    var handler = NoActivityEvent;
                    if (handler != null) handler(this, EventArgs.Empty);
                }
            }
        }
    }
}
