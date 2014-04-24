using System.Collections.Generic;
using Gallifrey.IntegrationPoints;
using Gallifrey.Models;
using Gallifrey.Serialization;
using Gallifrey.Settings;

namespace Gallifrey
{
    public class Backend
    {
        public List<JiraTimer> TimerList;
        public AppSettings AppSettings;
        public JiraConnection JiraConnection;

        public Backend()
        {
            TimerList = JiraTimerCollectionSerializer.DeSerialize();
            AppSettings = AppSettingsSerializer.DeSerialize();
            JiraConnection = new JiraConnection(AppSettings);
        }
    }
}
