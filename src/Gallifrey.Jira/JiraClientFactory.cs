using System.Security.Authentication;

namespace Gallifrey.Jira
{
    public static class JiraClientFactory
    {
        public static IJiraClient BuildJiraClient(string jiraUrl, string username, string password, bool useTempo, string tempoToken)
        {
            if (string.IsNullOrWhiteSpace(jiraUrl) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidCredentialException("Required settings to create connection to jira are missing");
            }

            if (useTempo && string.IsNullOrWhiteSpace(tempoToken))
            {
                throw new InvalidCredentialException("Required settings to create connection to tempo are missing");
            }

            jiraUrl = jiraUrl.Replace("/secure/Dashboard.jspa", "");

            try
            {
                return new JiraRestClient(jiraUrl, username, password, useTempo, tempoToken);
            }
            catch (System.Exception e)
            {
                throw new System.Exception("Unable to connect to jira", e);
            }
        }
    }
}
