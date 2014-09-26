namespace Gallifrey.Jira.Model
{
    public class Fields
    {
        public string summary { get; set; }
        public Project project { get; set; }
        public WorkLogs worklog { get; set; }
        public Status status { get; set; }
        public Issue parent { get; set; }
        public TimeTracking timetracking { get; set; }
    }
}