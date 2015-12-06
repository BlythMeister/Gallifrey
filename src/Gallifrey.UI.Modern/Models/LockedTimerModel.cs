using System;
using Gallifrey.ExtensionMethods;
using Gallifrey.IdleTimers;

namespace Gallifrey.UI.Modern.Models
{
    public class LockedTimerModel
    {

        public Guid UniqueId { get; private set; }
        public string Date { get; private set; }
        public string IdleTimeValue { get; private set; }
        public TimeSpan IdleTime { get; private set; }
        public bool IsSelected { get; set; }
        public DateTime DateAndTimeForTimer { get; private set; }
        public DateTime DateForTimer { get { return DateAndTimeForTimer.Date; } }

        public LockedTimerModel(IdleTimer idleTimer)
        {
            DateAndTimeForTimer = idleTimer.DateStarted;
            Date = string.Format("{0} at {1}", idleTimer.DateStarted.ToString("ddd, dd MMM"), idleTimer.DateStarted.ToString("t"));
            UniqueId = idleTimer.UniqueId;
            IdleTimeValue = idleTimer.IdleTimeValue.FormatAsString();
            IdleTime = idleTimer.IdleTimeValue;
            IsSelected = false;
        }
    }
}