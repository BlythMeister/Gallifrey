namespace Gallifrey.Jira.Model
{
    public class TempoWorkLog
    {
        public class TempoWorkLogIssue
        {
            public string key { get; set; }
            public double remainingEstimateSeconds { get; set; }
        }

        public class TempoWorkLogUser
        {
            public string key { get; set; }
            public string name { get; set; }
            public string displayName { get; set; }
        }

        public TempoWorkLogIssue issue { get; set; }
        public double timeSpentSeconds { get; set; }
        public string dateStarted { get; set; }
        public string comment { get; set; }
        public TempoWorkLogUser author { get; set; }
    }
}
