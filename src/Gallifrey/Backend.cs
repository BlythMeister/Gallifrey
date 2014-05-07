using Gallifrey.IdleTimers;
using Gallifrey.IntegrationPoints;
using Gallifrey.JiraTimers;
using Gallifrey.Serialization;
using Gallifrey.Settings;

namespace Gallifrey
{
    public class Backend
    {
        public JiraTimerCollection JiraTimerCollection;
        public IdleTimerCollection IdleTimerCollection;
        public AppSettings AppSettings;
        public JiraConnnectionSettings JiraConnnectionSettings;
        public JiraConnection JiraConnection;
        
        public Backend()
        {
            AppSettings = AppSettingsSerializer.DeSerialize();
            JiraConnnectionSettings = JiraConnectionSettingsSerializer.DeSerialize();
            JiraTimerCollection = new JiraTimerCollection();
            IdleTimerCollection = new IdleTimerCollection();
        }

        public void Initialise()
        {
            JiraConnection = new JiraConnection(JiraConnnectionSettings);
        }

        public void Close()
        {
            JiraTimerCollection.SaveTimers();
            IdleTimerCollection.SaveTimers();
            AppSettings.SaveSettings();
            JiraConnnectionSettings.SaveSettings();
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

        public void StartIdleTimer()
        {
            IdleTimerCollection.NewLockTimer();
        }

        public void StopIdleTimer()
        {
            IdleTimerCollection.StopLockedTimers();
        }
    }
}
