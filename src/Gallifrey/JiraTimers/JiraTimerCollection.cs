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
        }

        public void AddTimer(Issue jiraIssue, DateTime startDate, TimeSpan seedTime, bool startNow)
        {
            var newTimer = new JiraTimer(jiraIssue, startDate, seedTime);
            AddTimer(newTimer);
            if (startNow)
            {
                StartTimer(newTimer.UniqueId);
            }
        }

        public void RemoveTimer(Guid uniqueId)
        {
            timerList.Remove(timerList.First(timer => timer.UniqueId == uniqueId));
        }

        public void StartTimer(Guid uniqueId)
        {
            var timerForInteration = timerList.First(timer => timer.UniqueId == uniqueId);
            if (timerForInteration.DateStarted.Date != DateTime.Now.Date)
            {
                timerForInteration = new JiraTimer(timerForInteration, DateTime.Now);
                AddTimer(timerForInteration);
                uniqueId = timerForInteration.UniqueId;
            }

            timerForInteration.StartTimer();

            var runningTimerId = GetRunningTimerId();
            if (runningTimerId.HasValue && runningTimerId.Value != uniqueId)
            {
                timerList.First(timer => timer.UniqueId == runningTimerId.Value).StopTimer();
            }
        }

        public void StopTimer(Guid uniqueId)
        {
            var timerForInteration = timerList.First(timer => timer.UniqueId == uniqueId);
            timerForInteration.StopTimer();
        }

        public Guid? GetRunningTimerId()
        {
            var runningTimer = timerList.FirstOrDefault(timer => timer.IsRunning);
            if (runningTimer == null)
            {
                return null;
            }
            else
            {
                return runningTimer.UniqueId;
            }
        }
    }
}
