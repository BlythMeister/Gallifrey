using System;

namespace Gallifrey.Jira
{
    public class WorkLog
    {
        public string timeSpent { get; set; }
        public int timeSpentSeconds { get; set; }
        public string comment { get; set; }
        public DateTime created { get; set; }
        public User author { get; set; }
    }
}