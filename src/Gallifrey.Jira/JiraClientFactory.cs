using System.Security.Authentication;

namespace Gallifrey.Jira
{
    public static class JiraClientFactory
    {
        public static IJiraClient BuildJiraClient(string jiraUrl, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(jiraUrl) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidCredentialException("Required settings to create connection to jira are missing");
            }

            jiraUrl = jiraUrl.Replace("/secure/Dashboard.jspa", "");

            var jira = ConnectUsingRest(jiraUrl, username, password);
            if (jira == null)
            {
                jira = ConnectUsingSoap(jiraUrl, username, password);
                if (jira == null)
                {
                    throw new System.Exception("Unable to connect to jira");
                }
            }

            return jira;
        }

        private static IJiraClient ConnectUsingRest(string jiraUrl, string username, string password)
        {
            try
            {
                var jira = new JiraRestClient(jiraUrl, username, password);
                jira.GetCurrentUser();
                return jira;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private static IJiraClient ConnectUsingSoap(string jiraUrl, string username, string password)
        {
            try
            {
                var jira = new JiraSoapClient(jiraUrl, username, password);
                jira.GetCurrentUser();
                return jira;
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }
}
