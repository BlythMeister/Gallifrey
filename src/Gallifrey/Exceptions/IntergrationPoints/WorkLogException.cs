using System;

namespace Gallifrey.Exceptions.IntergrationPoints
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
