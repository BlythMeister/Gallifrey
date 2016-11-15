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

            if (settings.InternalSettings.ValidateInstallationId())
            {
                Serialize(settings);
            }

            return settings;
        }
    }
}
