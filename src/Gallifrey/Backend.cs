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
        public JiraConnnectionSettings JiraConnnectionSettings;
        public JiraConnection JiraConnection;

        public Backend()
        {
            AppSettings = AppSettingsSerializer.DeSerialize();
            JiraConnnectionSettings = JiraConnectionSettingsSerializer.DeSerialize();
            JiraTimerCollection = new JiraTimerCollection();
        }

        public void Initialise()
        {
            JiraConnection = new JiraConnection(JiraConnnectionSettings);
        }

        public void SaveJiraConnectionSettings()
        {
            JiraConnnectionSettings.SaveSettings();
            if (JiraConnection == null)
            {
                JiraConnection = new JiraConnection(JiraConnnectionSettings);
            }
            else
            {
                JiraConnection.ReConnect(JiraConnnectionSettings);
            }
        }
    }
}
