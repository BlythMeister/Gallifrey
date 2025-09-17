using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ProjectSearchResult
    {
        public List<Project> values { get; set; }
        public bool isLast { get; set; }
        public int maxResults { get; set; }
        public int startAt { get; set; }
        public int total { get; set; }
    }
}