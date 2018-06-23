using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Status
    {
        public string name { get; set; }
        public string id { get; set; }
    }
}