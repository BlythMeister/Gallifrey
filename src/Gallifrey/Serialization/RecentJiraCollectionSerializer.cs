using System.Collections.Generic;
using Gallifrey.JiraIntegration;

namespace Gallifrey.Serialization
{
    internal static class RecentJiraCollectionSerializer
    {
        private static ItemSerializer<List<RecentJira>> serializer;

        internal static void Serialize(List<RecentJira> recentJiraCollection)
        {
            if (serializer == null)
            {
                serializer = new ItemSerializer<List<RecentJira>>("RecentJira.dat");
            }

            serializer.Serialize(recentJiraCollection);
        }

        internal static List<RecentJira> DeSerialize()
        {
            if (serializer == null)
            {
                serializer = new ItemSerializer<List<RecentJira>>("RecentJira.dat");
            }

            return serializer.DeSerialize();
        }
    }
}
