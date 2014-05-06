using Gallifrey.IntegrationPoints;
using Gallifrey.JiraTimers;
using Gallifrey.Serialization;
using Gallifrey.Settings;

namespace Gallifrey
{
    public class Backend
    {
        public JiraTimerCollection JiraTimerCollection;
        public AppSettings AppSettings;
        public JiraConnection JiraConnection;

        public void Initialise()
        {
            AppSettings = AppSettingsSerializer.DeSerialize();
            JiraConnection = new JiraConnection(AppSettings);
            JiraTimerCollection = new JiraTimerCollection(JiraConnection);
        }

        public void SaveAppSettings()
        {
            AppSettings.SaveSettings();
            if(JiraConnection != null) JiraConnection.ReConnect(AppSettings);
        }
    }
}
