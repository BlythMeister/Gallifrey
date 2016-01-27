using System;
using System.Collections.Generic;
using System.Linq;
using Gallifrey.AppTracking;
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
        IEnumerable<JiraTimer> GetStoppedUnexportedTimers();
        IEnumerable<JiraTimer> GetAllUnexportedTimers();
        IEnumerable<RecentJira> GetJiraReferencesForLastDays(int days);
        Guid AddTimer(Issue jiraIssue, DateTime startDate, TimeSpan seedTime, bool startNow);
        Guid AddTempTimer(string tempTimerDescription, DateTime startDate, TimeSpan seedTime, bool startNow);
        void RemoveTimer(Guid uniqueId);
        void StartTimer(Guid uniqueId);
        void StopTimer(Guid uniqueId, bool automatedStop);
        Guid? GetRunningTimerId();
        void RemoveTimersOlderThanDays(int keepTimersForDays);
        JiraTimer GetTimer(Guid timerGuid);
        Guid RenameTimer(Guid timerGuid, Issue newIssue);
        void ChangeTempTimerDescription(Guid editedTimerId, string tempTimerDescription);
        Guid ChangeTimerDate(Guid timerGuid, DateTime newStartDate);
        Tuple<int, int> GetNumberExported();
        TimeSpan GetTotalUnexportedTime();
        TimeSpan GetTotalTempTime();
        TimeSpan GetTotalExportableTime();
        TimeSpan GetTotalExportedTimeThisWeek(DayOfWeek startOfWeek);
        TimeSpan GetTotalTimeForDate(DateTime timerDate);
        bool AdjustTime(Guid uniqueId, int hours, int minutes, bool addTime);
        void AddJiraExportedTime(Guid uniqueId, int hours, int minutes);
        void AddIdleTimer(Guid uniqueId, List<IdleTimer> idleTimer);
        void RefreshFromJira(Guid uniqueId, Issue jiraIssue, User currentUser);
    }

    public class JiraTimerCollection : IJiraTimerCollection
    {
        private IExportSettings exportSettings;
        private readonly ITrackUsage trackUsage;
        private readonly List<JiraTimer> timerList;
        internal event EventHandler<ExportPromptDetail> exportPrompt;

        internal JiraTimerCollection(IExportSettings exportSettings, ITrackUsage trackUsage)
        {
            this.exportSettings = exportSettings;
            this.trackUsage = trackUsage;
            timerList = JiraTimerCollectionSerializer.DeSerialize();
        }

        internal void UpdateAppSettings(IExportSettings newExportSettings)
        {
            exportSettings = newExportSettings;
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

        public IEnumerable<JiraTimer> GetStoppedUnexportedTimers()
        {
            return GetAllUnexportedTimers().Where(timer => !timer.IsRunning).OrderBy(timer => timer.DateStarted);
        }

        public IEnumerable<JiraTimer> GetAllUnexportedTimers()
        {
            return timerList.Where(timer => !timer.FullyExported).OrderBy(timer => timer.DateStarted);
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
            var timerSearch = timerList.FirstOrDefault(timer => timer.JiraReference == newTimer.JiraReference && timer.DateStarted.Date == newTimer.DateStarted.Date);

            if (timerSearch != null)
            {
                throw new DuplicateTimerException("Already have a timer for this task on this day!", timerSearch.UniqueId);
            }

            trackUsage.TrackAppUsage(TrackingType.TimerAdded);
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
                if (exportSettings.ExportPrompt != null && exportSettings.ExportPrompt.OnCreatePreloaded && !newTimer.FullyExported)
                {
                    exportPrompt.Invoke(this, new ExportPromptDetail(newTimer.UniqueId, seedTime));
                }
            }
            return newTimer.UniqueId;
        }

        public Guid AddTempTimer(string tempTimerDescription, DateTime startDate, TimeSpan seedTime, bool startNow)
        {
            var foundValid = false;
            var nextTempTimerNumber = 0;
            var currentLocalTimers = GetTimersForADate(startDate.Date).Where(x => x.TempTimer).ToList();
            while (!foundValid)
            {
                nextTempTimerNumber++;
                if (!currentLocalTimers.Any(x => x.JiraReference.EndsWith($"-{nextTempTimerNumber}")))
                {
                    foundValid = true;
                }
            }

            var newTimer = new JiraTimer(nextTempTimerNumber, tempTimerDescription, startDate, seedTime);

            AddTimer(newTimer);
            if (startNow)
            {
                StartTimer(newTimer.UniqueId);
            }
            return newTimer.UniqueId;
        }

        public void RemoveTimer(Guid uniqueId)
        {
            trackUsage.TrackAppUsage(TrackingType.TimerDeleted);
            RemoveTimerInternal(uniqueId);
        }

        private void RemoveTimerInternal(Guid uniqueId)
        {
            timerList.Remove(GetTimer(uniqueId));
            SaveTimers();
        }

        public void StartTimer(Guid uniqueId)
        {
            var timerForInteration = GetTimer(uniqueId);
            if (timerForInteration.DateStarted.Date != DateTime.Now.Date)
            {
                if (timerForInteration.TempTimer)
                {
                    uniqueId = AddTempTimer(timerForInteration.JiraName, DateTime.Now, new TimeSpan(), false);
                    timerForInteration = GetTimer(uniqueId);
                }
                else
                {
                    timerForInteration = new JiraTimer(timerForInteration, DateTime.Now, true);
                    AddTimer(timerForInteration);
                    uniqueId = timerForInteration.UniqueId;
                }
            }

            var runningTimerId = GetRunningTimerId();
            if (runningTimerId.HasValue && runningTimerId.Value != uniqueId)
            {
                GetTimer(runningTimerId.Value).StopTimer();
            }

            timerForInteration.StartTimer();

            SaveTimers();
        }

        public void StopTimer(Guid uniqueId, bool automatedStop)
        {
            var timerForInteration = GetTimer(uniqueId);
            var stopTime = timerForInteration.StopTimer();

            SaveTimers();
            if (exportSettings.ExportPrompt != null && exportSettings.ExportPrompt.OnStop && !timerForInteration.FullyExported && !automatedStop)
            {
                exportPrompt.Invoke(this, new ExportPromptDetail(uniqueId, stopTime));
            }
        }

        public Guid? GetRunningTimerId()
        {
            var runningTimer = timerList.FirstOrDefault(timer => timer.IsRunning);

            return runningTimer?.UniqueId;
        }

        public void RemoveTimersOlderThanDays(int keepTimersForDays)
        {
            if (keepTimersForDays > 0) keepTimersForDays = keepTimersForDays * -1;

            timerList.RemoveAll(timer => timer.FullyExported &&
                timer.DateStarted.Date != DateTime.Now.Date &&
                timer.DateStarted.Date <= DateTime.Now.AddDays(keepTimersForDays).Date);

            SaveTimers();
        }

        public JiraTimer GetTimer(Guid timerGuid)
        {
            return timerList.FirstOrDefault(timer => timer.UniqueId == timerGuid);
        }

        public Guid RenameTimer(Guid timerGuid, Issue newIssue)
        {
            var currentTimer = GetTimer(timerGuid);
            if (currentTimer.IsRunning) currentTimer.StopTimer();
            var newTimer = new JiraTimer(newIssue, currentTimer.DateStarted, currentTimer.ExactCurrentTime);

            var timerSearch = timerList.FirstOrDefault(timer => timer.JiraReference == newTimer.JiraReference && timer.DateStarted.Date == newTimer.DateStarted.Date);

            if (timerSearch != null)
            {
                throw new DuplicateTimerException("Already have a timer for this task on this day!", timerSearch.UniqueId);
            }

            RemoveTimerInternal(timerGuid);
            AddTimer(newTimer);
            SaveTimers();
            return newTimer.UniqueId;
        }

        public void ChangeTempTimerDescription(Guid timerGuid, string tempTimerDescription)
        {
            var currentTimer = GetTimer(timerGuid);
            currentTimer.UpdateTempTimerDescription(tempTimerDescription);
            SaveTimers();
        }

        public Guid ChangeTimerDate(Guid timerGuid, DateTime newStartDate)
        {
            var currentTimer = GetTimer(timerGuid);
            if (currentTimer.IsRunning) currentTimer.StopTimer();

            JiraTimer newTimer;
            if (currentTimer.TempTimer)
            {
                var foundValid = false;
                var nextTempTimerNumber = 0;
                var currentLocalTimers = GetTimersForADate(newStartDate.Date).Where(x => x.TempTimer).ToList();
                while (!foundValid)
                {
                    nextTempTimerNumber++;
                    if (!currentLocalTimers.Any(x => x.JiraReference.EndsWith($"-{nextTempTimerNumber}")))
                    {
                        foundValid = true;
                    }
                }
                newTimer = new JiraTimer(nextTempTimerNumber, currentTimer.JiraName, newStartDate.Date, currentTimer.ExactCurrentTime);
            }
            else
            {
                newTimer = new JiraTimer(currentTimer, newStartDate.Date, false);

                var timerSearch = timerList.FirstOrDefault(timer => timer.JiraReference == newTimer.JiraReference && timer.DateStarted.Date == newTimer.DateStarted.Date);

                if (timerSearch != null)
                {
                    throw new DuplicateTimerException("Already have a timer for this task on this day!", timerSearch.UniqueId);
                }
            }

            RemoveTimerInternal(timerGuid);
            AddTimer(newTimer);
            SaveTimers();
            return newTimer.UniqueId;
        }

        public Tuple<int, int> GetNumberExported()
        {
            return new Tuple<int, int>(timerList.Count(jiraTimer => jiraTimer.FullyExported), timerList.Count);
        }

        public TimeSpan GetTotalUnexportedTime()
        {
            var unexportedTime = new TimeSpan();
            return timerList.Where(timer => !timer.FullyExported).Aggregate(unexportedTime, (current, jiraTimer) => current.Add(new TimeSpan(jiraTimer.TimeToExport.Hours, jiraTimer.TimeToExport.Minutes, 0)));
        }

        public TimeSpan GetTotalTempTime()
        {
            var unexportedTime = new TimeSpan();
            return timerList.Where(timer => timer.TempTimer && !timer.IsRunning && !timer.FullyExported).Aggregate(unexportedTime, (current, jiraTimer) => current.Add(new TimeSpan(jiraTimer.TimeToExport.Hours, jiraTimer.TimeToExport.Minutes, 0)));
        }

        public TimeSpan GetTotalExportableTime()
        {
            var unexportedTime = new TimeSpan();
            return timerList.Where(timer => !timer.TempTimer && !timer.IsRunning && !timer.FullyExported).Aggregate(unexportedTime, (current, jiraTimer) => current.Add(new TimeSpan(jiraTimer.TimeToExport.Hours, jiraTimer.TimeToExport.Minutes, 0)));
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
            if (exportSettings.ExportPrompt != null && exportSettings.ExportPrompt.OnManualAdjust && !timer.FullyExported)
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

        public void AddIdleTimer(Guid uniqueId, List<IdleTimer> idleTimers)
        {
            trackUsage.TrackAppUsage(TrackingType.LockedTimerAdd);
            var timer = GetTimer(uniqueId);
            foreach (var idleTimer in idleTimers)
            {
                timer.AddIdleTimer(idleTimer);
            }
            SaveTimers();
            if (exportSettings.ExportPrompt != null && exportSettings.ExportPrompt.OnAddIdle && !timer.FullyExported)
            {
                var idleTime = new TimeSpan();
                idleTime = idleTimers.Aggregate(idleTime, (current, idleTimer) => current.Add(idleTimer.IdleTimeValue));
                exportPrompt.Invoke(this, new ExportPromptDetail(uniqueId, idleTime));
            }
        }

        public void RefreshFromJira(Guid uniqueId, Issue jiraIssue, User currentUser)
        {
            var timer = GetTimer(uniqueId);
            if (timer != null)
            {
                timer.RefreshFromJira(jiraIssue, currentUser);
                SaveTimers();
            }
        }
    }
}