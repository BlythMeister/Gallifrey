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
    public interface IBackend
    {
        IJiraTimerCollection JiraTimerCollection { get; }
        IIdleTimerCollection IdleTimerCollection { get; }
        IAppSettings AppSettings { get; }
        IJiraConnectionSettings JiraConnectionSettings { get; }
        IJiraConnection JiraConnection { get; }
        event EventHandler<int> NoActivityEvent;
        void Initialise();
        void Close();
        void SaveJiraConnectionSettings();
        void SaveAppSettings();
        void StartIdleTimer();
        void StopIdleTimer();
    }

    public class Backend : IBackend
    {
        private readonly JiraTimerCollection jiraTimerCollection;
        private readonly IdleTimerCollection idleTimerCollection;
        private readonly AppSettings appSettings;
        private readonly JiraConnectionSettings jiraConnectionSettings;
        private JiraConnection jiraConnection;
        public event EventHandler<int> NoActivityEvent;
        internal ActivityChecker ActivityChecker;
        private readonly Timer hearbeat;
        
        public Backend()
        {
            appSettings = AppSettingsSerializer.DeSerialize();
            jiraConnectionSettings = JiraConnectionSettingsSerializer.DeSerialize();
            jiraTimerCollection = new JiraTimerCollection();
            idleTimerCollection = new IdleTimerCollection();
            ActivityChecker = new ActivityChecker(jiraTimerCollection, appSettings);
            ActivityChecker.NoActivityEvent += OnNoActivityEvent;
            hearbeat = new Timer(3600000);
            hearbeat.Elapsed += HearbeatOnElapsed;
            hearbeat.Start();
        }

        private void OnNoActivityEvent(object sender, int millisecondsSinceActivity)
        {
            var handler = NoActivityEvent;
            if (handler != null) handler(sender, millisecondsSinceActivity);
        }

        private void HearbeatOnElapsed(object sender, ElapsedEventArgs e)
        {
            jiraTimerCollection.RemoveTimersOlderThanDays(appSettings.KeepTimersForDays);
            idleTimerCollection.RemoveOldTimers();
        }

        public void Initialise()
        {
            jiraConnection = new JiraConnection(jiraConnectionSettings);
        }

        public void Close()
        {
            jiraTimerCollection.SaveTimers();
            idleTimerCollection.SaveTimers();
            appSettings.SaveSettings();
            jiraConnectionSettings.SaveSettings();
        }

        public void SaveJiraConnectionSettings()
        {
            jiraConnectionSettings.SaveSettings();
            if (jiraConnection == null)
            {
                jiraConnection = new JiraConnection(jiraConnectionSettings);
            }
            else
            {
                jiraConnection.ReConnect(jiraConnectionSettings);
            }
        }

        public void SaveAppSettings()
        {
            appSettings.SaveSettings();
            ActivityChecker.UpdateAppSettings(appSettings);
        }

        public void StartIdleTimer()
        {
            idleTimerCollection.NewLockTimer();
        }

        public void StopIdleTimer()
        {
            idleTimerCollection.StopLockedTimers();
        }

        public IJiraTimerCollection JiraTimerCollection
        {
            get { return jiraTimerCollection; }
        }

        public IIdleTimerCollection IdleTimerCollection
        {
            get { return idleTimerCollection; }
        }

        public IAppSettings AppSettings
        {
            get { return appSettings; }
        }

        public IJiraConnectionSettings JiraConnectionSettings
        {
            get { return jiraConnectionSettings; }
        }

        public IJiraConnection JiraConnection
        {
            get { return jiraConnection; }
        }
    }
}
