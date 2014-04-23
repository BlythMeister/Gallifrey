using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Gallifrey.Settings;

namespace Gallifrey.Model
{
    public static class JiraTimerCollectionSerializer
    {
        private readonly static string Path = System.IO.Path.Combine(FilePathSettings.DataSavePath, "TimerCollection.bin");

        public static void Serialize(List<JiraTimer> timerCollection)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(Path, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, timerCollection);
            stream.Close();
        }

        public static List<JiraTimer> DeSerialize()
        {
            List<JiraTimer> timers;
            IFormatter formatter = new BinaryFormatter();

            if (File.Exists(Path))
            {
                Stream stream = new FileStream(Path,
                                               FileMode.Open,
                                               FileAccess.Read,
                                               FileShare.Read);
                timers = (List<JiraTimer>)formatter.Deserialize(stream);
                stream.Close();
            }
            else
            {
                timers = new List<JiraTimer>();
            }

            return timers;
        }
    }
}
