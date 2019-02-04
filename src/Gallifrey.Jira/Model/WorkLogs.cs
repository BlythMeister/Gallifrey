using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class WorkLogs
    {
        public List<WorkLog> worklogs { get; set; }
    }
}
