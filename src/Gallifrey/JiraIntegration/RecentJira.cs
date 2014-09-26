using System;
using Newtonsoft.Json;

namespace Gallifrey.JiraIntegration
{
    public class RecentJira
    {
        public string JiraReference { get; private set; }
        public string JiraProjectName { get; private set; }
        public string JiraName { get; private set; }
        public string JiraParentReference { get; private set; }
        public string JiraParentName { get; private set; }
        public DateTime DateSeen { get; private set; }

        [JsonConstructor]
        public RecentJira(string jiraReference, string jiraProjectName, string jiraName, string jiraParentReference, string jiraParentName, DateTime dateSeen)
        {
            JiraReference = jiraReference;
            JiraProjectName = jiraProjectName;
            JiraName = jiraName;
            JiraParentReference = jiraParentReference;
            JiraParentName = jiraParentName;
            DateSeen = dateSeen;
        }

        public RecentJira(string jiraReference, string jiraProjectName, string jiraName, string jiraParentReference, string jiraParentName)
        {
            JiraReference = jiraReference;
            JiraProjectName = jiraProjectName;
            JiraName = jiraName;
            JiraParentReference = jiraParentReference;
            JiraParentName = jiraParentName;
        }

        public void UpdateDetail(string jiraProjectName, string jiraName, string jiraParentReference, string jiraParentName, DateTime dateSeen)
        {
            JiraProjectName = jiraProjectName;
            JiraName = jiraName;
            JiraParentReference = jiraParentReference;
            JiraParentName = jiraParentName;
            DateSeen = dateSeen;
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(JiraParentReference))
            {
                return string.Format("{0} ({1})", JiraReference, JiraName);    
            }
            else
            {
                return string.Format("{0} ({1}) - Parent [ {2} - {3} ]", JiraReference, JiraName, JiraParentReference, JiraParentName);
            }
        }
    }
}
