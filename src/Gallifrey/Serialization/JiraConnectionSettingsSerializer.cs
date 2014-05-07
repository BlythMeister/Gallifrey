using System.IO;
using Gallifrey.Settings;
using Newtonsoft.Json;

namespace Gallifrey.Serialization
{
    public static class JiraConnectionSettingsSerializer
    {
        private readonly static string SavePath = Path.Combine(FilePathSettings.DataSavePath, "AppSettings.dat");

        public static void Serialize(JiraConnnectionSettings appSettings)
        {
            if (!Directory.Exists(FilePathSettings.DataSavePath)) Directory.CreateDirectory(FilePathSettings.DataSavePath);
            
            File.WriteAllText(SavePath, JsonConvert.SerializeObject(appSettings));
        }

        public static JiraConnnectionSettings DeSerialize()
        {
            JiraConnnectionSettings jiraConnnectionSettings;
            
            if (File.Exists(SavePath))
            {
                var text = File.ReadAllText(SavePath);
                jiraConnnectionSettings = JsonConvert.DeserializeObject<JiraConnnectionSettings>(text);
            }
            else
            {
                jiraConnnectionSettings = new JiraConnnectionSettings();
            }

            return jiraConnnectionSettings;
        }
    }
}
