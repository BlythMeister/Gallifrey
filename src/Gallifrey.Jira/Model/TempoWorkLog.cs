using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class TempoWorkLog
    {
        public class TempoWorkLogIssue
        {
            public string key { get; set; }
        }

        public class TempoWorkLogUser
        {
            public string accountId { get; set; }
            public string displayName { get; set; }
        }

        public TempoWorkLogIssue issue { get; set; }
        public double timeSpentSeconds { get; set; }
        public string startDate { get; set; }
        public string startTime { get; set; }
        public string description { get; set; }
        public TempoWorkLogUser author { get; set; }
    }
}
