namespace Gallifrey.Jira.Exception
{
    internal class JiraClientException : System.Exception
    {
        public JiraClientException(string message)
            : base(message)
        {

        }

        public JiraClientException(string message, System.Exception innerException)
            : base(message, innerException)
        {

        }
    }
}