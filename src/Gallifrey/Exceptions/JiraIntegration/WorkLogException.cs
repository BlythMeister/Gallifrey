using System;

namespace Gallifrey.Exceptions.JiraIntegration
{
    public class WorkLogException : Exception
    {
        public WorkLogException(string message)
            : base(message)
        {
        }

        public WorkLogException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
