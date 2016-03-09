using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using Gallifrey.AppTracking;
using Gallifrey.ChangeLog;
using Gallifrey.Contributors;
using Gallifrey.Exceptions;
using Gallifrey.Exceptions.IdleTimers;
using Gallifrey.IdleTimers;
using Gallifrey.InactiveMonitor;
using Gallifrey.Jira.Model;
using Gallifrey.JiraIntegration;
using Gallifrey.JiraTimers;
using Gallifrey.Serialization;
using Gallifrey.Settings;
using Gallifrey.Versions;
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
        private DateTime lastMissingTimerCheck = DateTime.MinValue;
        private readonly Mutex exportedHeartbeatMutex;

        public Backend(InstanceType instanceType, AppType appType)
        {
            settingsCollection = SettingsCollectionSerializer.DeSerialize();
            trackUsage = new TrackUsage(settingsCollection.AppSettings, settingsCollection.InternalSettings, instanceType, appType);
            versionControl = new VersionControl(instanceType, appType, trackUsage);
            jiraTimerCollection = new JiraTimerCollection(settingsCollection.ExportSettings, trackUsage);
            jiraTimerCollection.exportPrompt += OnExportPromptEvent;
            jiraConnection = new JiraConnection(trackUsage);
            idleTimerCollection = new IdleTimerCollection();
            ActivityChecker = new ActivityChecker(jiraTimerCollection, settingsCollection.AppSettings);
            withThanksCreator = new WithThanksCreator();

            ActivityChecker.NoActivityEvent += OnNoActivityEvent;
            var cleanUpAndTrackingHearbeat = new Timer(1800000); // 30 minutes
            cleanUpAndTrackingHearbeat.Elapsed += CleanUpAndTrackingHearbeatOnElapsed;
            cleanUpAndTrackingHearbeat.Start();

            exportedHeartbeatMutex = new Mutex(false);
            var jiraExportHearbeat = new Timer(600000); //10 minutes
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
                ExportPromptEvent?.Invoke(sender, promptDetail);
            }
        }

        private void OnNoActivityEvent(object sender, int millisecondsSinceActivity)
        {
            NoActivityEvent?.Invoke(sender, millisecondsSinceActivity);
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
                    DailyTrackingEvent?.Invoke(this, null);
                    trackUsage.TrackAppUsage(TrackingType.DailyHearbeat);
                    settingsCollection.InternalSettings.LastHeartbeatTracked = DateTime.UtcNow;
                    settingsCollection.SaveSettings();
                }
            }
            catch { /*Surpress Errors, if this fails timers won't be removed*/}
        }

        private void JiraExportHearbeatHearbeatOnElapsed(object sender, ElapsedEventArgs e)
        {
            exportedHeartbeatMutex.WaitOne();
            var issues = new List<Issue>();
            var timersNotChecked = new Dictionary<string, List<Guid>>();

            try
            {
                var keepTimersForDays = settingsCollection.AppSettings.KeepTimersForDays;
                if (keepTimersForDays > 0) keepTimersForDays = keepTimersForDays * -1;
                var workingDate = DateTime.Now.AddDays(keepTimersForDays + 1);

                var doMissingTimerCheck = false;

                if (lastMissingTimerCheck < DateTime.UtcNow.AddHours(-1))
                {
                    doMissingTimerCheck = true;
                    lastMissingTimerCheck = DateTime.UtcNow;
                }

                while (workingDate.Date <= DateTime.Now.Date)
                {
                    var timersOnDate = jiraTimerCollection.GetTimersForADate(workingDate.Date).ToList();

                    if (doMissingTimerCheck)
                    {
                        var jirasExportedTo = JiraConnection.GetJiraIssuesFromJQL($"worklogAuthor = currentUser() and worklogDate = {workingDate.ToString("yyyy-MM-dd")}");

                        foreach (var issue in jirasExportedTo)
                        {
                            if (!timersOnDate.Any(x => x.JiraReference == issue.key))
                            {
                                var issueWithWorklogs = issues.FirstOrDefault(x => x.key == issue.key);

                                if (issueWithWorklogs == null)
                                {
                                    issueWithWorklogs = jiraConnection.GetJiraIssue(issue.key, true);
                                    issues.Add(issueWithWorklogs);
                                }

                                var timerReference = jiraTimerCollection.AddTimer(issueWithWorklogs, workingDate.Date, new TimeSpan(), false);
                                jiraTimerCollection.RefreshFromJira(timerReference, issueWithWorklogs, jiraConnection.CurrentUser);
                            }
                        }
                    }

                    foreach (var timer in timersOnDate.Where(x => !x.TempTimer))
                    {
                        var issueWithWorklogs = issues.FirstOrDefault(x => x.key == timer.JiraReference);

                        //If we have already downloaded the jira, then just refresh it.
                        //If we have not downloaded already, we should check if we need to refresh.
                        //If we need to refresh, download and refresh. - add to collection for future reference

                        if (issueWithWorklogs != null)
                        {
                            jiraTimerCollection.RefreshFromJira(timer.UniqueId, issueWithWorklogs, jiraConnection.CurrentUser);
                        }
                        else if (!timer.LastJiraTimeCheck.HasValue || timer.LastJiraTimeCheck.Value < DateTime.UtcNow.AddMinutes(-30))
                        {
                            issueWithWorklogs = jiraConnection.GetJiraIssue(timer.JiraReference, true);
                            issues.Add(issueWithWorklogs);

                            jiraTimerCollection.RefreshFromJira(timer.UniqueId, issueWithWorklogs, jiraConnection.CurrentUser);

                            if (timersNotChecked.ContainsKey(timer.JiraReference))
                            {
                                foreach (var uncheckedTimer in timersNotChecked[timer.JiraReference])
                                {
                                    jiraTimerCollection.RefreshFromJira(uncheckedTimer, issueWithWorklogs, jiraConnection.CurrentUser);
                                }

                                timersNotChecked.Remove(timer.JiraReference);
                            }
                        }
                        else
                        {
                            if (timersNotChecked.ContainsKey(timer.JiraReference))
                            {
                                timersNotChecked[timer.JiraReference].Add(timer.UniqueId);
                            }
                            else
                            {
                                timersNotChecked.Add(timer.JiraReference, new List<Guid> { timer.UniqueId });
                            }
                        }
                    }

                    workingDate = workingDate.AddDays(1);
                }
            }
            catch { /*Surpress the error*/ }

            exportedHeartbeatMutex.ReleaseMutex();
        }

        public void Initialise()
        {
            var processes = Process.GetProcesses();
            if (processes.Count(process => process.ProcessName.Contains("Gallifrey") && !process.ProcessName.Contains("vshost")) > 1)
            {
                throw new MultipleGallifreyRunningException();
            }

            jiraConnection.ReConnect(settingsCollection.JiraConnectionSettings, settingsCollection.ExportSettings);

            Task.Factory.StartNew(() =>
            {
                CleanUpAndTrackingHearbeatOnElapsed(this, null);
                JiraExportHearbeatHearbeatOnElapsed(this, null);
            });
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

        public IJiraTimerCollection JiraTimerCollection => jiraTimerCollection;
        public IIdleTimerCollection IdleTimerCollection => idleTimerCollection;
        public ISettingsCollection Settings => settingsCollection;
        public IJiraConnection JiraConnection => jiraConnection;
        public IVersionControl VersionControl => versionControl;
        public List<WithThanksDefinition> WithThanksDefinitions => withThanksCreator.WithThanksDefinitions;
    }
}
