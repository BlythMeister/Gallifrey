using System;
using System.Linq;

namespace Gallifrey.Jira.Model
{
    public class Issue
    {
        public string key { get; set; }
        public Fields fields { get; set; }

        public TimeSpan GetCurrentLoggedTimeForDate(DateTime date, string userName)
        {
            var loggedTime = new TimeSpan();

            foreach (var worklog in fields.worklog.worklogs.Where(worklog => worklog.started.Date == date.Date && worklog.author.name.ToLower() == userName.ToLower()))
            {
                loggedTime = loggedTime.Add(new TimeSpan(0, 0, (int)worklog.timeSpentSeconds));
            }

            return loggedTime;
        }
    }
}