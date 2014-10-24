using System;
using System.Collections.Generic;
using System.Linq;
using Gallifrey.Comparers;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.IdleTimers;
using Gallifrey.Jira.Model;
using Gallifrey.JiraIntegration;
using Gallifrey.Serialization;
using Gallifrey.Settings;

namespace Gallifrey.JiraTimers
{
    public interface IJiraTimerCollection
    {
        IEnumerable<DateTime> GetValidTimerDates();
        IEnumerable<JiraTimer> GetTimersForADate(DateTime timerDate);
        IEnumerable<JiraTimer> GetUnexportedTimers(DateTime timerDate);
        JiraTimer GetOldestUnexportedTimer();
        IEnumerable<RecentJira> GetJiraReferencesForLastDays(int days);
        Guid AddTimer(Issue jiraIssue, DateTime startDate, TimeSpan seedTime, bool startNow);
        void RemoveTimer(Guid uniqueId);
        void StartTimer(Guid uniqueId);
        void StopTimer(Guid uniqueId);
        Guid? GetRunningTimerId();
        void RemoveTimersOlderThanDays(int keepTimersForDays);
        JiraTimer GetTimer(Guid timerGuid);
        void RenameTimer(Guid timerGuid, Issue newIssue);
        void ChangeTimerDate(Guid timerGuid, DateTime newStartDate);
        Tuple<int, int> GetNumberExported();
        TimeSpan GetTotalUnexportedTime();
        TimeSpan GetTotalExportedTimeThisWeek(DayOfWeek startOfWeek);
        TimeSpan GetTotalTimeForDate(DateTime timerDate);
        bool AdjustTime(Guid uniqueId, int hours, int minutes, bool addTime);
        void AddJiraExportedTime(Guid uniqueId, int hours, int minutes);
        void AddIdleTimer(Guid uniqueId, IdleTimer idleTimer);
        void RefreshFromJira(Guid uniqueId, Issue jiraIssue, string currentUsername);
    }

    public class JiraTimerCollection : IJiraTimerCollection
    {
        private IAppSettings appSettings;
        private readonly List<JiraTimer> timerList;
        internal event EventHandler<ExportPromptDetail> exportPrompt;

        internal JiraTimerCollection(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
            timerList = JiraTimerCollectionSerializer.DeSerialize();
        }

        internal void UpdateAppSettings(IAppSettings newAppSettings)
        {
            appSettings = newAppSettings;
        }

        internal void SaveTimers()
        {
            JiraTimerCollectionSerializer.Serialize(timerList);
        }

        public IEnumerable<DateTime> GetValidTimerDates()
        {
            return timerList.Select(timer => timer.DateStarted.Date).Distinct();
        }

        public IEnumerable<JiraTimer> GetTimersForADate(DateTime timerDate)
        {
            return timerList.Where(timer => timer.DateStarted.Date == timerDate.Date).OrderBy(timer => timer.JiraReference, new JiraReferenceComparer());
        }

        public IEnumerable<JiraTimer> GetUnexportedTimers(DateTime timerDate)
        {
            return timerList.Where(timer => timer.DateStarted.Date == timerDate.Date && !timer.FullyExported).OrderBy(timer => timer.JiraReference, new JiraReferenceComparer());
        }

        public JiraTimer GetOldestUnexportedTimer()
        {
            var timers =  timerList.Where(timer => !timer.FullyExported).OrderBy(timer => timer.DateStarted);

            return timers.FirstOrDefault();
        }

        public IEnumerable<RecentJira> GetJiraReferencesForLastDays(int days)
        {
            if (days > 0) days = days * -1;

            return timerList
                .Where(timer => timer.DateStarted.Date >= DateTime.Now.AddDays(days).Date)
                .Select(timer => new RecentJira(timer.JiraReference, timer.JiraProjectName, timer.JiraName, timer.JiraParentReference, timer.JiraParentName))
                .Distinct(new DuplicateRecentLogComparer())
                .OrderBy(x => x.JiraReference, new JiraReferenceComparer());
        }

        private void AddTimer(JiraTimer newTimer)
        {
            if (timerList.Any(timer => timer.JiraReference == newTimer.JiraReference && timer.DateStarted.Date == newTimer.DateStarted.Date))
            {
                throw new DuplicateTimerException("Already have a timer for this task on this day!");
            }

            timerList.Add(newTimer);
            SaveTimers();
        }

        public Guid AddTimer(Issue jiraIssue, DateTime startDate, TimeSpan seedTime, bool startNow)
        {
            var newTimer = new JiraTimer(jiraIssue, startDate, seedTime);
            AddTimer(newTimer);
            if (startNow)
            {
                StartTimer(newTimer.UniqueId);
            }
            else
            {
                if (appSettings.ExportPrompt != null && appSettings.ExportPrompt.OnCreatePreloaded && !newTimer.FullyExported)
                {
                    exportPrompt.Invoke(this, new ExportPromptDetail(newTimer.UniqueId, seedTime));
                }
            }
            return newTimer.UniqueId;
        }

        public void RemoveTimer(Guid uniqueId)
        {
            timerList.Remove(GetTimer(uniqueId));
            SaveTimers();
        }

        public void StartTimer(Guid uniqueId)
        {
            var timerForInteration = GetTimer(uniqueId);
            if (timerForInteration.DateStarted.Date != DateTime.Now.Date)
            {
                timerForInteration = new JiraTimer(timerForInteration, DateTime.Now, true);
                AddTimer(timerForInteration);
                uniqueId = timerForInteration.UniqueId;
            }

            var runningTimerId = GetRunningTimerId();
            if (runningTimerId.HasValue && runningTimerId.Value != uniqueId)
            {
                GetTimer(runningTimerId.Value).StopTimer();
            }

            timerForInteration.StartTimer();

            SaveTimers();
        }

        public void StopTimer(Guid uniqueId)
        {
            var timerForInteration = GetTimer(uniqueId);
            var stopTime = timerForInteration.StopTimer();

            SaveTimers();
            if (appSettings.ExportPrompt != null && appSettings.ExportPrompt.OnStop && !timerForInteration.FullyExported)
            {
                exportPrompt.Invoke(this, new ExportPromptDetail(uniqueId, stopTime));
            }
        }

        public Guid? GetRunningTimerId()
        {
            var runningTimer = timerList.FirstOrDefault(timer => timer.IsRunning);
            if (runningTimer == null)
            {
                return null;
            }

            return runningTimer.UniqueId;
        }

        public void RemoveTimersOlderThanDays(int keepTimersForDays)
        {
            if (keepTimersForDays > 0) keepTimersForDays = keepTimersForDays * -1;

            timerList.RemoveAll(timer => timer.FullyExported &&
                timer.DateStarted.Date != DateTime.Now.Date &&
                timer.DateStarted <= DateTime.Now.AddDays(keepTimersForDays));

            SaveTimers();
        }

        public JiraTimer GetTimer(Guid timerGuid)
        {
            return timerList.FirstOrDefault(timer => timer.UniqueId == timerGuid);
        }

        public void RenameTimer(Guid timerGuid, Issue newIssue)
        {
            var currentTimer = GetTimer(timerGuid);
            if (currentTimer.IsRunning) currentTimer.StopTimer();
            var newTimer = new JiraTimer(newIssue, currentTimer.DateStarted, currentTimer.ExactCurrentTime);

            if (timerList.Any(timer => timer.JiraReference == newTimer.JiraReference && timer.DateStarted.Date == newTimer.DateStarted.Date && timer.UniqueId != timerGuid))
            {
                throw new DuplicateTimerException("Already have a timer for this task on this day!");
            }

            RemoveTimer(timerGuid);
            AddTimer(newTimer);
            SaveTimers();
        }

        public void ChangeTimerDate(Guid timerGuid, DateTime newStartDate)
        {
            var currentTimer = GetTimer(timerGuid);
            if (currentTimer.IsRunning) currentTimer.StopTimer();
            var newTimer = new JiraTimer(currentTimer, newStartDate.Date, false);

            if (timerList.Any(timer => timer.JiraReference == newTimer.JiraReference && timer.DateStarted.Date == newTimer.DateStarted.Date && timer.UniqueId != timerGuid))
            {
                throw new DuplicateTimerException("Already have a timer for this task on this day!");
            }

            RemoveTimer(timerGuid);
            AddTimer(newTimer);
            SaveTimers();
        }

        public Tuple<int, int> GetNumberExported()
        {
            return new Tuple<int, int>(timerList.Count(jiraTimer => jiraTimer.FullyExported), timerList.Count);
        }

        public TimeSpan GetTotalUnexportedTime()
        {
            var unexportedTime = new TimeSpan();
            return timerList.Aggregate(unexportedTime, (current, jiraTimer) => current.Add(new TimeSpan(jiraTimer.TimeToExport.Hours, jiraTimer.TimeToExport.Minutes, 0)));
        }

        public TimeSpan GetTotalExportedTimeThisWeek(DayOfWeek startOfWeek)
        {
            var exportedTime = new TimeSpan();
            return timerList.Where(jiraTimer => jiraTimer.IsThisWeek(startOfWeek)).Aggregate(exportedTime, (current, jiraTimer) => current.Add(jiraTimer.ExportedTime));
        }

        public TimeSpan GetTotalTimeForDate(DateTime timerDate)
        {
            var time = new TimeSpan();
            return timerList.Where(jiraTimer => jiraTimer.DateStarted.Date == timerDate.Date).Aggregate(time, (current, jiraTimer) => current.Add(new TimeSpan(jiraTimer.ExactCurrentTime.Hours, jiraTimer.ExactCurrentTime.Minutes, jiraTimer.ExactCurrentTime.Seconds)));
        }

        public bool AdjustTime(Guid uniqueId, int hours, int minutes, bool addTime)
        {
            var timer = GetTimer(uniqueId);
            var adjustment = new TimeSpan(hours, minutes, 0);

            if (!timer.ManualAdjustment(adjustment, addTime))
            {
                return false;
            }

            SaveTimers();
            if (appSettings.ExportPrompt != null && appSettings.ExportPrompt.OnManualAdjust && !timer.FullyExported)
            {
                if (!addTime) adjustment = adjustment.Negate();
                exportPrompt.Invoke(this, new ExportPromptDetail(uniqueId, adjustment));
            }

            return true;
        }

        public void AddJiraExportedTime(Guid uniqueId, int hours, int minutes)
        {
            var timer = GetTimer(uniqueId);
            timer.AddJiraExportedTime(new TimeSpan(hours, minutes, 0));
            SaveTimers();
        }

        public void AddIdleTimer(Guid uniqueId, IdleTimer idleTimer)
        {
            var timer = GetTimer(uniqueId);
            timer.AddIdleTimer(idleTimer);
            SaveTimers();
            if (appSettings.ExportPrompt != null && appSettings.ExportPrompt.OnAddIdle && !timer.FullyExported)
            {
                exportPrompt.Invoke(this, new ExportPromptDetail(uniqueId, idleTimer.IdleTimeValue));
            }
        }

        public void RefreshFromJira(Guid uniqueId, Issue jiraIssue, string currentUsername)
        {
            var timer = GetTimer(uniqueId);
            timer.RefreshFromJira(jiraIssue, currentUsername);
            SaveTimers();
        }
    }
}