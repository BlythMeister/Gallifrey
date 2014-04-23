using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Gallifrey.Settings;

namespace Gallifrey.Model
{
    public class TimerCollectionSerializer
    {
        private string path = Path.Combine(FilePathSettings.DataSavePath, "TimerCollection.bin");

        public void Serialize(IEnumerable<Timer> timerCollection)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, timerCollection);
            stream.Close();
        }

        public IEnumerable<Timer> DeSerialize()
        {
            List<Timer> timers;
            IFormatter formatter = new BinaryFormatter();

            if (File.Exists(path))
            {
                Stream stream = new FileStream(path,
                                               FileMode.Open,
                                               FileAccess.Read,
                                               FileShare.Read);
                timers = (List<Timer>)formatter.Deserialize(stream);
                stream.Close();
            }
            else
            {
                timers = new List<Timer>();
            }

            return timers;
        }
    }
}
