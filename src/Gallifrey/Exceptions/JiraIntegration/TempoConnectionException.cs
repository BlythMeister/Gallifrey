using System;

namespace Gallifrey.Exceptions.JiraIntegration
{
    public class TempoConnectionException : Exception
    {
        public TempoConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}