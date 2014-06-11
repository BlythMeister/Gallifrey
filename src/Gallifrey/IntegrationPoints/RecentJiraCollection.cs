using System;
using System.Collections.Generic;
using System.Linq;
using Gallifrey.Serialization;

namespace Gallifrey.IntegrationPoints
{
    public interface IRecentJiraCollection
    {
        IEnumerable<RecentJira> GetRecentJiraCollection();
        void AddRecentJira(string jiraReference, string jiraProjectName, string jiraName);
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
            return recentJiraList;
        }

        public void AddRecentJira(string jiraReference, string jiraProjectName, string jiraName)
        {
            if (recentJiraList.Any(x => x.JiraReference == jiraReference))
            {
                var recentJira = recentJiraList.First(x => x.JiraReference == jiraReference);
                recentJira.UpdateDetail(jiraProjectName, jiraName, DateTime.Now);
            }
            else
            {
                recentJiraList.Add(new RecentJira(jiraReference, jiraProjectName, jiraName, DateTime.Now));
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
