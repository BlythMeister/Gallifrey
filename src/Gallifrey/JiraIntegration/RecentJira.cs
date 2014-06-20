using System;
using Newtonsoft.Json;

namespace Gallifrey.JiraIntegration
{
    public class RecentJira : IEquatable<RecentJira>
    {
        public JiraInfo JiraInfo { get; private set; }
        public DateTime DateSeen { get; private set; }

        [JsonConstructor]
        public RecentJira(JiraInfo jiraInfo, DateTime dateSeen)
        {
            JiraInfo = jiraInfo;
            DateSeen = dateSeen;
        }

        public RecentJira(JiraInfo jiraInfo)
        {
            JiraInfo = jiraInfo;
        }

        public void UpdateDetail(string jiraProjectName, string jiraName, DateTime dateSeen)
        {
            JiraInfo.UpdateDetail(jiraProjectName, jiraName);
            DateSeen = dateSeen;
        }

        public bool Equals(RecentJira other)
        {
            return JiraInfo.Equals(other.JiraInfo);
        }

        public override string ToString()
        {
            return JiraInfo.ToString();
        }
    }
}
