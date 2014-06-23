using System;
using System.Diagnostics;
using Gallifrey.ExtensionMethods;
using Newtonsoft.Json;

namespace Gallifrey.IdleTimers
{
    public class IdleTimer
    {
        public DateTime DateStarted { get; private set; }
        public DateTime? DateFinished { get; private set; }
        public TimeSpan IdleTimeValue { get; private set; }
        public Guid UniqueId { get; private set; }
        public bool IsRunning { get { return !DateFinished.HasValue; } }

        [JsonConstructor]
        public IdleTimer(DateTime dateStarted, DateTime? dateFinished, TimeSpan currentTime, Guid uniqueId)
        {
            DateStarted = dateStarted;
            DateFinished = dateFinished;
            IdleTimeValue = currentTime;
            UniqueId = uniqueId;
        }

        public IdleTimer()
        {
            DateStarted = DateTime.Now;
            DateFinished = null;
            IdleTimeValue = new TimeSpan();
            UniqueId = Guid.NewGuid();
        }

        public void StopTimer()
        {
            DateFinished = DateTime.Now;
            IdleTimeValue = DateFinished.Value.Subtract(DateStarted);
        }

        public override string ToString()
        {
            return DateFinished.HasValue ?
                string.Format("Date - {0} - From [ {1} ] To [ {2} ] - Time [ {3} ]", DateStarted.ToString("ddd, dd MMM"), DateStarted.ToString("HH:mm:ss"), DateFinished.Value.ToString("HH:mm:ss"), IdleTimeValue.FormatAsString()) :
                string.Format("Date - {0} - From [ {1} ] To [ IN PROGRESS ]", DateStarted.ToString("ddd, dd MMM"), DateStarted.ToString("HH:mm:ss"));
        }
    }
}
