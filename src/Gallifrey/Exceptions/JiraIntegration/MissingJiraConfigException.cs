using System;

namespace Gallifrey.Exceptions.JiraIntegration
{
    public class MissingJiraConfigException : Exception
    {
        public MissingJiraConfigException(string message)
            : base(message)
        {
        }
    }
}
