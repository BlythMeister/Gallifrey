using Gallifrey.AppTracking;
using Gallifrey.Comparers;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.IdleTimers;
using Gallifrey.Jira.Model;
using Gallifrey.JiraIntegration;
using Gallifrey.Serialization;
using Gallifrey.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gallifrey.JiraTimers
{
    public interface IJiraTimerCollection
    {
        IEnumerable<DateTime> GetValidTimerDates();

        IEnumerable<JiraTimer> GetTimersForADate(DateTime timerDate);

        IEnumerable<JiraTimer> GetStoppedUnexportedTimers();

        IEnumerable<JiraTimer> GetAllLocalTimers();

        IEnumerable<JiraTimer> GetAllTimersWithTime();

        IEnumerable<RecentJira> GetJiraReferencesForLastDays(int days);

        Guid AddTimer(Issue jiraIssue, DateTime startDate, TimeSpan seedTime, bool startNow);

        Guid AddLocalTimer(string localTimerDescription, DateTime startDate, TimeSpan seedTime, bool startNow);

        void RemoveTimer(Guid uniqueId);

        void StartTimer(Guid uniqueId);

        void StopTimer(Guid uniqueId, bool automatedStop);

        Guid? GetRunningTimerId();

        JiraTimer GetTimer(Guid timerGuid);

        Guid RenameTimer(Guid timerGuid, Issue newIssue);

        Guid ChangeLocalTimerDescription(Guid editedTimerId, string localTimerDescription);

        Guid ChangeTimerDate(Guid timerGuid, DateTime newStartDate);

        Tuple<int, int> GetNumberExported();

        TimeSpan GetStoppedTotalExportableTime();

        TimeSpan GetTotalLocalTime();

        TimeSpan GetTotalExportableTime();

        TimeSpan GetTotalExportedTimeThisWeek(DayOfWeek startOfWeek);

        TimeSpan GetTotalTimeThisWeekNoSeconds(DayOfWeek startOfWeek);

        TimeSpan GetTotalTimeForDate(DateTime timerDate);

        TimeSpan GetTotalTimeForDateNoSeconds(DateTime timerDate);

        bool AdjustTime(Guid uniqueId, int hours, int minutes, bool addTime);

        void AddJiraExportedTime(Guid uniqueId, int hours, int minutes);

        void AddIdleTimer(Guid uniqueId, List<IdleTimer> idleTimer);

        void RefreshFromJira(Guid uniqueId, Issue jiraIssue, TimeSpan loggedTime);

        event EventHandler GeneralTimerModification;
    }

    public class JiraTimerCollection : IJiraTimerCollection
    {
        private readonly ISettingsCollection settingsCollection;
        private readonly ITrackUsage trackUsage;
        private readonly List<JiraTimer> timerList;
        private bool isIntialised;

        internal event EventHandler<ExportPromptDetail> ExportPrompt;

        public event EventHandler GeneralTimerModification;

        internal JiraTimerCollection(ISettingsCollection settingsCollection, ITrackUsage trackUsage)
        {
            this.settingsCollection = settingsCollection;
            this.trackUsage = trackUsage;
            timerList = new List<JiraTimer>();
            isIntialised = false;
        }

        internal void Initialise()
        {
            var timers = JiraTimerCollectionSerializer.DeSerialize();
            timers.AddRange(timerList);
            timerList.Clear();
            timerList.AddRange(timers.Distinct(new DuplicateTimerComparer()));
            isIntialised = true;
        }

        internal void SaveTimers()
        {
            if (!isIntialised)
            {
                return;
            }

            JiraTimerCollectionSerializer.Serialize(timerList);
            GeneralTimerModification?.Invoke(this, null);
        }

        public IEnumerable<DateTime> GetValidTimerDates()
        {
            return timerList.Select(timer => timer.DateStarted.Date).Distinct();
        }

        public IEnumerable<JiraTimer> GetTimersForADate(DateTime timerDate)
        {
            return timerList.Where(timer => timer.DateStarted.Date == timerDate.Date).OrderBy(timer => timer.JiraReference, new JiraReferenceComparer());
        }

        public IEnumerable<JiraTimer> GetStoppedUnexportedTimers()
        {
            return GetAllUnexportedTimers().Where(timer => !timer.IsRunning).OrderBy(timer => timer.DateStarted);
        }

        public IEnumerable<JiraTimer> GetAllUnexportedTimers()
        {
            return timerList.Where(timer => !timer.FullyExported).OrderBy(timer => timer.DateStarted);
        }

        public IEnumerable<JiraTimer> GetAllLocalTimers()
        {
            return timerList.Where(timer => timer.LocalTimer).OrderBy(timer => timer.DateStarted);
        }

        public IEnumerable<JiraTimer> GetAllTimersWithTime()
        {
            return timerList.Where(timer => timer.ExactCurrentTime > TimeSpan.Zero).OrderBy(timer => timer.DateStarted);
        }

        public IEnumerable<RecentJira> GetJiraReferencesForLastDays(int days)
        {
            if (days > 0)
            {
                days = days * -1;
            }

            return timerList
                .Where(timer => timer.DateStarted.Date >= DateTime.Now.AddDays(days).Date)
                .Select(timer => new RecentJira(timer.JiraReference, timer.JiraProjectName, timer.JiraName, timer.JiraParentReference, timer.JiraParentName))
                .Distinct(new DuplicateRecentJiraComparer())
                .OrderBy(x => x.JiraReference, new JiraReferenceComparer());
        }

        private void AddTimer(JiraTimer newTimer)
        {
            var timerSearch = timerList.FirstOrDefault(timer => string.Equals(timer.JiraReference, newTimer.JiraReference, StringComparison.InvariantCultureIgnoreCase) && timer.DateStarted.Date == newTimer.DateStarted.Date);

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
                if (settingsCollection.ExportSettings.ExportPrompt != null && settingsCollection.ExportSettings.ExportPrompt.OnCreatePreloaded && !newTimer.FullyExported && !newTimer.LocalTimer)
                {
                    ExportPrompt?.Invoke(this, new ExportPromptDetail(newTimer.UniqueId, seedTime));
                }
            }
            return newTimer.UniqueId;
        }

        public Guid AddLocalTimer(string localTimerDescription, DateTime startDate, TimeSpan seedTime, bool startNow)
        {
            var foundValid = false;
            var nextLocalTimerNumber = 0;
            var currentLocalTimers = GetTimersForADate(startDate.Date).Where(x => x.LocalTimer).ToList();
            while (!foundValid)
            {
                nextLocalTimerNumber++;
                if (!currentLocalTimers.Any(x => x.JiraReference.EndsWith($"-{nextLocalTimerNumber}")))
                {
                    foundValid = true;
                }
            }

            var newTimer = new JiraTimer(nextLocalTimerNumber, localTimerDescription, startDate, seedTime);

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
            var timerForInteraction = GetTimer(uniqueId);
            if (timerForInteraction.DateStarted.Date != DateTime.Now.Date)
            {
                if (timerForInteraction.LocalTimer)
                {
                    uniqueId = AddLocalTimer(timerForInteraction.JiraName, DateTime.Now, new TimeSpan(), false);
                    timerForInteraction = GetTimer(uniqueId);
                }
                else
                {
                    timerForInteraction = new JiraTimer(timerForInteraction, DateTime.Now, true);
                    AddTimer(timerForInteraction);
                    uniqueId = timerForInteraction.UniqueId;
                }
            }

            var runningTimerId = GetRunningTimerId();
            if (runningTimerId.HasValue && runningTimerId.Value != uniqueId)
            {
                GetTimer(runningTimerId.Value).StopTimer();
            }

            timerForInteraction.StartTimer();

            SaveTimers();
        }

        public void StopTimer(Guid uniqueId, bool automatedStop)
        {
            var timerForInteraction = GetTimer(uniqueId);
            var stopTime = timerForInteraction.StopTimer();

            SaveTimers();
            if (settingsCollection.ExportSettings.ExportPrompt != null && settingsCollection.ExportSettings.ExportPrompt.OnStop && !timerForInteraction.FullyExported && !automatedStop && !timerForInteraction.LocalTimer)
            {
                ExportPrompt?.Invoke(this, new ExportPromptDetail(uniqueId, stopTime));
            }
        }

        public Guid? GetRunningTimerId()
        {
            var runningTimer = timerList.FirstOrDefault(timer => timer.IsRunning);

            return runningTimer?.UniqueId;
        }

        public void RemoveTimersOlderThanDays(int keepTimersForDays)
        {
            if (keepTimersForDays > 0)
            {
                keepTimersForDays = keepTimersForDays * -1;
            }

            var possiblePurgeDates = timerList.Where(timer => timer.DateStarted.Date != DateTime.Now.Date && timer.DateStarted.Date <= DateTime.Now.AddDays(keepTimersForDays).Date)
                                              .Select(x => x.DateStarted.Date)
                                              .Distinct()
                                              .ToList();

            foreach (var possiblePurgeDate in possiblePurgeDates)
            {
                var dateTimers = timerList.Where(x => x.DateStarted.Date == possiblePurgeDate.Date).ToList();

                if (dateTimers.All(timer => timer.FullyExported && !timer.LocalTimer))
                {
                    timerList.RemoveAll(x => x.DateStarted.Date == possiblePurgeDate.Date);
                }
            }

            SaveTimers();
        }

        public JiraTimer GetTimer(Guid timerGuid)
        {
            return timerList.FirstOrDefault(timer => timer.UniqueId == timerGuid);
        }

        public Guid RenameTimer(Guid timerGuid, Issue newIssue)
        {
            var currentTimer = GetTimer(timerGuid);
            if (currentTimer.IsRunning)
            {
                currentTimer.StopTimer();
            }

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

        public Guid ChangeLocalTimerDescription(Guid timerGuid, string localTimerDescription)
        {
            var currentTimer = GetTimer(timerGuid);
            if (currentTimer.LocalTimer)
            {
                currentTimer.UpdateLocalTimerDescription(localTimerDescription);
                SaveTimers();
                return currentTimer.UniqueId;
            }

            var newGuid = AddLocalTimer(localTimerDescription, currentTimer.DateStarted, currentTimer.ExactCurrentTime, false);
            RemoveTimer(timerGuid);
            return newGuid;
        }

        public Guid ChangeTimerDate(Guid timerGuid, DateTime newStartDate)
        {
            var currentTimer = GetTimer(timerGuid);
            if (currentTimer.IsRunning)
            {
                currentTimer.StopTimer();
            }

            JiraTimer newTimer;
            if (currentTimer.LocalTimer)
            {
                var foundValid = false;
                var nextLocalTimerNumber = 0;
                var currentLocalTimers = GetTimersForADate(newStartDate.Date).Where(x => x.LocalTimer).ToList();
                while (!foundValid)
                {
                    nextLocalTimerNumber++;
                    if (!currentLocalTimers.Any(x => x.JiraReference.EndsWith($"-{nextLocalTimerNumber}")))
                    {
                        foundValid = true;
                    }
                }
                newTimer = new JiraTimer(nextLocalTimerNumber, currentTimer.JiraName, newStartDate.Date, currentTimer.ExactCurrentTime);
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
            return new Tuple<int, int>(timerList.Count(jiraTimer => jiraTimer.FullyExported && jiraTimer.CurrentTime > TimeSpan.Zero), timerList.Count(jiraTimer => jiraTimer.CurrentTime > TimeSpan.Zero));
        }

        public TimeSpan GetStoppedTotalExportableTime()
        {
            var unexportedTime = new TimeSpan();
            return timerList.Where(timer => !timer.LocalTimer && !timer.IsRunning && !timer.FullyExported).ToList().Aggregate(unexportedTime, (current, jiraTimer) => current.Add(new TimeSpan(jiraTimer.TimeToExport.Hours, jiraTimer.TimeToExport.Minutes, 0)));
        }

        public TimeSpan GetTotalLocalTime()
        {
            var unexportedTime = new TimeSpan();
            return timerList.Where(timer => timer.LocalTimer && !timer.FullyExported).ToList().Aggregate(unexportedTime, (current, jiraTimer) => current.Add(new TimeSpan(jiraTimer.TimeToExport.Hours, jiraTimer.TimeToExport.Minutes, 0)));
        }

        public TimeSpan GetTotalExportableTime()
        {
            var unexportedTime = new TimeSpan();
            return timerList.Where(timer => !timer.LocalTimer && !timer.FullyExported).ToList().Aggregate(unexportedTime, (current, jiraTimer) => current.Add(new TimeSpan(jiraTimer.TimeToExport.Hours, jiraTimer.TimeToExport.Minutes, 0)));
        }

        public TimeSpan GetTotalExportedTimeThisWeek(DayOfWeek startOfWeek)
        {
            var exportedTime = new TimeSpan();
            return timerList.Where(jiraTimer => jiraTimer.IsThisWeek(startOfWeek)).ToList().Aggregate(exportedTime, (current, jiraTimer) => current.Add(jiraTimer.ExportedTime));
        }

        public TimeSpan GetTotalTimeThisWeekNoSeconds(DayOfWeek startOfWeek)
        {
            var time = new TimeSpan();
            return timerList.Where(jiraTimer => jiraTimer.IsThisWeek(startOfWeek)).ToList().Aggregate(time, (current, jiraTimer) => current.Add(new TimeSpan(jiraTimer.ExactCurrentTime.Hours, jiraTimer.ExactCurrentTime.Minutes, 0)));
        }

        public TimeSpan GetTotalTimeForDate(DateTime timerDate)
        {
            var time = new TimeSpan();
            return timerList.Where(jiraTimer => jiraTimer.DateStarted.Date == timerDate.Date).ToList().Aggregate(time, (current, jiraTimer) => current.Add(new TimeSpan(jiraTimer.ExactCurrentTime.Hours, jiraTimer.ExactCurrentTime.Minutes, jiraTimer.ExactCurrentTime.Seconds)));
        }

        public TimeSpan GetTotalTimeForDateNoSeconds(DateTime timerDate)
        {
            var unexportedTime = new TimeSpan();
            return timerList.Where(timer => timer.DateStarted.Date == timerDate.Date).ToList().Aggregate(unexportedTime, (current, jiraTimer) => current.Add(new TimeSpan(jiraTimer.ExactCurrentTime.Hours, jiraTimer.ExactCurrentTime.Minutes, 0)));
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
            if (settingsCollection.ExportSettings.ExportPrompt != null && settingsCollection.ExportSettings.ExportPrompt.OnManualAdjust && !timer.FullyExported && !timer.LocalTimer)
            {
                if (!addTime)
                {
                    adjustment = adjustment.Negate();
                }

                ExportPrompt?.Invoke(this, new ExportPromptDetail(uniqueId, adjustment));
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
            if (settingsCollection.ExportSettings.ExportPrompt != null && settingsCollection.ExportSettings.ExportPrompt.OnAddIdle && !timer.FullyExported && !timer.LocalTimer)
            {
                var idleTime = new TimeSpan();
                idleTime = idleTimers.Aggregate(idleTime, (current, idleTimer) => current.Add(idleTimer.IdleTimeValue));
                ExportPrompt?.Invoke(this, new ExportPromptDetail(uniqueId, idleTime));
            }
        }

        public void RefreshFromJira(Guid uniqueId, Issue jiraIssue, TimeSpan loggedTime)
        {
            var timer = GetTimer(uniqueId);
            if (timer != null)
            {
                timer.SetJiraExportedTime(loggedTime);
                timer.RefreshFromJira(jiraIssue);
                SaveTimers();
            }
        }
    }
}
