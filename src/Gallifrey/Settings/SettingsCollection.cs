using Gallifrey.Serialization;

namespace Gallifrey.Settings
{
    public interface ISettingsCollection
    {
        IAppSettings AppSettings { get; }
        IJiraConnectionSettings JiraConnectionSettings { get; }
        IUiSettings UiSettings { get; }
        IInternalSettings InternalSettings { get; }
        IExportSettings ExportSettings { get; }
        string InstallationHash { get; }
        void SaveSettings();
    }

    public class SettingsCollection : ISettingsCollection
    {
        public IAppSettings AppSettings { get; }
        public IJiraConnectionSettings JiraConnectionSettings { get; }
        public IUiSettings UiSettings { get; }
        public IInternalSettings InternalSettings { get; }
        public IExportSettings ExportSettings { get; }
        public string InstallationHash => DataEncryption.GetSha256Hash($"{InternalSettings.InstallationInstaceId}-{JiraConnectionSettings.JiraUsername}");

        public SettingsCollection()
        {
            AppSettings = new AppSettings();
            JiraConnectionSettings = new JiraConnectionSettings();
            UiSettings = new UiSettings();
            InternalSettings = new InternalSettings();
            ExportSettings = new ExportSettings();
        }

        public SettingsCollection(IAppSettings appSettings, IJiraConnectionSettings jiraConnectionSettings, IUiSettings uiSettings, IInternalSettings internalSettings, IExportSettings exportSettings)
        {
            AppSettings = appSettings;
            JiraConnectionSettings = jiraConnectionSettings;
            UiSettings = uiSettings;
            InternalSettings = internalSettings;
            ExportSettings = exportSettings;
        }

        public void SaveSettings()
        {
            SettingsCollectionSerializer.Serialize(this);
        }
    }
}
