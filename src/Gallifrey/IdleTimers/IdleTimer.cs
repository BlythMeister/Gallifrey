using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Gallifrey.IdleTimers
{
    public class IdleTimer
    {
        public DateTime DateStarted { get; private set; }
        public DateTime? DateFinished { get; private set; }
        public TimeSpan CurrentTime { get; private set; }
        public Guid UniqueId { get; private set; }
        public bool IsRunning { get; private set; }
        private readonly Stopwatch currentRunningTime;

        [JsonConstructor]
        public IdleTimer(DateTime dateStarted, DateTime? dateFinished, TimeSpan currentTime, Guid uniqueId)
        {
            DateStarted = dateStarted;
            DateFinished = dateFinished;
            CurrentTime = currentTime;
            UniqueId = uniqueId;
            currentRunningTime = new Stopwatch();
            IsRunning = false;
        }

        public IdleTimer()
        {
            DateStarted = DateTime.Now;
            DateFinished = null;
            CurrentTime = new TimeSpan();
            UniqueId = Guid.NewGuid();
            currentRunningTime = new Stopwatch();
            currentRunningTime.Start();
            IsRunning = true;
        }

        public TimeSpan ExactCurrentTime
        {
            get { return CurrentTime.Add(currentRunningTime.Elapsed); }
        }

       public void StopTimer()
        {
            currentRunningTime.Stop();
            CurrentTime = CurrentTime.Add(currentRunningTime.Elapsed);
            currentRunningTime.Reset();
            IsRunning = false;
        }
    }
}
