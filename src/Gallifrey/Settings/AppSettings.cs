using Gallifrey.Serialization;

namespace Gallifrey.Settings
{
    public class AppSettings
    {
        public void SaveSettings()
        {
            AppSettingsSerializer.Serialize(this);    
        }
    }
}
