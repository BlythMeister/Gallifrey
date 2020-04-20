using Gallifrey.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gallifrey.IdleTimers
{
    public interface IIdleTimerCollection
    {
        void RemoveTimer(Guid uniqueId);

        IEnumerable<IdleTimer> GetUnusedIdleTimers();

        IdleTimer GetTimer(Guid uniqueId);
    }

    public class IdleTimerCollection : IIdleTimerCollection
    {
        private readonly List<IdleTimer> idleTimerList;
        private bool isIntialised;

        internal IdleTimerCollection()
        {
            idleTimerList = new List<IdleTimer>();
            isIntialised = false;
        }

        internal void Initialise()
        {
            idleTimerList.AddRange(IdleTimerCollectionSerializer.DeSerialize());
            idleTimerList.RemoveAll(x => x.IsRunning);
            isIntialised = true;
        }

        internal void SaveTimers()
        {
            if (!isIntialised) return;
            IdleTimerCollectionSerializer.Serialize(idleTimerList);
        }

        internal void NewIdleTimer(TimeSpan initalTimeSpan)
        {
            //Only allow 1 running timer at a time
            if (!idleTimerList.Any(x => x.IsRunning))
            {
                idleTimerList.Add(new IdleTimer(initalTimeSpan));
            }

            SaveTimers();
        }

        internal Guid? StopIdleTimers()
        {
            var lockedTimer = idleTimerList.FirstOrDefault(x => x.IsRunning);

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
                idleTimerList.Remove(timerToRemove);
                SaveTimers();
            }
        }

        public IEnumerable<IdleTimer> GetUnusedIdleTimers()
        {
            return idleTimerList.Where(timer => timer.IsRunning == false).OrderByDescending(timer => timer.DateFinished);
        }

        public void RemoveOldTimers()
        {
            idleTimerList.RemoveAll(idleTimer => !idleTimer.IsRunning && idleTimer.DateStarted <= DateTime.Now.AddDays(-1));
            SaveTimers();
        }

        public IdleTimer GetTimer(Guid uniqueId)
        {
            return idleTimerList.FirstOrDefault(timer => timer.UniqueId == uniqueId);
        }
    }
}
