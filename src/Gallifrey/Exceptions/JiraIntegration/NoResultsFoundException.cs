using System;

namespace Gallifrey.Exceptions.JiraIntegration
{
    public class NoResultsFoundException : Exception
    {
        public NoResultsFoundException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
