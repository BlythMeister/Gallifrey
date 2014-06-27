using System.Collections.Generic;
using Gallifrey.JiraIntegration;

namespace Gallifrey.Comparers
{
    public class DuplicateRecentLogComparer : IEqualityComparer<RecentJira>
    {
        public bool Equals(RecentJira x, RecentJira y)
        {
            return x.JiraReference == y.JiraReference;
        }

        public int GetHashCode(RecentJira recentJira)
        {
            return GetHashCode();
        }
    }
}