using System;

namespace Gallifrey.Exceptions.IdleTimers
{
    public class NoIdleTimerRunningException : Exception
    {
        public NoIdleTimerRunningException(string message)
            : base(message)
        {

        }
    }
}
