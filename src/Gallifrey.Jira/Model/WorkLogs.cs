using System.Collections.Generic;

namespace Gallifrey.Jira.Model
{
    public class WorkLogs
    {
        public List<WorkLog> worklogs { get; set; }
        public int total { get; set; }
        public int maxResults { get; set; }
    }
}