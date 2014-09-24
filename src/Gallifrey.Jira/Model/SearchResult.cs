using System.Collections.Generic;

namespace Gallifrey.Jira
{
    public class SearchResult
    {
        public List<Issue> issues { get; set; }
        public int startAt { get; set; }
        public int maxResults { get; set; }
        public int total { get; set; }
    }
}