using Atlassian.Jira;
using Gallifrey.Settings;

namespace Gallifrey.IntegrationPoints
{
    public class JiraConnection
    {
        private readonly AppSettings appSettings;
        private readonly Jira jira;

        public JiraConnection(AppSettings appSettings)
        {
            this.appSettings = appSettings;
            jira = new Jira(appSettings.JiraUrl, appSettings.JiraUsername, appSettings.JiraPassword);
        }
    }
}
