using System;
using System.Collections.Generic;
using System.Linq;
using Atlassian.Jira;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.IdleTimers;
using Gallifrey.Serialization;

namespace Gallifrey.JiraTimers
{
    public interface IJiraTimerCollection
    {
        IEnumerable<DateTime> GetValidTimerDates();
        IEnumerable<JiraTimer> GetTimersForADate(DateTime timerDate);
        Guid AddTimer(Issue jiraIssue, DateTime startDate, TimeSpan seedTime, bool startNow);
        void RemoveTimer(Guid uniqueId);
        void StartTimer(Guid uniqueId);
        void StopTimer(Guid uniqueId);
        Guid? GetRunningTimerId();
        void RemoveTimersOlderThanDays(int keepTimersForDays);
        JiraTimer GetTimer(Guid timerGuid);
        void RenameTimer(Guid timerGuid, Issue newIssue);
        Tuple<int, int> GetNumberExported();
        TimeSpan GetTotalUnexportedTime();
        TimeSpan GetTotalExportedTimeThisWeek();
        TimeSpan GetTotalTimeForDate(DateTime timerDate);
        void AdjustTime(Guid uniqueId, int hours, int minutes, bool addTime);
        void SetJiraExportedTime(Guid uniqueId, TimeSpan loggedTime);
        void AddJiraExportedTime(Guid uniqueId, int hours, int minutes);
        void AddIdleTimer(Guid uniqueId, IdleTimer idleTimer);
    }

    public class JiraTimerCollection : IJiraTimerCollection
    {
        private readonly List<JiraTimer> timerList;

        internal JiraTimerCollection()
        {
            timerList = JiraTimerCollectionSerializer.DeSerialize();
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
            return timerList.Where(timer => timer.DateStarted.Date == timerDate.Date).OrderBy(timer => timer.JiraReference);
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
                timerForInteration = new JiraTimer(timerForInteration, DateTime.Now);
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
            timerForInteration.StopTimer();

            SaveTimers();
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
            foreach (var timer in timerList.Where(timer => timer.DateStarted <= DateTime.Now.AddDays(keepTimersForDays)))
            {
                timerList.Remove(timer);
            }
            SaveTimers();
        }

        public JiraTimer GetTimer(Guid timerGuid)
        {
            return timerList.First(timer => timer.UniqueId == timerGuid);
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

        public Tuple<int, int> GetNumberExported()
        {
            return new Tuple<int, int>(timerList.Count(jiraTimer => jiraTimer.FullyExported), timerList.Count);
        }

        public TimeSpan GetTotalUnexportedTime()
        {
            var unexportedTime = new TimeSpan();
            return timerList.Aggregate(unexportedTime, (current, jiraTimer) => current.Add(jiraTimer.TimeToExport));
        }

        public TimeSpan GetTotalExportedTimeThisWeek()
        {
            var exportedTime = new TimeSpan();
            return timerList.Where(jiraTimer => jiraTimer.IsThisWeek).Aggregate(exportedTime, (current, jiraTimer) => current.Add(jiraTimer.ExportedTime));
        }

        public TimeSpan GetTotalTimeForDate(DateTime timerDate)
        {
            var time = new TimeSpan();
            return timerList.Where(jiraTimer => jiraTimer.DateStarted.Date == timerDate.Date).Aggregate(time, (current, jiraTimer) => current.Add(new TimeSpan(jiraTimer.ExactCurrentTime.Hours, jiraTimer.ExactCurrentTime.Minutes, jiraTimer.ExactCurrentTime.Seconds)));
        }

        public void AdjustTime(Guid uniqueId, int hours, int minutes, bool addTime)
        {
            var timer = GetTimer(uniqueId);
            timer.ManualAdjustment(hours, minutes, addTime);
            SaveTimers();
        }

        public void SetJiraExportedTime(Guid uniqueId, TimeSpan loggedTime)
        {
            var timer = GetTimer(uniqueId);
            timer.SetJiraExportedTime(loggedTime);
            SaveTimers();
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
        }
    }
}