using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class TempoWorkLog
    {
        public class TempoWorkLogIssue
        {
            public long id { get; set; }
        }

        public TempoWorkLogIssue issue { get; set; }
        public double timeSpentSeconds { get; set; }
    }
}
