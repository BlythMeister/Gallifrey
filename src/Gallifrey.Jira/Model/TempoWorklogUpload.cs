using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class TempoWorkLogUpload
    {
        public long issueId { get; set; }
        public double timeSpentSeconds { get; set; }
        public string startDate { get; set; }
        public string startTime { get; set; }
        public string description { get; set; }
        public string authorAccountId { get; set; }
        public double remainingEstimateSeconds { get; set; }
    }
}
