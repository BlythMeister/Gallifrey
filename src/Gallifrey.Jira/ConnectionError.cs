using System;

namespace Gallifrey.Jira
{
    public class ConnectionError : Exception
    {
        public Type ConnectionType { get; }

        public enum Type
        {
            Jira,
            Tempo
        }

        public ConnectionError(Type connectionType)
        {
            ConnectionType = connectionType;
        }

        public ConnectionError(Type connectionType, string message) : base(message)
        {
            ConnectionType = connectionType;
        }

        public ConnectionError(Type connectionType, string message, Exception innerException) : base(message, innerException)
        {
            ConnectionType = connectionType;
        }
    }
}
