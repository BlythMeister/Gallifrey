using Gallifrey.IdleTimers;
using System.Collections.Generic;

namespace Gallifrey.Comparers
{
    public class DuplicateIdleTimerComparer : IEqualityComparer<IdleTimer>
    {
        public bool Equals(IdleTimer x, IdleTimer y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null)
            {
                return false;
            }

            if (y == null)
            {
                return false;
            }

            return x.UniqueId == y.UniqueId;
        }

        public int GetHashCode(IdleTimer x)
        {
            return x.UniqueId.GetHashCode();
        }
    }
}
