using System.Collections.Generic;
using Gallifrey.JiraTimers;

namespace Gallifrey.Serialization
{
    internal static class JiraTimerCollectionSerializer
    {
        private static ItemSerializer<List<JiraTimer>> serializer;

        internal static void Serialize(List<JiraTimer> timerCollection)
        {
            if (serializer == null)
            {
                serializer = new ItemSerializer<List<JiraTimer>>("TimerCollection.dat");
            }

            serializer.Serialize(timerCollection);
        }

        internal static List<JiraTimer> DeSerialize()
        {
            if (serializer == null)
            {
                serializer = new ItemSerializer<List<JiraTimer>>("TimerCollection.dat");
            }

            return serializer.DeSerialize();
        }
    }
}
