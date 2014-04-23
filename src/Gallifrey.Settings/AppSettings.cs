using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Gallifrey.Settings
{
    public class AppSettings
    {
        private readonly string SavePath = Path.Combine(FilePathSettings.DataSavePath, "AppSettings.bin");
        public string JiraUrl { get; set; }
        public string JiraUsername { get; set; }
        public string JiraPassword { get; set; }

        public AppSettings()
        {
            IFormatter formatter = new BinaryFormatter();

            if (File.Exists(SavePath))
            {
                Stream stream = new FileStream(SavePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                SetSettings((AppSettings) formatter.Deserialize(stream));
                stream.Close();
            }
        }

        public void SaveSettings()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, this);
            stream.Close();
        }

        private void SetSettings(AppSettings readAppSettings)
        {
            JiraUrl = readAppSettings.JiraUrl;
            JiraUsername = readAppSettings.JiraUsername;
            JiraPassword = readAppSettings.JiraPassword;
        }
    }
}
