using System;
using System.Collections.Generic;
using System.IO;
using Gallifrey.JiraTimers;
using Gallifrey.Settings;
using Newtonsoft.Json;

namespace Gallifrey.Serialization
{
    internal static class JiraTimerCollectionSerializer
    {
        private readonly static string SavePath = Path.Combine(FilePathSettings.DataSavePath, "TimerCollection.dat");

        internal static void Serialize(List<JiraTimer> timerCollection)
        {
            if (!Directory.Exists(FilePathSettings.DataSavePath)) Directory.CreateDirectory(FilePathSettings.DataSavePath);


            File.WriteAllText(SavePath, DataEncryption.Encrypt(JsonConvert.SerializeObject(timerCollection)));
        }

        internal static List<JiraTimer> DeSerialize()
        {
            List<JiraTimer> timers;

            if (File.Exists(SavePath))
            {
                try
                {
                    var text = TryAndFixJson(DataEncryption.Decrypt(File.ReadAllText(SavePath)));
                    timers = JsonConvert.DeserializeObject<List<JiraTimer>>(text);
                }
                catch (Exception)
                {
                    timers = new List<JiraTimer>();
                }
            }
            else
            {
                timers = new List<JiraTimer>();
            }

            return timers;
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
