using Gallifrey.Serialization;

namespace Gallifrey.Settings
{
    public interface ISettingsCollection
    {
        IAppSettings AppSettings { get; }
        IJiraConnectionSettings JiraConnectionSettings { get; }
        IUiSettings UiSettings { get; }
        void SaveSettings();
    }

    public class SettingsCollection : ISettingsCollection
    {
        public IAppSettings AppSettings { get; private set; }
        public IJiraConnectionSettings JiraConnectionSettings { get; private set; }
        public IUiSettings UiSettings { get; private set; }

        public SettingsCollection()
        {
            AppSettings = new AppSettings();
            JiraConnectionSettings = new JiraConnectionSettings();
            UiSettings = new UiSettings();
        }

        public SettingsCollection(IAppSettings appSettings, IJiraConnectionSettings jiraConnectionSettings, IUiSettings uiSettings)
        {
            AppSettings = appSettings;
            JiraConnectionSettings = jiraConnectionSettings;
            UiSettings = uiSettings;
        }

        public void SaveSettings()
        {
            SettingsCollectionSerializer.Serialize(this);
        }
    }
}
