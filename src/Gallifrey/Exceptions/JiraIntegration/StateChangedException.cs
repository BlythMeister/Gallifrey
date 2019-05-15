using System;

namespace Gallifrey.Exceptions.JiraIntegration
{
    public class StateChangedException : Exception
    {
        public StateChangedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
