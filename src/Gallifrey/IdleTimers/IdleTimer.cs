using System;
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
        public bool IsRunning => !DateFinished.HasValue;

        [JsonConstructor]
        public IdleTimer(DateTime dateStarted, DateTime? dateFinished, TimeSpan idleTimeValue, Guid uniqueId)
        {
            DateStarted = dateStarted;
            DateFinished = dateFinished;
            IdleTimeValue = idleTimeValue;
            if (IdleTimeValue.TotalSeconds == 0)
            {
                IdleTimeValue = DateFinished.Value.Subtract(DateStarted);
            }
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
                $"Date - {DateStarted.ToString("ddd, dd MMM")} - From [ {DateStarted.ToString("HH:mm:ss")} ] To [ {DateFinished.Value.ToString("HH:mm:ss")} ] - Time [ {IdleTimeValue.FormatAsString()} ]" :
                $"Date - {DateStarted.ToString("ddd, dd MMM")} - From [ {DateStarted.ToString("HH:mm:ss")} ] To [ IN PROGRESS ]";
        }
    }
}
