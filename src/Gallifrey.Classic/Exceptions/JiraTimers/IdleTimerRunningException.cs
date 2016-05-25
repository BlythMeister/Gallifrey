using System;

namespace Gallifrey.Exceptions.JiraTimers
{
    public class IdleTimerRunningException : Exception
    {
        public IdleTimerRunningException(string message)
            : base(message)
        {

        }
    }
}
