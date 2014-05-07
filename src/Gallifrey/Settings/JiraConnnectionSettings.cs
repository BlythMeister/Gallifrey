using Gallifrey.Serialization;

namespace Gallifrey.Settings
{
    public class JiraConnnectionSettings
    {
        public string JiraUrl { get; set; }
        public string JiraUsername { get; set; }
        public string JiraPassword { get; set; }

        internal void SaveSettings()
        {
            JiraConnectionSettingsSerializer.Serialize(this);    
        }
    }
}
