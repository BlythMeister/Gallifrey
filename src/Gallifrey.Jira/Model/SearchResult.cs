using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SearchResult
    {
        public List<Issue> issues { get; set; }
        public int maxResults { get; set; }
        public int total { get; set; }
    }
}