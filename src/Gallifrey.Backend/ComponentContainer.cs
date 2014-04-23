using System.Collections.Generic;
using Gallifrey.JiraIntergration;
using Gallifrey.Model;
using Gallifrey.Settings;

namespace Gallifrey.Backend
{
    public class ComponentContainer
    {
        public List<JiraTimer> TimerList;
        public AppSettings AppSettings;
        public JiraConnection JiraConnection;

        public ComponentContainer()
        {
            TimerList = JiraTimerCollectionSerializer.DeSerialize();
            AppSettings = new AppSettings();
            JiraConnection = new JiraConnection(AppSettings);
        }
    }
}
