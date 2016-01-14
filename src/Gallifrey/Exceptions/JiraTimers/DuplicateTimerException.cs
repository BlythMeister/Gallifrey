using System;

namespace Gallifrey.Exceptions.JiraTimers
{
    public class DuplicateTimerException : Exception
    {
        public Guid TimerId { get; set; }

        public DuplicateTimerException(string message, Guid timerId)
            : base(message)
        {
            TimerId = timerId;
        }
    }
}
