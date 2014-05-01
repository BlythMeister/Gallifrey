using System;

namespace Gallifrey.Exceptions.IntergrationPoints
{
    public class StateChangedException : Exception
    {
        public StateChangedException(string message)
            : base(message)
        {

        }

        public StateChangedException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
