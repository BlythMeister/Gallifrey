using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class TempoWorkLog
    {
        public class TempoWorkLogIssue
        {
            public string key { get; set; }
            public double? remainingEstimateSeconds { get; set; }
        }

        public class TempoWorkLogUser
        {
            public string key { get; set; }
            public string name { get; set; }
        }

        public TempoWorkLogIssue issue { get; set; }
        public double timeSpentSeconds { get; set; }
        public string dateStarted { get; set; }
        public string comment { get; set; }
        public TempoWorkLogUser author { get; set; }
    }
}
