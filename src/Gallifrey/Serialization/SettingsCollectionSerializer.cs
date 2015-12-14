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
            var setANewDefault = false;

            if (settings.UiSettings.SetDefaults())
            {
                setANewDefault = true;
            }

            if (settings.InternalSettings.SetDefaults())
            {
                setANewDefault = true;
            }

            if (setANewDefault)
            {
                Serialize(settings);
            }

            return settings;
        }
    }
}
