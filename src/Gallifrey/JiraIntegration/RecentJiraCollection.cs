using System;
using System.Collections.Generic;
using System.Linq;
using Gallifrey.Comparers;
using Gallifrey.Jira.Model;
using Gallifrey.JiraTimers;
using Gallifrey.Serialization;

namespace Gallifrey.JiraIntegration
{
    public interface IRecentJiraCollection
    {
        IEnumerable<RecentJira> GetRecentJiraCollection();
        void AddRecentJira(Issue jiraIssue);
        void RemoveExpiredCache();
        void Remove(string jiraReference);
    }

    public class RecentJiraCollection : IRecentJiraCollection
    {
        private readonly List<RecentJira> recentJiraList;

        internal RecentJiraCollection()
        {
            recentJiraList = RecentJiraCollectionSerializer.DeSerialize();
        }
        
        internal void SaveCache()
        {
            RecentJiraCollectionSerializer.Serialize(recentJiraList);
        }

        public IEnumerable<RecentJira> GetRecentJiraCollection()
        {
            return recentJiraList.OrderBy(x=>x.JiraReference, new JiraReferenceComparer());
        }

        public void AddRecentJira(Issue jiraIssue)
        {
            string jiraParentReference = string.Empty, jiraParentName = string.Empty;
            var jiraReference = jiraIssue.key;
            var jiraProjectName = jiraIssue.fields.project.key;
            var jiraName = jiraIssue.fields.summary;

            if (jiraIssue.fields.parent != null)
            {
                jiraParentReference = jiraIssue.fields.parent.key;
                jiraParentName = jiraIssue.fields.parent.fields.summary;
            }

            if (recentJiraList.Any(x => x.JiraReference == jiraReference))
            {
                var recentJira = recentJiraList.First(x => x.JiraReference == jiraReference);
                recentJira.UpdateDetail(jiraProjectName, jiraName, jiraParentReference, jiraParentName, DateTime.Now);
            }
            else
            {
                recentJiraList.Add(new RecentJira(jiraReference, jiraProjectName, jiraName, jiraParentReference, jiraParentName, DateTime.Now));
            }
            SaveCache();
        }

        public void RemoveExpiredCache()
        {
            recentJiraList.RemoveAll(recentJira => recentJira.DateSeen < DateTime.Now.AddDays(-28));
            SaveCache();
        }

        public void Remove(string jiraReference)
        {
            recentJiraList.RemoveAll(recentJira => recentJira.JiraReference == jiraReference);
            SaveCache();
        }
    }

}
