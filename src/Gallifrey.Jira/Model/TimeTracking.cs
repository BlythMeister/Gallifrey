namespace Gallifrey.Jira.Model
{
    public class TimeTracking
    {
        public string originalEstimate { get; set; }
        public string remainingEstimate { get; set; }
        public int originalEstimateSeconds { get; set; }
        public int remainingEstimateSeconds { get; set; }
    }
}