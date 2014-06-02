﻿using System;
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
        ISettingsCollection Settings { get; }
        IJiraConnection JiraConnection { get; }
        event EventHandler<int> NoActivityEvent;
        void Initialise();
        void Close();
        void SaveSettings();
        void StartIdleTimer();
        Guid StopIdleTimer();
    }

    public class Backend : IBackend
    {
        private readonly JiraTimerCollection jiraTimerCollection;
        private readonly IdleTimerCollection idleTimerCollection;
        private readonly SettingsCollection settingsCollection;
        private JiraConnection jiraConnection;
        public event EventHandler<int> NoActivityEvent;
        internal ActivityChecker ActivityChecker;
        private readonly Timer hearbeat;
        private Guid? runningTimerWhenIdle;
        
        public Backend()
        {
            settingsCollection = SettingsCollectionSerializer.DeSerialize();
            jiraTimerCollection = new JiraTimerCollection();
            idleTimerCollection = new IdleTimerCollection();
            ActivityChecker = new ActivityChecker(jiraTimerCollection, settingsCollection.AppSettings);
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
            jiraTimerCollection.RemoveTimersOlderThanDays(settingsCollection.AppSettings.KeepTimersForDays);
            idleTimerCollection.RemoveOldTimers();
        }

        public void Initialise()
        {
            jiraConnection = new JiraConnection(settingsCollection.JiraConnectionSettings);
        }

        public void Close()
        {
            jiraTimerCollection.SaveTimers();
            idleTimerCollection.SaveTimers();
            settingsCollection.SaveSettings();
        }

        public void SaveSettings()
        {
            settingsCollection.SaveSettings();
            if (jiraConnection == null)
            {
                jiraConnection = new JiraConnection(settingsCollection.JiraConnectionSettings);
            }
            else
            {
                jiraConnection.ReConnect(settingsCollection.JiraConnectionSettings);
            }

            ActivityChecker.UpdateAppSettings(settingsCollection.AppSettings);
        }

        public void StartIdleTimer()
        {
            ActivityChecker.Reset();

            runningTimerWhenIdle = JiraTimerCollection.GetRunningTimerId();
            if (runningTimerWhenIdle.HasValue)
            {
                jiraTimerCollection.StopTimer(runningTimerWhenIdle.Value);
            }
            idleTimerCollection.NewLockTimer();
            }

        public Guid StopIdleTimer()
        {
            ActivityChecker.Reset();

            if (runningTimerWhenIdle.HasValue)
            {
                jiraTimerCollection.StartTimer(runningTimerWhenIdle.Value);
                runningTimerWhenIdle = null;
            }
            return idleTimerCollection.StopLockedTimers();
        }

        public IJiraTimerCollection JiraTimerCollection
        {
            get { return jiraTimerCollection; }
        }

        public IIdleTimerCollection IdleTimerCollection
        {
            get { return idleTimerCollection; }
        }

        public ISettingsCollection Settings
        {
            get { return settingsCollection; }
        }

        public IJiraConnection JiraConnection
        {
            get { return jiraConnection; }
        }
    }
}
