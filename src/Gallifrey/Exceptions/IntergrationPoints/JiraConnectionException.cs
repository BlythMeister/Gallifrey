using System;

namespace Gallifrey.Exceptions.IntergrationPoints
{
    public class JiraConnectionException : Exception
    {
        public JiraConnectionException(string message)
            : base(message)
        {

        }

        public JiraConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
