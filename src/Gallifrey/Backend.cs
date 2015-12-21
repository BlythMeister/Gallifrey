using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Xml.Linq;
using Gallifrey.AppTracking;
using Gallifrey.ChangeLog;
using Gallifrey.Contributors;
using Gallifrey.Exceptions;
using Gallifrey.Exceptions.IdleTimers;
using Gallifrey.IdleTimers;
using Gallifrey.InactiveMonitor;
using Gallifrey.JiraIntegration;
using Gallifrey.JiraTimers;
using Gallifrey.Serialization;
using Gallifrey.Settings;
using Gallifrey.Versions;

namespace Gallifrey
{
    public interface IBackend
    {
        IJiraTimerCollection JiraTimerCollection { get; }
        IIdleTimerCollection IdleTimerCollection { get; }
        ISettingsCollection Settings { get; }
        IJiraConnection JiraConnection { get; }
        IVersionControl VersionControl { get; }
        List<WithThanksDefinition> WithThanksDefinitions { get; }
        event EventHandler<int> NoActivityEvent;
        event EventHandler<ExportPromptDetail> ExportPromptEvent;
        event EventHandler DailyTrackingEvent;
        void Initialise();
        void Close();
        void TrackEvent(TrackingType trackingType);
        void SaveSettings(bool jiraSettingsChanged);
        bool StartIdleTimer();
        Guid StopIdleTimer();
        IEnumerable<ChangeLogVersion> GetChangeLog(XDocument changeLogContent);
    }

    public class Backend : IBackend
    {
        private readonly JiraTimerCollection jiraTimerCollection;
        private readonly IdleTimerCollection idleTimerCollection;
        private readonly SettingsCollection settingsCollection;
        private readonly ITrackUsage trackUsage;
        private readonly JiraConnection jiraConnection;
        private readonly VersionControl versionControl;
        private readonly WithThanksCreator withThanksCreator;

        public event EventHandler<int> NoActivityEvent;
        public event EventHandler<ExportPromptDetail> ExportPromptEvent;
        public event EventHandler DailyTrackingEvent;
        internal ActivityChecker ActivityChecker;
        private Guid? runningTimerWhenIdle;

        public Backend(InstanceType instanceType, AppType appType)
        {
            settingsCollection = SettingsCollectionSerializer.DeSerialize();
            trackUsage = new TrackUsage(settingsCollection.AppSettings, settingsCollection.InternalSettings, instanceType, appType);
            versionControl = new VersionControl(instanceType, appType, trackUsage);
            jiraTimerCollection = new JiraTimerCollection(settingsCollection.ExportSettings);
            jiraTimerCollection.exportPrompt += OnExportPromptEvent;
            jiraConnection = new JiraConnection(trackUsage);
            idleTimerCollection = new IdleTimerCollection();
            ActivityChecker = new ActivityChecker(jiraTimerCollection, settingsCollection.AppSettings);
            withThanksCreator = new WithThanksCreator();

            ActivityChecker.NoActivityEvent += OnNoActivityEvent;
            var cleanUpAndTrackingHearbeat = new Timer(1800000); // 30 minutes
            cleanUpAndTrackingHearbeat.Elapsed += CleanUpAndTrackingHearbeatOnElapsed;
            cleanUpAndTrackingHearbeat.Start();
            var jiraExportHearbeat = new Timer(120000); //2 minutes
            jiraExportHearbeat.Elapsed += JiraExportHearbeatHearbeatOnElapsed;
            jiraExportHearbeat.Start();

            if (Settings.AppSettings.TimerRunningOnShutdown.HasValue)
            {
                var timer = jiraTimerCollection.GetTimer(Settings.AppSettings.TimerRunningOnShutdown.Value);
                if (timer != null && timer.DateStarted.Date == DateTime.Now.Date)
                {
                    JiraTimerCollection.StartTimer(Settings.AppSettings.TimerRunningOnShutdown.Value);
                }

                Settings.AppSettings.TimerRunningOnShutdown = null;
                SaveSettings(false);
            }
        }

        private void OnExportPromptEvent(object sender, ExportPromptDetail promptDetail)
        {
            if (promptDetail.ExportTime.TotalSeconds >= 60)
            {
                if (ExportPromptEvent != null) ExportPromptEvent(sender, promptDetail);
            }
        }

        private void OnNoActivityEvent(object sender, int millisecondsSinceActivity)
        {
            if (NoActivityEvent != null) NoActivityEvent(sender, millisecondsSinceActivity);
        }

        private void CleanUpAndTrackingHearbeatOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                jiraTimerCollection.RemoveTimersOlderThanDays(settingsCollection.AppSettings.KeepTimersForDays);
                idleTimerCollection.RemoveOldTimers();
                jiraConnection.UpdateCache();

                var runningTimerId = jiraTimerCollection.GetRunningTimerId();
                if (runningTimerId.HasValue)
                {
                    var runningTimer = jiraTimerCollection.GetTimer(runningTimerId.Value);
                    if (runningTimer.DateStarted.Date != DateTime.Now.Date)
                    {
                        jiraTimerCollection.StopTimer(runningTimerId.Value, true);
                        jiraTimerCollection.StartTimer(runningTimerId.Value);
                    }
                }

                if (settingsCollection.InternalSettings.LastHeartbeatTracked.Date < DateTime.UtcNow.Date)
                {
                    if (DailyTrackingEvent != null) DailyTrackingEvent.Invoke(this, null);
                    trackUsage.TrackAppUsage(TrackingType.DailyHearbeat);
                    settingsCollection.InternalSettings.LastHeartbeatTracked = DateTime.UtcNow;
                    settingsCollection.SaveSettings();
                }
            }
            catch { /*Surpress Errors, if this fails timers won't be removed*/}
        }

        private void JiraExportHearbeatHearbeatOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var unexportedTimers = jiraTimerCollection.GetAllUnexportedTimers().ToList();

                var jiraSearches = new Dictionary<string, List<Guid>>();

                foreach (var unexportedTimer in unexportedTimers)
                {
                    if (!unexportedTimer.LastJiraTimeCheck.HasValue || unexportedTimer.LastJiraTimeCheck.Value < DateTime.UtcNow.AddMinutes(-10))
                    {
                        if (jiraSearches.ContainsKey(unexportedTimer.JiraReference))
                        {
                            jiraSearches[unexportedTimer.JiraReference].Add(unexportedTimer.UniqueId);
                        }
                        else
                        {
                            jiraSearches.Add(unexportedTimer.JiraReference, new List<Guid> { unexportedTimer.UniqueId });
                        }
                    }
                }

                foreach (var jiraSearch in jiraSearches)
                {
                    var issueWithWorklogs = jiraConnection.GetJiraIssue(jiraSearch.Key, true);
                    foreach (var uniqueTimer in jiraSearch.Value)
                    {
                        jiraTimerCollection.RefreshFromJira(uniqueTimer, issueWithWorklogs, jiraConnection.CurrentUser);
                    }
                }
            }
            catch { /*Surpress the error*/ }
        }

        public void Initialise()
        {
            var processes = Process.GetProcesses();
            if (processes.Count(process => process.ProcessName.Contains("Gallifrey") && !process.ProcessName.Contains("vshost")) > 1)
            {
                throw new MultipleGallifreyRunningException();
            }

            jiraConnection.ReConnect(settingsCollection.JiraConnectionSettings, settingsCollection.ExportSettings);

            CleanUpAndTrackingHearbeatOnElapsed(this, null);
        }

        public void Close()
        {
            trackUsage.TrackAppUsage(TrackingType.AppClose);
            var runningTimer = jiraTimerCollection.GetRunningTimerId();
            if (runningTimer.HasValue)
            {
                jiraTimerCollection.StopTimer(runningTimer.Value, false);
            }
            settingsCollection.AppSettings.TimerRunningOnShutdown = runningTimer;

            try
            {
                idleTimerCollection.StopLockedTimers();
            }
            catch (NoIdleTimerRunningException) { /*This being caught is good, there was nothing to stop*/}

            jiraTimerCollection.SaveTimers();
            idleTimerCollection.SaveTimers();
            settingsCollection.SaveSettings();
        }

        public void TrackEvent(TrackingType trackingType)
        {
            trackUsage.TrackAppUsage(trackingType);
        }

        public void SaveSettings(bool jiraSettingsChanged)
        {
            settingsCollection.SaveSettings();

            if (jiraSettingsChanged)
            {
                jiraConnection.ReConnect(settingsCollection.JiraConnectionSettings, settingsCollection.ExportSettings);
            }

            ActivityChecker.UpdateAppSettings(settingsCollection.AppSettings);
            jiraTimerCollection.UpdateAppSettings(settingsCollection.ExportSettings);
            trackUsage.UpdateSettings(settingsCollection.AppSettings, settingsCollection.InternalSettings);
        }

        public bool StartIdleTimer()
        {
            ActivityChecker.StopActivityCheck();

            runningTimerWhenIdle = JiraTimerCollection.GetRunningTimerId();
            if (runningTimerWhenIdle.HasValue)
            {
                jiraTimerCollection.StopTimer(runningTimerWhenIdle.Value, true);
            }
            return idleTimerCollection.NewLockTimer();
        }

        public Guid StopIdleTimer()
        {
            ActivityChecker.StartActivityCheck();

            if (runningTimerWhenIdle.HasValue)
            {
                var timer = jiraTimerCollection.GetTimer(runningTimerWhenIdle.Value);
                if (timer.DateStarted.Date == DateTime.Now.Date)
                {
                    jiraTimerCollection.StartTimer(runningTimerWhenIdle.Value);
                }
                runningTimerWhenIdle = null;
            }
            return idleTimerCollection.StopLockedTimers();
        }

        public IEnumerable<ChangeLogVersion> GetChangeLog(XDocument changeLogContent)
        {
            var changeLogItems = ChangeLogProvider.GetChangeLog(settingsCollection.InternalSettings.LastChangeLogVersion, changeLogContent);

            if (versionControl.IsAutomatedDeploy)
            {
                settingsCollection.InternalSettings.SetLastChangeLogVersion(versionControl.DeployedVersion);
                settingsCollection.SaveSettings();
                trackUsage.UpdateSettings(settingsCollection.AppSettings, settingsCollection.InternalSettings);
            }

            return changeLogItems;
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

        public IVersionControl VersionControl
        {
            get { return versionControl; }
        }

        public List<WithThanksDefinition> WithThanksDefinitions
        {
            get { return withThanksCreator.WithThanksDefinitions; }
        }
    }
}
