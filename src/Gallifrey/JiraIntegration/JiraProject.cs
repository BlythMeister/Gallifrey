namespace Gallifrey.JiraIntegration
{
    public class JiraProject
    {
        public string JiraProjectCode { get; }
        public string JiraProjectName { get; }

        public JiraProject(string jiraProjectCode, string jiraProjectName)
        {
            JiraProjectCode = jiraProjectCode;
            JiraProjectName = jiraProjectName;
        }
    }
}
