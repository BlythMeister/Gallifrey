using System.Collections.Generic;
using Atlassian.Jira;

namespace Gallifrey.JiraIntegration
{
    public class GetFilterResult
    {
        public bool Result { get; private set; }
        public IEnumerable<JiraNamedEntity> Filters { get; private set; }
        public string ErrorMessage { get; private set; }

        public GetFilterResult(bool result, IEnumerable<JiraNamedEntity> filters, string errorMessage)
        {
            Result = result;
            Filters = filters;
            ErrorMessage = errorMessage;
        }
    }
}
