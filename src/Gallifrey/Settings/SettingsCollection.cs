using Gallifrey.Serialization;
using Newtonsoft.Json;

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
        string UserHash { get; }
        string SiteHash { get; }
    }

    public class SettingsCollection : ISettingsCollection
    {
        public IAppSettings AppSettings { get; private set; }
        public IJiraConnectionSettings JiraConnectionSettings { get; private set; }
        public IUiSettings UiSettings { get; private set; }
        public IInternalSettings InternalSettings { get; private set; }
        public IExportSettings ExportSettings { get; private set; }
        [JsonIgnore] public string InstallationHash => DataEncryption.GetSha256Hash($"{InternalSettings.InstallationInstanceId}-{JiraConnectionSettings.JiraUsername}-{JiraConnectionSettings.JiraHost}");
        [JsonIgnore] public string UserHash => DataEncryption.GetSha256Hash($"{JiraConnectionSettings.JiraUsername.ToLower()}-{JiraConnectionSettings.JiraHost}");
        [JsonIgnore] public string SiteHash => DataEncryption.GetSha256Hash(JiraConnectionSettings.JiraHost);
        private bool isIntialised;

        // ReSharper disable once UnusedMember.Global
        public SettingsCollection()
        {
            AppSettings = new AppSettings();
            JiraConnectionSettings = new JiraConnectionSettings();
            UiSettings = new UiSettings();
            InternalSettings = new InternalSettings();
            ExportSettings = new ExportSettings();
            isIntialised = false;
        }

        public SettingsCollection(ISettingsCollection settings)
        {
            AppSettings = settings.AppSettings;
            JiraConnectionSettings = settings.JiraConnectionSettings;
            UiSettings = settings.UiSettings;
            InternalSettings = settings.InternalSettings;
            ExportSettings = settings.ExportSettings;

            isIntialised = false;
        }

        public void Initialise()
        {
            var settings = SettingsCollectionSerializer.DeSerialize();

            AppSettings = settings.AppSettings;
            JiraConnectionSettings = settings.JiraConnectionSettings;
            UiSettings = settings.UiSettings;
            InternalSettings = settings.InternalSettings;
            ExportSettings = settings.ExportSettings;

            isIntialised = true;
        }

        public void SaveSettings()
        {
            if (!isIntialised) return;
            SettingsCollectionSerializer.Serialize(this);
        }
    }
}
