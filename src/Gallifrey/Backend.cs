using System;
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
        
        public Backend()
        {
            AppSettings = AppSettingsSerializer.DeSerialize();
            JiraConnnectionSettings = JiraConnectionSettingsSerializer.DeSerialize();
            JiraTimerCollection = new JiraTimerCollection();
            IdleTimerCollection = new IdleTimerCollection();
            ActivityChecker = new ActivityChecker(JiraTimerCollection, AppSettings);
            ActivityChecker.NoActivityEvent += OnNoActivityEvent;
        }

        internal void OnNoActivityEvent(object sender, EventArgs e)
        {
            var handler = NoActivityEvent;
            if (handler != null) handler(sender, e);
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
