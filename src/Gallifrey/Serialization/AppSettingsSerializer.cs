using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Gallifrey.Settings;
using Newtonsoft.Json;

namespace Gallifrey.Serialization
{
    public static class AppSettingsSerializer
    {
        private readonly static string SavePath = Path.Combine(FilePathSettings.DataSavePath, "AppSettings.dat");

        public static void Serialize(AppSettings appSettings)
        {
            if (!Directory.Exists(FilePathSettings.DataSavePath)) Directory.CreateDirectory(FilePathSettings.DataSavePath);
            
            File.WriteAllText(SavePath, JsonConvert.SerializeObject(appSettings));
        }

        public static AppSettings DeSerialize()
        {
            AppSettings appSettings;
            
            if (File.Exists(SavePath))
            {
                var text = File.ReadAllText(SavePath);
                appSettings = JsonConvert.DeserializeObject<AppSettings>(text);
            }
            else
            {
                appSettings = new AppSettings();
            }

            return appSettings;
        }
    }
}
