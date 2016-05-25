using System.Collections.Generic;

namespace Gallifrey.Jira.Model
{
    public class WorkLogs
    {
        public List<WorkLog> worklogs { get; set; }
        public double total { get; set; }
        public double maxResults { get; set; }
    }
}