using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Fields
    {
        public string summary { get; set; }
        public Project project { get; set; }
        public Issue parent { get; set; }
        public TimeTracking timetracking { get; set; }
    }
}
