using Gallifrey.Serialization;

namespace Gallifrey.Settings
{
    public interface ISettingsCollection
    {
        IAppSettings AppSettings { get; }
        IJiraConnectionSettings JiraConnectionSettings { get; }
        IHarvestConnectionSettings HarvestConnectionSettings { get; }
        IUiSettings UiSettings { get; }
        IInternalSettings InternalSettings { get; }
        IExportSettings ExportSettings { get; }
        void SaveSettings();
    }

    public class SettingsCollection : ISettingsCollection
    {
        public IAppSettings AppSettings { get; private set; }
        public IJiraConnectionSettings JiraConnectionSettings { get; private set; }
        public IHarvestConnectionSettings HarvestConnectionSettings { get; private set; }
        public IUiSettings UiSettings { get; private set; }
        public IInternalSettings InternalSettings { get; private set; }
        public IExportSettings ExportSettings { get; private set; }

        public SettingsCollection()
        {
            AppSettings = new AppSettings();
            JiraConnectionSettings = new JiraConnectionSettings();
            HarvestConnectionSettings = new HarvestConnectionSettings();
            UiSettings = new UiSettings();
            InternalSettings = new InternalSettings();
            ExportSettings = new ExportSettings();
        }

        public SettingsCollection(IAppSettings appSettings, IJiraConnectionSettings jiraConnectionSettings, IHarvestConnectionSettings harvestConnectionSettings, IUiSettings uiSettings, IInternalSettings internalSettings, IExportSettings exportSettings)
        {
            AppSettings = appSettings;
            JiraConnectionSettings = jiraConnectionSettings;
            HarvestConnectionSettings = harvestConnectionSettings;
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
