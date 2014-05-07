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
        private readonly static string SavePath = System.IO.Path.Combine(FilePathSettings.DataSavePath, "TimerCollection.dat");

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
                    var text = DataEncryption.Decrypt(File.ReadAllText(SavePath));
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
    }
}
