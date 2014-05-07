using System.Collections.Generic;
using System.Linq;
using Gallifrey.Exceptions.IdleTimers;
using Gallifrey.Serialization;

namespace Gallifrey.IdleTimers
{
    public class IdleTimerCollection
    {
        private readonly List<IdleTimer> lockTimerList;

        internal IdleTimerCollection()
        {
            lockTimerList = IdleTimerCollectionSerializer.DeSerialize();
        }

        internal void SaveTimers()
        {
            IdleTimerCollectionSerializer.Serialize(lockTimerList);
        }

        internal void NewLockTimer()
        {
            lockTimerList.Add(new IdleTimer());
        }

        internal void StopLockedTimers()
        {
            if (!lockTimerList.Any(x => x.isRunning))
            {
                throw new NoIdleTimerRunningException("Cannot find any idle timers running!");
            }

            foreach (var lockTimer in lockTimerList.Where(timer => timer.isRunning))
            {
                lockTimer.StopTimer();
            }
        }
    }
}
