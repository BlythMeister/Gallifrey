using Gallifrey.ExtensionMethods;
using Gallifrey.IdleTimers;
using System;

namespace Gallifrey.UI.Modern.Models
{
    public class LockedTimerModel
    {
        public Guid UniqueId { get; }
        public string Date { get; }
        public string IdleTimeValue { get; }
        public TimeSpan IdleTime { get; }
        public bool IsSelected { get; set; }
        public DateTime DateAndTimeForTimer { get; }
        public DateTime DateForTimer => DateAndTimeForTimer.Date;

        public LockedTimerModel(IdleTimer idleTimer)
        {
            DateAndTimeForTimer = idleTimer.DateStarted;
            Date = $"{idleTimer.DateStarted:ddd, dd MMM} at {idleTimer.DateStarted:t}";
            UniqueId = idleTimer.UniqueId;
            IdleTimeValue = idleTimer.IdleTimeValue.FormatAsString(false);
            IdleTime = idleTimer.IdleTimeValue;
            IsSelected = false;
        }
    }
}
