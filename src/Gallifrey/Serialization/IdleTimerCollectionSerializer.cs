using System;
using System.Collections.Generic;
using System.IO;
using Gallifrey.IdleTimers;
using Gallifrey.Settings;
using Newtonsoft.Json;

namespace Gallifrey.Serialization
{
    internal static class IdleTimerCollectionSerializer
    {
        private readonly static string SavePath = System.IO.Path.Combine(FilePathSettings.DataSavePath, "IdleTimerCollection.dat");

        internal static void Serialize(List<IdleTimer> timerCollection)
        {
            if (!Directory.Exists(FilePathSettings.DataSavePath)) Directory.CreateDirectory(FilePathSettings.DataSavePath);

            
            File.WriteAllText(SavePath, DataEncryption.Encrypt(JsonConvert.SerializeObject(timerCollection)));
        }

        internal static List<IdleTimer> DeSerialize()
        {
            List<IdleTimer> timers;
            
            if (File.Exists(SavePath))
            {
                try
                {
                    var text = DataEncryption.Decrypt(File.ReadAllText(SavePath));
                    timers = JsonConvert.DeserializeObject<List<IdleTimer>>(text);
                }
                catch (Exception)
                {
                    timers = new List<IdleTimer>();
                }

            }
            else
            {
                timers = new List<IdleTimer>();
            }

            return timers;
        }
    }
}
