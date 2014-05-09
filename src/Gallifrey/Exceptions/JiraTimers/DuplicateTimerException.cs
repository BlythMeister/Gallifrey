using System;

namespace Gallifrey.Exceptions.JiraTimers
{
    public class DuplicateTimerException : Exception
    {
        public DuplicateTimerException(string message)
            : base(message)
        {

        }
    }
}
