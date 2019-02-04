using Gallifrey.IdleTimers;
using System.Collections.Generic;

namespace Gallifrey.Comparers
{
    public class DuplicateIdleTimerComparer : IEqualityComparer<IdleTimer>
    {
        public bool Equals(IdleTimer x, IdleTimer y)
        {
            if (x == null && y != null) return false;
            if (y == null && x != null) return false;

            return x?.UniqueId == y?.UniqueId;
        }

        public int GetHashCode(IdleTimer x)
        {
            return x.GetHashCode();
        }
    }
}
