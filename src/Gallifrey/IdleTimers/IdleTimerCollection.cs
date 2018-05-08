using Gallifrey.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gallifrey.IdleTimers
{
    public interface IIdleTimerCollection
    {
        void RemoveTimer(Guid uniqueId);
        IEnumerable<IdleTimer> GetUnusedLockTimers();
        IdleTimer GetTimer(Guid uniqueId);
    }

    public class IdleTimerCollection : IIdleTimerCollection
    {
        private readonly List<IdleTimer> lockTimerList;

        internal IdleTimerCollection()
        {
            lockTimerList = IdleTimerCollectionSerializer.DeSerialize();
            lockTimerList.RemoveAll(x => x.IsRunning);
        }

        internal void SaveTimers()
        {
            IdleTimerCollectionSerializer.Serialize(lockTimerList);
        }

        internal void NewLockTimer(TimeSpan initalTimeSpan)
        {
            //Only allow 1 running timer at a time
            if (!lockTimerList.Any(x => x.IsRunning))
            {
                lockTimerList.Add(new IdleTimer(initalTimeSpan));
            }

            SaveTimers();
        }

        internal Guid? StopLockedTimers()
        {
            var lockedTimer = lockTimerList.FirstOrDefault(x => x.IsRunning);

            if (lockedTimer == null)
            {
                return null;
            }

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
            return lockTimerList.Where(timer => timer.IsRunning == false).OrderByDescending(timer => timer.DateFinished);
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
