using System;

namespace Gallifrey.Jira
{
    public class WorkLog
    {
        public string timeSpent { get; set; }
        public int timeSpentSeconds { get; set; }
        public string comment { get; set; }
        public DateTime started { get; set; }
        public User author { get; set; }
    }
}