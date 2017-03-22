using System;
using Gallifrey.ExtensionMethods;
using Newtonsoft.Json;

namespace Gallifrey.IdleTimers
{
    public class IdleTimer
    {
        public DateTime DateStarted { get; }
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
                SetIdleTimeValueFromDates();
            }
            UniqueId = uniqueId;
        }

        public IdleTimer(TimeSpan initalTimeSpan)
        {
            DateStarted = DateTime.Now.Subtract(initalTimeSpan);
            DateFinished = null;
            IdleTimeValue = initalTimeSpan;
            UniqueId = Guid.NewGuid();
        }

        public void StopTimer()
        {
            SetIdleTimeValueFromDates();
        }

        private void SetIdleTimeValueFromDates()
        {
            if (!DateFinished.HasValue)
            {
                DateFinished = DateTime.Now;
            }

            IdleTimeValue = DateFinished.Value.Subtract(DateStarted);
            if (IdleTimeValue.Seconds > 30)
            {
                IdleTimeValue = IdleTimeValue.Add(new TimeSpan(0, 1, 0));
            }

            IdleTimeValue = new TimeSpan(IdleTimeValue.Hours, IdleTimeValue.Minutes, 0);
        }
    }
}
