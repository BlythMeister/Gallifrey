using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Issue
    {
        public string key { get; set; }
        public Fields fields { get; set; }
    }
}