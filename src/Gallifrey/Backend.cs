using Gallifrey.AppTracking;
using Gallifrey.ChangeLog;
using Gallifrey.Contributors;
using Gallifrey.Exceptions;
using Gallifrey.IdleTimers;
using Gallifrey.InactiveMonitor;
using Gallifrey.Jira.Model;
using Gallifrey.JiraIntegration;
using Gallifrey.JiraTimers;
using Gallifrey.Serialization;
using Gallifrey.Settings;
using Gallifrey.Versions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using Timer = System.Timers.Timer;

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

        event EventHandler DailyTrackingEvent;

        event EventHandler BackendModifiedTimers;

        event EventHandler IsPremiumChanged;

        event EventHandler SettingsChanged;

        event EventHandler<int> NoActivityEvent;

        event EventHandler<ExportPromptDetail> ExportPromptEvent;

        void Initialise();

        void Close();

        void TrackEvent(TrackingType trackingType);

        void SaveSettings(bool jiraSettingsChanged, bool trackingOptOut);

        void StartLockTimer(TimeSpan? idleTime = null);

        Guid? StopLockTimer();

        IEnumerable<ChangeLogVersion> GetChangeLog(XDocument changeLogContent);

        void ResetInactiveAlert();

        bool IsInitialised { get; }
    }

    public class Backend : IBackend
    {
        public bool IsInitialised { get; private set; }

        private readonly JiraTimerCollection jiraTimerCollection;
        private readonly IdleTimerCollection idleTimerCollection;
        private readonly SettingsCollection settingsCollection;
        private readonly RecentJiraCollection recentJiraCollection;
        private readonly ITrackUsage trackUsage;
        private readonly JiraConnection jiraConnection;
        private readonly VersionControl versionControl;
        private readonly WithThanksCreator withThanksCreator;
        private readonly PremiumChecker premiumChecker;
        private readonly Mutex exportedHeartbeatMutex;
        private readonly Timer cleanUpAndTrackingHeartbeat;
        private readonly Timer jiraExportHeartbeat;

        public event EventHandler<int> NoActivityEvent;

        public event EventHandler<ExportPromptDetail> ExportPromptEvent;

        public event EventHandler DailyTrackingEvent;

        public event EventHandler BackendModifiedTimers;

        public event EventHandler IsPremiumChanged;

        public event EventHandler SettingsChanged;

        internal ActivityChecker ActivityChecker;

        private Guid? runningTimerWhenIdle;
        private DateTime lastMissingTimerCheck = DateTime.MinValue;

        public Backend(InstanceType instanceType)
        {
            settingsCollection = new SettingsCollection(SettingsCollectionSerializer.DeSerialize());
            versionControl = new VersionControl(instanceType);
            trackUsage = new TrackUsage(versionControl, settingsCollection, instanceType);
            jiraTimerCollection = new JiraTimerCollection(settingsCollection, trackUsage);
            recentJiraCollection = new RecentJiraCollection();
            jiraConnection = new JiraConnection(trackUsage, recentJiraCollection);
            idleTimerCollection = new IdleTimerCollection();
            ActivityChecker = new ActivityChecker(jiraTimerCollection, settingsCollection);
            withThanksCreator = new WithThanksCreator();
            premiumChecker = new PremiumChecker();

            versionControl.UpdateCheckOccured += (sender, b) => trackUsage.TrackAppUsage(b ? TrackingType.UpdateCheckManual : TrackingType.UpdateCheck);
            jiraTimerCollection.ExportPrompt += OnExportPromptEvent;
            ActivityChecker.NoActivityEvent += OnNoActivityEvent;

            cleanUpAndTrackingHeartbeat = new Timer(TimeSpan.FromMinutes(15).TotalMilliseconds);
            cleanUpAndTrackingHeartbeat.Elapsed += CleanUpAndTrackingHeartbeatOnElapsed;

            exportedHeartbeatMutex = new Mutex(false);
            jiraExportHeartbeat = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
            jiraExportHeartbeat.Elapsed += JiraExportHeartbeatHeartbeatOnElapsed;
        }

        private void OnExportPromptEvent(object sender, ExportPromptDetail promptDetail)
        {
            if (promptDetail.ExportTime.TotalSeconds >= 60)
            {
                ExportPromptEvent?.Invoke(sender, promptDetail);
            }
        }

        private void OnNoActivityEvent(object sender, int millisecondsSinceActivity)
        {
            NoActivityEvent?.Invoke(sender, millisecondsSinceActivity);
        }

        private void CleanUpAndTrackingHeartbeatOnElapsed(object sender, ElapsedEventArgs e)
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
                BackendModifiedTimers?.Invoke(this, null);

                if (settingsCollection.InternalSettings.LastHeartbeatTracked.Date < DateTime.UtcNow.Date)
                {
                    if (versionControl.IsAutomatedDeploy && jiraConnection.IsConnected)
                    {
                        DailyTrackingEvent?.Invoke(this, null);
                        trackUsage.TrackAppUsage(TrackingType.DailyHeartbeat);
                        settingsCollection.InternalSettings.SetLastHeartbeatTracked(DateTime.UtcNow);
                        settingsCollection.SaveSettings();
                    }
                }

                var isPremium = premiumChecker.CheckIfPremium(settingsCollection);
                if (!versionControl.IsAutomatedDeploy || isPremium != settingsCollection.InternalSettings.IsPremium)
                {
                    settingsCollection.InternalSettings.SetIsPremium(isPremium);
                    settingsCollection.SaveSettings();
                    IsPremiumChanged?.Invoke(this, null);
                }
            }
            catch { /*Suppress Errors, if this fails timers won't be removed*/}
        }

        private void JiraExportHeartbeatHeartbeatOnElapsed(object sender, ElapsedEventArgs e)
        {
            exportedHeartbeatMutex.WaitOne();
            var issueCache = new List<Issue>();

            try
            {
                var keepTimersForDays = settingsCollection.AppSettings.KeepTimersForDays;
                if (keepTimersForDays > 0) keepTimersForDays *= -1;
                var workingDate = DateTime.Now.AddDays(keepTimersForDays + 1);
                var doCheck = false;

                if (lastMissingTimerCheck < DateTime.UtcNow.AddHours(-1))
                {
                    doCheck = true;
                    lastMissingTimerCheck = DateTime.UtcNow;
                }

                var checkDates = new Dictionary<DateTime, List<JiraTimer>>();

                while (workingDate.Date <= DateTime.Now.Date)
                {
                    var timersOnDate = jiraTimerCollection.GetTimersForADate(workingDate.Date).ToList();

                    if (doCheck || timersOnDate.Any(x => !x.LastJiraTimeCheck.HasValue || x.LastJiraTimeCheck.Value < DateTime.UtcNow.AddMinutes(-30)))
                    {
                        checkDates.Add(workingDate, timersOnDate);
                    }

                    workingDate = workingDate.AddDays(1);
                }

                var jirasExportedTo = jiraConnection.GetWorkLoggedForDates(checkDates.Keys).ToList();

                foreach (var checkDate in checkDates)
                {
                    foreach (var timeExport in jirasExportedTo.Where(x => x.LoggedDate.Date == checkDate.Key.Date))
                    {
                        if (checkDate.Value.All(x => x.JiraReference != timeExport.JiraRef))
                        {
                            var issue = issueCache.FirstOrDefault(x => x.key == timeExport.JiraRef);

                            if (issue == null && jiraConnection.DoesJiraExist(timeExport.JiraRef))
                            {
                                issue = jiraConnection.GetJiraIssue(timeExport.JiraRef);
                                issueCache.Add(issue);
                            }

                            var timerReference = jiraTimerCollection.AddTimer(issue, checkDate.Key.Date, new TimeSpan(), false);
                            BackendModifiedTimers?.Invoke(this, null);
                            jiraTimerCollection.RefreshFromJira(timerReference, issue, timeExport.TimeSpent);
                        }
                    }

                    foreach (var timer in checkDate.Value.Where(x => !x.LocalTimer))
                    {
                        var issue = issueCache.FirstOrDefault(x => x.key == timer.JiraReference);
                        if (issue == null && jiraConnection.DoesJiraExist(timer.JiraReference))
                        {
                            issue = jiraConnection.GetJiraIssue(timer.JiraReference);
                            issueCache.Add(issue);
                        }

                        var time = jirasExportedTo.FirstOrDefault(x => x.JiraRef == timer.JiraReference && x.LoggedDate.Date == checkDate.Key.Date)?.TimeSpent;
                        jiraTimerCollection.RefreshFromJira(timer.UniqueId, issue, time ?? TimeSpan.Zero);
                    }
                }
            }
            catch { /*Suppress the error*/ }

            exportedHeartbeatMutex.ReleaseMutex();
        }

        public void Initialise()
        {
            settingsCollection.Initialise();
            idleTimerCollection.Initialise();
            jiraTimerCollection.Initialise();
            recentJiraCollection.Initialise();

            if (settingsCollection.InternalSettings.LastChangeLogVersion == new Version(0, 0, 0, 0) && settingsCollection.InternalSettings.NewUser)
            {
                throw new MissingConfigException("New User");
            }

            jiraConnection.ReConnect(settingsCollection.JiraConnectionSettings, settingsCollection.ExportSettings);
            cleanUpAndTrackingHeartbeat.Start();
            jiraExportHeartbeat.Start();
            BackendModifiedTimers?.Invoke(this, null);

            if (Settings.AppSettings.TimerRunningOnShutdown.HasValue)
            {
                var timer = jiraTimerCollection.GetTimer(Settings.AppSettings.TimerRunningOnShutdown.Value);
                if (timer != null && timer.DateStarted.Date == DateTime.Now.Date)
                {
                    JiraTimerCollection.StartTimer(Settings.AppSettings.TimerRunningOnShutdown.Value);
                }

                Settings.AppSettings.TimerRunningOnShutdown = null;
                SaveSettings(false, false);
            }

            if (Settings.AppSettings.NoTimerRunningOnShutdown.HasValue)
            {
                ActivityChecker.SetValue(Settings.AppSettings.NoTimerRunningOnShutdown.Value);

                Settings.AppSettings.NoTimerRunningOnShutdown = null;
                SaveSettings(false, false);
            }

            CleanUpAndTrackingHeartbeatOnElapsed(this, null);

            IsInitialised = true;

            Task.Run(() => { JiraExportHeartbeatHeartbeatOnElapsed(this, null); });
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
            settingsCollection.AppSettings.NoTimerRunningOnShutdown = ActivityChecker.Elapsed;

            idleTimerCollection.StopIdleTimers();

            jiraTimerCollection.SaveTimers();
            idleTimerCollection.SaveTimers();
            settingsCollection.SaveSettings();
        }

        public void TrackEvent(TrackingType trackingType)
        {
            trackUsage.TrackAppUsage(trackingType);
        }

        public void SaveSettings(bool jiraSettingsChanged, bool trackingOptOut)
        {
            settingsCollection.SaveSettings();

            if (jiraSettingsChanged)
            {
                jiraConnection.ReConnect(settingsCollection.JiraConnectionSettings, settingsCollection.ExportSettings);
            }

            if (trackingOptOut)
            {
                trackUsage.TrackAppUsage(TrackingType.OptOut);
            }

            if (IsInitialised) SettingsChanged?.Invoke(this, null);
        }

        public void StartLockTimer(TimeSpan? initialTimeSpan = null)
        {
            ActivityChecker.PauseForLockTimer(initialTimeSpan);

            if (!runningTimerWhenIdle.HasValue)
            {
                runningTimerWhenIdle = JiraTimerCollection.GetRunningTimerId();
                if (runningTimerWhenIdle.HasValue)
                {
                    jiraTimerCollection.StopTimer(runningTimerWhenIdle.Value, true);
                    if (initialTimeSpan.HasValue)
                    {
                        jiraTimerCollection.AdjustTime(runningTimerWhenIdle.Value, initialTimeSpan.Value.Hours, initialTimeSpan.Value.Minutes, false);
                    }
                }
            }

            idleTimerCollection.NewIdleTimer(initialTimeSpan.GetValueOrDefault(new TimeSpan()));
        }

        public Guid? StopLockTimer()
        {
            ActivityChecker.ResumeAfterLockTimer();

            if (runningTimerWhenIdle.HasValue)
            {
                var timer = jiraTimerCollection.GetTimer(runningTimerWhenIdle.Value);
                if (timer != null && timer.DateStarted.Date == DateTime.Now.Date)
                {
                    jiraTimerCollection.StartTimer(runningTimerWhenIdle.Value);
                }
                runningTimerWhenIdle = null;
            }
            return idleTimerCollection.StopIdleTimers();
        }

        public IEnumerable<ChangeLogVersion> GetChangeLog(XDocument changeLogContent)
        {
            var changeLogItems = ChangeLogProvider.GetChangeLog(settingsCollection.InternalSettings.LastChangeLogVersion, changeLogContent);

            if (versionControl.IsAutomatedDeploy)
            {
                settingsCollection.InternalSettings.SetLastChangeLogVersion(versionControl.DeployedVersion);
                settingsCollection.SaveSettings();
            }

            return changeLogItems;
        }

        public void ResetInactiveAlert()
        {
            ActivityChecker.Reset();
        }

        public IJiraTimerCollection JiraTimerCollection => jiraTimerCollection;
        public IIdleTimerCollection IdleTimerCollection => idleTimerCollection;
        public ISettingsCollection Settings => settingsCollection;
        public IJiraConnection JiraConnection => jiraConnection;
        public IVersionControl VersionControl => versionControl;
        public List<WithThanksDefinition> WithThanksDefinitions => withThanksCreator.WithThanksDefinitions;
    }
}
