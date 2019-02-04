using Gallifrey.JiraIntegration;
using System.Collections.Generic;

namespace Gallifrey.Comparers
{
    public class DuplicateRecentJiraComparer : IEqualityComparer<RecentJira>
    {
        public bool Equals(RecentJira x, RecentJira y)
        {
            if (x == null && y != null) return false;
            if (y == null && x != null) return false;

            return x?.JiraReference == y?.JiraReference;
        }

        public int GetHashCode(RecentJira x)
        {
            return x.GetHashCode();
        }
    }
}
