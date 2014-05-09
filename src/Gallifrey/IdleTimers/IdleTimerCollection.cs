using System;
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

        public void RemoveTimer(Guid uniqueId)
        {
            lockTimerList.Remove(lockTimerList.First(timer => timer.UniqueId == uniqueId));
        }

        public IEnumerable<IdleTimer> GetUnusedLockTimers()
        {
            return lockTimerList.Where(timer => timer.isRunning == false);
        }

        public void RemoveOldTimers()
        {
            foreach (var idleTimer in lockTimerList.Where(idleTimer => idleTimer.DateStarted >= DateTime.Now.AddDays(-1)))
            {
                lockTimerList.Remove(idleTimer);
            }
        }
    }
}
