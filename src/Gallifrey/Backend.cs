using System;
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

        public Backend()
        {
            AppSettings = AppSettingsSerializer.DeSerialize();
            AppSettings.SaveSettings();
            JiraConnection = new JiraConnection(AppSettings);
            JiraTimerCollection = new JiraTimerCollection(JiraConnection);
        }
    }
}
