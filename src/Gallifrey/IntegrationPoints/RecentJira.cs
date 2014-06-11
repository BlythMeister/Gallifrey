using System;
using Newtonsoft.Json;

namespace Gallifrey.IntegrationPoints
{
    public class RecentJira
    {
        public string JiraReference { get; private set; }
        public string JiraProjectName { get; private set; }
        public string JiraName { get; private set; }
        public DateTime DateSeen { get; private set; }

        [JsonConstructor]
        public RecentJira(string jiraReference, string jiraProjectName, string jiraName, DateTime dateSeen)
        {
            JiraReference = jiraReference;
            JiraProjectName = jiraProjectName;
            JiraName = jiraName;
            DateSeen = dateSeen;
        }

        public RecentJira(string jiraReference, string jiraProjectName, string jiraName)
        {
            JiraReference = jiraReference;
            JiraProjectName = jiraProjectName;
            JiraName = jiraName;
        }

        public void UpdateDetail(string jiraProjectName, string jiraName, DateTime dateSeen)
        {
            JiraProjectName = jiraProjectName;
            JiraName = jiraName;
            DateSeen = dateSeen;
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(JiraReference))
            {
                return string.Format("{0} - Desc [ {1} ]", JiraReference, JiraName);    
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
