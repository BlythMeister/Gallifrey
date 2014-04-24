using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Gallifrey.Models;
using Gallifrey.Settings;

namespace Gallifrey.Serialization
{
    public static class AppSettingsSerializer
    {
        private readonly static string SavePath = Path.Combine(FilePathSettings.DataSavePath, "AppSettings.bin");

        public static void Serialize(AppSettings appSettings)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, appSettings);
            stream.Close();
        }

        public static AppSettings DeSerialize()
        {
            AppSettings appSettings;
            IFormatter formatter = new BinaryFormatter();

            if (File.Exists(SavePath))
            {
                Stream stream = new FileStream(SavePath,
                                               FileMode.Open,
                                               FileAccess.Read,
                                               FileShare.Read);
                appSettings = (AppSettings)formatter.Deserialize(stream);
                stream.Close();
            }
            else
            {
                appSettings = new AppSettings();
            }

            return appSettings;
        }
    }
}
