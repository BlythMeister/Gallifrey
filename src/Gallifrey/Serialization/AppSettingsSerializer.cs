using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Gallifrey.Settings;
using Newtonsoft.Json;

namespace Gallifrey.Serialization
{
    internal static class AppSettingsSerializer
    {
        private readonly static string SavePath = Path.Combine(FilePathSettings.DataSavePath, "AppSettings.dat");

        internal static void Serialize(AppSettings appSettings)
        {
            if (!Directory.Exists(FilePathSettings.DataSavePath)) Directory.CreateDirectory(FilePathSettings.DataSavePath);
            
            File.WriteAllText(SavePath, DataEncryption.Encrypt(JsonConvert.SerializeObject(appSettings)));
        }

        internal static AppSettings DeSerialize()
        {
            AppSettings appSettings;
            
            if (File.Exists(SavePath))
            {
                try
                {
                    var text = DataEncryption.Decrypt(File.ReadAllText(SavePath));
                    appSettings = JsonConvert.DeserializeObject<AppSettings>(text);
                }
                catch (Exception)
                {
                    appSettings = new AppSettings();    
                }
                
            }
            else
            {
                appSettings = new AppSettings();
            }

            return appSettings;
        }
    }
}
