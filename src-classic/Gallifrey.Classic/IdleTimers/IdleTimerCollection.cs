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

        internal bool NewLockTimer()
        {
            var addedTimer = false;
            if (!lockTimerList.Any(x => x.IsRunning))
            {
                lockTimerList.Add(new IdleTimer());
                addedTimer = true;
            }
            
            SaveTimers();
            return addedTimer;
        }

        internal Guid StopLockedTimers()
        {
            if (!lockTimerList.Any(x => x.IsRunning))
            {
                throw new NoIdleTimerRunningException("Cannot find any idle timers running!");
            }

            var lockedTimer = lockTimerList.First(timer => timer.IsRunning);
            lockedTimer.StopTimer();
            
            SaveTimers();

            return lockedTimer.UniqueId;
        }

        public void RemoveTimer(Guid uniqueId)
        {
            var timerToRemove = GetTimer(uniqueId);
            if (timerToRemove != null)
            {
                lockTimerList.Remove(timerToRemove);
                SaveTimers();
            }
        }

        public IEnumerable<IdleTimer> GetUnusedLockTimers()
        {
            return lockTimerList.Where(timer => timer.IsRunning == false).OrderByDescending(timer=>timer.DateFinished);
        }

        public void RemoveOldTimers()
        {
            lockTimerList.RemoveAll(idleTimer => !idleTimer.IsRunning && idleTimer.DateStarted <= DateTime.Now.AddDays(-1));
            SaveTimers();
        }

        public IdleTimer GetTimer(Guid uniqueId)
        {
            return lockTimerList.FirstOrDefault(timer => timer.UniqueId == uniqueId);
        }
    }
}
