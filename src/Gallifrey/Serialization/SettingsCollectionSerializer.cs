using System;
using System.IO;
using Gallifrey.Settings;
using Newtonsoft.Json;

namespace Gallifrey.Serialization
{
    internal static class SettingsCollectionSerializer
    {
        private readonly static string SavePath = Path.Combine(FilePathSettings.DataSavePath, "Settings.dat");
        
        internal static void Serialize(SettingsCollection settingsCollection)
        {
            if (!Directory.Exists(FilePathSettings.DataSavePath)) Directory.CreateDirectory(FilePathSettings.DataSavePath);

            File.WriteAllText(SavePath, DataEncryption.Encrypt(JsonConvert.SerializeObject(settingsCollection)));
        }

        internal static SettingsCollection DeSerialize()
        {
            SettingsCollection settingsCollection;
            
            if (File.Exists(SavePath))
            {
                try
                {
                    var text = DataEncryption.Decrypt(File.ReadAllText(SavePath));
                    settingsCollection = JsonConvert.DeserializeObject<SettingsCollection>(text);
                }
                catch (Exception)
                {
                    settingsCollection = new SettingsCollection();    
                }
                
            }
            else
            {
                settingsCollection = new SettingsCollection();    
            }

            return settingsCollection;
        }
    }
}
