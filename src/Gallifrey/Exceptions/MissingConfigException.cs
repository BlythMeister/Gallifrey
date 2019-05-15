using System;

namespace Gallifrey.Exceptions
{
    public class MissingConfigException : Exception
    {
        public MissingConfigException(string message)
            : base(message)
        {
        }
    }
}
