using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Project
    {
        public string key { get; set; }
        public string name { get; set; }
    }
}
