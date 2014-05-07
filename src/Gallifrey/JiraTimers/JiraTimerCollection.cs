using System;
using System.Collections.Generic;
using System.Linq;
using Atlassian.Jira;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.Serialization;

namespace Gallifrey.JiraTimers
{
    public class JiraTimerCollection
    {
        private readonly List<JiraTimer> timerList;

        public JiraTimerCollection()
        {
            timerList = JiraTimerCollectionSerializer.DeSerialize();
        }

        public void SaveTimers()
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
        
        public void AddTimer(Issue jiraIssue, DateTime startDate, TimeSpan seedTime)
        {
            var newTimer = new JiraTimer(jiraIssue, startDate, seedTime, new TimeSpan(), Guid.NewGuid());

            if (timerList.Any(timer => timer.JiraReference == newTimer.JiraReference && timer.DateStarted.Date == newTimer.DateStarted.Date))
            {
                throw new DuplicateTimerException("Already have a timer for this task on this day!");
            }

            timerList.Add(newTimer);
        }

        public void RemoveTimer(Guid uniqueId)
        {
            timerList.Remove(timerList.First(timer => timer.UniqueId == uniqueId));
        }

        public bool IsTimerForToday(Guid uniqueId)
        {
            var timerForInteration = timerList.First(timer => timer.UniqueId == uniqueId);
            return timerForInteration.DateStarted.Date == DateTime.Now.Date;
        }

        public void StartTimer(Guid uniqueId)
        {
            var timerForInteration = timerList.First(timer => timer.UniqueId == uniqueId);
            timerForInteration.StartTimer();

            foreach (var runningTimer in GetRunningTimers().Where(timer => timer.UniqueId != uniqueId))
            {
                runningTimer.StopTimer();
            }
        }

        public void StopTimer(Guid uniqueId)
        {
            var timerForInteration = timerList.First(timer => timer.UniqueId == uniqueId);
            timerForInteration.StopTimer();
        }

        public IEnumerable<JiraTimer> GetRunningTimers()
        {
            return timerList.Where(timer => timer.IsRunning);
        }
    }
}
