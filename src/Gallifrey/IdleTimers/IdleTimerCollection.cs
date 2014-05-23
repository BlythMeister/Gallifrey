using System;
using System.Collections.Generic;
using System.Linq;
using Gallifrey.Exceptions.IdleTimers;
using Gallifrey.Serialization;

namespace Gallifrey.IdleTimers
{
    public interface IIdleTimerCollection
    {
        void RemoveTimer(Guid uniqueId);
        IEnumerable<IdleTimer> GetUnusedLockTimers();
        void RemoveOldTimers();
    }

    public class IdleTimerCollection : IIdleTimerCollection
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
            if (!lockTimerList.Any(x => x.IsRunning))
            {
                throw new NoIdleTimerRunningException("Cannot find any idle timers running!");
            }

            foreach (var lockTimer in lockTimerList.Where(timer => timer.IsRunning))
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
            return lockTimerList.Where(timer => timer.IsRunning == false);
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
