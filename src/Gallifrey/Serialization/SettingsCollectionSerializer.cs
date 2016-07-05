using Gallifrey.Settings;

namespace Gallifrey.Serialization
{
    internal static class SettingsCollectionSerializer
    {
        private static ItemSerializer<SettingsCollection> serializer;

        internal static void Serialize(SettingsCollection settingsCollection)
        {
            if (serializer == null)
            {
                serializer = new ItemSerializer<SettingsCollection>("Settings.dat");
            }

            serializer.Serialize(settingsCollection);
        }

        internal static SettingsCollection DeSerialize()
        {
            if (serializer == null)
            {
                serializer = new ItemSerializer<SettingsCollection>("Settings.dat");
            }

            var settings = serializer.DeSerialize();

            return SetMissingDefaults(settings);
        }

        private static SettingsCollection SetMissingDefaults(SettingsCollection settings)
        {
            var uiSettingsDefaultsSet = settings.UiSettings.SetDefaults();
            var internalSettingsDefaultsSet = settings.InternalSettings.SetDefaults();
            
            if (uiSettingsDefaultsSet || internalSettingsDefaultsSet)
            {
                Serialize(settings);
            }

            return settings;
        }
    }
}
