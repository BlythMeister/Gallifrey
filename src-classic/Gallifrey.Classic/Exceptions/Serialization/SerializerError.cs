using System;

namespace Gallifrey.Exceptions.Serialization
{
    class SerializerError : Exception
    {
        public SerializerError(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SerializerError(string message) 
            : base(message)
        {
        }
    }
}
