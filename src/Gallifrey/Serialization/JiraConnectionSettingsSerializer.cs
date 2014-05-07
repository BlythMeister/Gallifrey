using System;
using System.IO;
using Gallifrey.Settings;
using Newtonsoft.Json;

namespace Gallifrey.Serialization
{
    internal static class JiraConnectionSettingsSerializer
    {
        private readonly static string SavePath = Path.Combine(FilePathSettings.DataSavePath, "JiraConnectionSettings.dat");

        internal static void Serialize(JiraConnnectionSettings appSettings)
        {
            if (!Directory.Exists(FilePathSettings.DataSavePath)) Directory.CreateDirectory(FilePathSettings.DataSavePath);
            
            File.WriteAllText(SavePath, DataEncryption.Encrypt(JsonConvert.SerializeObject(appSettings)));
        }

        internal static JiraConnnectionSettings DeSerialize()
        {
            JiraConnnectionSettings jiraConnnectionSettings;
            
            if (File.Exists(SavePath))
            {
                try
                {
                    var text = DataEncryption.Decrypt(File.ReadAllText(SavePath));
                    jiraConnnectionSettings = JsonConvert.DeserializeObject<JiraConnnectionSettings>(text);
                }
                catch (Exception)
                {
                    jiraConnnectionSettings = new JiraConnnectionSettings();
                }
            }
            else
            {
                jiraConnnectionSettings = new JiraConnnectionSettings();
            }

            return jiraConnnectionSettings;
        }
    }
}
