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
        IdleTimer GetTimer(Guid uniqueId);
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
            SaveTimers();
        }

        internal Guid StopLockedTimers()
        {
            if (!lockTimerList.Any(x => x.IsRunning))
            {
                throw new NoIdleTimerRunningException("Cannot find any idle timers running!");
            }

            var lastStop = Guid.NewGuid();//Should never return this, as we know there is something running.
            foreach (var lockTimer in lockTimerList.Where(timer => timer.IsRunning))
            {
                lockTimer.StopTimer();
                lastStop = lockTimer.UniqueId;
            }

            SaveTimers();

            return lastStop;
        }

        public void RemoveTimer(Guid uniqueId)
        {
            lockTimerList.Remove(GetTimer(uniqueId));
            SaveTimers();
        }

        public IEnumerable<IdleTimer> GetUnusedLockTimers()
        {
            return lockTimerList.Where(timer => timer.IsRunning == false).OrderByDescending(timer=>timer.DateFinished);
        }

        public void RemoveOldTimers()
        {
            foreach (var idleTimer in lockTimerList.Where(idleTimer => idleTimer.DateStarted >= DateTime.Now.AddDays(-1)))
            {
                lockTimerList.Remove(idleTimer);
            }
            SaveTimers();
        }

        public IdleTimer GetTimer(Guid uniqueId)
        {
            return lockTimerList.First(timer => timer.UniqueId == uniqueId);
        }
    }
}
