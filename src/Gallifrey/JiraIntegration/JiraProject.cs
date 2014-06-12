namespace Gallifrey.JiraIntegration
{
    public class JiraProject
    {
        public string JiraProjectCode { get; private set; }
        public string JiraProjectName { get; private set; }

        public JiraProject(string jiraProjectCode, string jiraProjectName)
        {
            JiraProjectCode = jiraProjectCode;
            JiraProjectName = jiraProjectName;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", JiraProjectCode, JiraProjectName);
        }
    }
}
