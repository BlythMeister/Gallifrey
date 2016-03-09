namespace Gallifrey.Jira.Model
{
    public class TimeTracking
    {
        public string originalEstimate { get; set; }
        public string remainingEstimate { get; set; }
        public double originalEstimateSeconds { get; set; }
        public double remainingEstimateSeconds { get; set; }
    }
}