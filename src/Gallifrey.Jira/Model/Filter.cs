using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Filter
    {
        public string name { get; set; }
        public string jql { get; set; }
    }
}