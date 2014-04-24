using System.Collections.Generic;
using Atlassian.Jira;

namespace Gallifrey.JiraIntegration
{
    public class GetIssueResult
    {
        public bool Result { get; private set; }
        public IEnumerable<Issue> Issues { get; private set; }
        public string ErrorMessage { get; private set; }

        public GetIssueResult(bool result, IEnumerable<Issue> issues, string errorMessage)
        {
            Result = result;
            Issues = issues;
            ErrorMessage = errorMessage;
        }
    }
}
