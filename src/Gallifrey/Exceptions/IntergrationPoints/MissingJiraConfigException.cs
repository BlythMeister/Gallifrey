using System;

namespace Gallifrey.Exceptions.IntergrationPoints
{
    public class MissingJiraConfigException : Exception
    {
        public MissingJiraConfigException(string message)
            : base(message)
        {

        }
    }
}
