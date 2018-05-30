using System;

namespace Gallifrey.Exceptions.JiraIntegration
{
    public class JiraConnectionException : Exception
    {
        public JiraConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
