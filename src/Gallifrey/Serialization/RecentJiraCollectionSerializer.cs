using System;
using System.Collections.Generic;
using System.IO;
using Gallifrey.JiraIntegration;
using Gallifrey.Settings;
using Newtonsoft.Json;

namespace Gallifrey.Serialization
{
    internal static class RecentJiraCollectionSerializer
    {
        private readonly static string SavePath = Path.Combine(FilePathSettings.DataSavePath, "RecentJira.dat");

        internal static void Serialize(List<RecentJira> recentJiraCollection)
        {
            if (!Directory.Exists(FilePathSettings.DataSavePath)) Directory.CreateDirectory(FilePathSettings.DataSavePath);

            File.WriteAllText(SavePath, DataEncryption.Encrypt(JsonConvert.SerializeObject(recentJiraCollection)));
        }

        internal static List<RecentJira> DeSerialize()
        {
            List<RecentJira> recentJiraCollection;
            
            if (File.Exists(SavePath))
            {
                try
                {
                    var text = TryAndFixJson(DataEncryption.Decrypt(File.ReadAllText(SavePath)));
                    recentJiraCollection = JsonConvert.DeserializeObject<List<RecentJira>>(text);
                }
                catch (Exception)
                {
                    recentJiraCollection = new List<RecentJira>();    
                }
                
            }
            else
            {
                recentJiraCollection = new List<RecentJira>();    
            }

            return recentJiraCollection;
        }

        private static string TryAndFixJson(string text)
        {
            text = text.Replace("[{\"JiraReference\"", "[{\"JiraInfo\":{\"JiraReference\"");
            text = text.Replace(",{\"JiraReference\"", ",{\"JiraInfo\":{\"JiraReference\"");
            text = text.Replace("\",\"DateStarted\"", "\"},\"DateStarted\"");
            return text;
        }
    }
}
