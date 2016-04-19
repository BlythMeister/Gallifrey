using System.Collections.Generic;
using Gallifrey.IdleTimers;

namespace Gallifrey.Serialization
{
    internal static class IdleTimerCollectionSerializer
    {
        private static ItemSerializer<List<IdleTimer>> serializer;

        internal static void Serialize(List<IdleTimer> idleTimeCollection)
        {
            if (serializer == null)
            {
                serializer = new ItemSerializer<List<IdleTimer>>("IdleTimerCollection.dat");
            }

            serializer.Serialize(idleTimeCollection);
        }

        internal static List<IdleTimer> DeSerialize()
        {
            if (serializer == null)
            {
                serializer = new ItemSerializer<List<IdleTimer>>("IdleTimerCollection.dat");
            }

            return serializer.DeSerialize();
        }
    }
}
