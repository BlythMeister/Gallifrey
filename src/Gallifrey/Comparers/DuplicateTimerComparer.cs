using Gallifrey.JiraTimers;
using System.Collections.Generic;

namespace Gallifrey.Comparers
{
    public class DuplicateTimerComparer : IEqualityComparer<JiraTimer>
    {
        public bool Equals(JiraTimer x, JiraTimer y)
        {
            if (x == null && y != null) return false;
            if (y == null && x != null) return false;

            return x?.UniqueId == y?.UniqueId;
        }

        public int GetHashCode(JiraTimer x)
        {
            return x.GetHashCode();
        }
    }
}