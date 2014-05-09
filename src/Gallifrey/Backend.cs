using System;
using System.Timers;
using Gallifrey.IdleTimers;
using Gallifrey.InactiveMonitor;
using Gallifrey.IntegrationPoints;
using Gallifrey.JiraTimers;
using Gallifrey.Serialization;
using Gallifrey.Settings;

namespace Gallifrey
{
    public class Backend
    {
        public JiraTimerCollection JiraTimerCollection;
        public IdleTimerCollection IdleTimerCollection;
        public AppSettings AppSettings;
        public JiraConnnectionSettings JiraConnnectionSettings;
        public JiraConnection JiraConnection;
        public event EventHandler NoActivityEvent;
        internal ActivityChecker ActivityChecker;
        private readonly Timer hearbeat;
        
        public Backend()
        {
            AppSettings = AppSettingsSerializer.DeSerialize();
            JiraConnnectionSettings = JiraConnectionSettingsSerializer.DeSerialize();
            JiraTimerCollection = new JiraTimerCollection();
            IdleTimerCollection = new IdleTimerCollection();
            ActivityChecker = new ActivityChecker(JiraTimerCollection, AppSettings);
            ActivityChecker.NoActivityEvent += OnNoActivityEvent;
            hearbeat = new Timer(3600000);
            hearbeat.Elapsed += HearbeatOnElapsed;
            hearbeat.Start();
        }

        private void OnNoActivityEvent(object sender, EventArgs e)
        {
            var handler = NoActivityEvent;
            if (handler != null) handler(sender, e);
        }

        private void HearbeatOnElapsed(object sender, ElapsedEventArgs e)
        {
            JiraTimerCollection.RemoveTimersOlderThanDays(AppSettings.KeepTimersForDays);
            IdleTimerCollection.RemoveOldTimers();
        }

        public void Initialise()
        {
            JiraConnection = new JiraConnection(JiraConnnectionSettings);
        }

        public void Close()
        {
            JiraTimerCollection.SaveTimers();
            IdleTimerCollection.SaveTimers();
            AppSettings.SaveSettings();
            JiraConnnectionSettings.SaveSettings();
        }

        public void SaveJiraConnectionSettings()
        {
            JiraConnnectionSettings.SaveSettings();
            if (JiraConnection == null)
            {
                JiraConnection = new JiraConnection(JiraConnnectionSettings);
            }
            else
            {
                JiraConnection.ReConnect(JiraConnnectionSettings);
            }
        }

        public void SaveAppSettings()
        {
            AppSettings.SaveSettings();
            ActivityChecker.UpdateAppSettings(AppSettings);
        }

        public void StartIdleTimer()
        {
            IdleTimerCollection.NewLockTimer();
        }

        public void StopIdleTimer()
        {
            IdleTimerCollection.StopLockedTimers();
        }
    }
}
