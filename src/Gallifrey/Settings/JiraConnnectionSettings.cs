using Newtonsoft.Json;
using System;

namespace Gallifrey.Settings
{
    public interface IJiraConnectionSettings
    {
        string JiraUrl { get; set; }
        string JiraUsername { get; set; }
        string JiraPassword { get; set; }
        bool UseTempo { get; set; }
        string TempoToken { get; set; }
        string JiraHost { get; }
    }

    public class JiraConnectionSettings : IJiraConnectionSettings
    {
        public string JiraUrl { get; set; }
        public string JiraUsername { get; set; }
        public string JiraPassword { get; set; }
        public bool UseTempo { get; set; }
        public string TempoToken { get; set; }

        [JsonIgnore]
        public string JiraHost
        {
            get
            {
                if (string.IsNullOrWhiteSpace(JiraUrl))
                {
                    return "";
                }
                try
                {
                    return new Uri(JiraUrl).Host.ToLower();
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }
    }
}
