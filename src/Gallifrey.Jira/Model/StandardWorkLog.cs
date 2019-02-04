using System;

namespace Gallifrey.Jira.Model
{
    public class StandardWorkLog
    {
        public string JiraRef { get; }
        public DateTime LoggedDate { get; }
        public TimeSpan TimeSpent { get; private set; }

        public StandardWorkLog(string jiraRef, DateTime loggedDate, double timeSpent)
        {
            JiraRef = jiraRef;
            LoggedDate = loggedDate;
            TimeSpent = TimeSpan.FromSeconds(timeSpent);
        }

        public void AddTime(double timeSpent)
        {
            var newTime = TimeSpan.FromSeconds(timeSpent);
            TimeSpent = TimeSpent.Add(newTime);
        }
    }
}
