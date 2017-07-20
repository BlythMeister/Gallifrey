namespace Gallifrey.Settings
{
    public interface IJiraConnectionSettings
    {
        string JiraUrl { get; set; }
        string JiraUsername { get; set; }
        string JiraPassword { get; set; }
        bool UseTempo { get; set; }
    }

    public class JiraConnectionSettings : IJiraConnectionSettings
    {
        public string JiraUrl { get; set; }
        public string JiraUsername { get; set; }
        public string JiraPassword { get; set; }
        public bool UseTempo { get; set; }
    }
}
