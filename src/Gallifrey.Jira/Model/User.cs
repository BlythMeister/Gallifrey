using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class User
    {
        public string key { get; set; }
        public string name { get; set; }
        public string displayName { get; set; }
        public string emailAddress { get; set; }
        public string accountId { get; set; }
        public bool active { get; set; }
    }
}
