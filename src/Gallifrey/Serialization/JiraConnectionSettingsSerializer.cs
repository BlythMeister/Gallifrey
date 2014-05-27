using System;
using System.IO;
using Gallifrey.Settings;
using Newtonsoft.Json;

namespace Gallifrey.Serialization
{
    internal static class JiraConnectionSettingsSerializer
    {
        private readonly static string SavePath = Path.Combine(FilePathSettings.DataSavePath, "JiraConnectionSettings.dat");

        internal static void Serialize(JiraConnectionSettings appSettings)
        {
            if (!Directory.Exists(FilePathSettings.DataSavePath)) Directory.CreateDirectory(FilePathSettings.DataSavePath);
            
            File.WriteAllText(SavePath, DataEncryption.Encrypt(JsonConvert.SerializeObject(appSettings)));
        }

        internal static JiraConnectionSettings DeSerialize()
        {
            JiraConnectionSettings jiraConnectionSettings;
            
            if (File.Exists(SavePath))
            {
                try
                {
                    var text = DataEncryption.Decrypt(File.ReadAllText(SavePath));
                    jiraConnectionSettings = JsonConvert.DeserializeObject<JiraConnectionSettings>(text);
                }
                catch (Exception)
                {
                    jiraConnectionSettings = new JiraConnectionSettings();
                }
            }
            else
            {
                jiraConnectionSettings = new JiraConnectionSettings();
            }

            return jiraConnectionSettings;
        }
    }
}
