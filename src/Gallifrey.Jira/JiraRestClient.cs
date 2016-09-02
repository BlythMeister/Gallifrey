using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Gallifrey.Jira.Enum;
using Gallifrey.Jira.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Gallifrey.Rest;
using Gallifrey.Rest.Exception;

namespace Gallifrey.Jira
{
    public class JiraRestClient : IJiraClient
    {
        private readonly ISimpleRestClient restClient;

        public JiraRestClient(string baseUrl, string username, string password)
        {
            var url = baseUrl + (baseUrl.EndsWith("/") ? "" : "/") + "rest/api/2/";
            restClient = new SimpleRestClient(url, username, password, GetErrorMessages);
        }
        
        public User GetCurrentUser()
        {
            return restClient.Get<User>(HttpStatusCode.OK, "myself");
        }

        public Issue GetIssue(string issueRef)
        {
            return restClient.Get<Issue>(HttpStatusCode.OK, $"issue/{issueRef}");
        }

        public Issue GetIssueWithWorklogs(string issueRef, string user)
        {
            var issue = GetIssue(issueRef);

            if (issue.fields.worklog == null)
            {
                issue.fields.worklog = new WorkLogs { worklogs = null };
            }

            if (issue.fields.worklog.worklogs == null || issue.fields.worklog.total > issue.fields.worklog.worklogs.Count)
            {
                var worklogs = restClient.Get(HttpStatusCode.OK, $"issue/{issueRef}/worklog", customDeserialize: s => FilterWorklogsToUser(s, user));

                issue.fields.worklog.worklogs = worklogs.worklogs;
                issue.fields.worklog.total = worklogs.total;
                issue.fields.worklog.maxResults = worklogs.maxResults;
            }

            return issue;
        }

        public string GetJqlForFilter(string filterName)
        {
            var filters = restClient.Get<List<Filter>>(HttpStatusCode.OK, "filter/favourite");

            var selectedFilter = filters.FirstOrDefault(f => f.name == filterName);

            if (selectedFilter != null)
            {
                return selectedFilter.jql;
            }

            return string.Empty;
        }

        public IEnumerable<Issue> GetIssuesFromFilter(string filterName)
        {
            var jql = GetJqlForFilter(filterName);

            if (string.IsNullOrWhiteSpace(jql))
            {
                return new List<Issue>();
            }

            return GetIssuesFromJql(jql);
        }

        public IEnumerable<Issue> GetIssuesFromJql(string jql)
        {
            var returnIssues = new List<Issue>();

            if (string.IsNullOrWhiteSpace(jql))
            {
                return returnIssues;
            }

            var moreToGet = true;
            var startAt = 0;

            while (moreToGet)
            {
                var searchResult = restClient.Get<SearchResult>(HttpStatusCode.OK, $"search?jql={jql}&maxResults=999&startAt={startAt}&fields=summary,project,parent");

                returnIssues.AddRange(searchResult.issues);

                if (searchResult.total > returnIssues.Count)
                {
                    startAt = startAt + searchResult.maxResults;
                }
                else
                {
                    moreToGet = false;
                }
            }

            return returnIssues;
        }

        public IEnumerable<Project> GetProjects()
        {
            return restClient.Get<List<Project>>(HttpStatusCode.OK, "project");
        }

        public IEnumerable<Filter> GetFilters()
        {
            return restClient.Get<List<Filter>>(HttpStatusCode.OK, "filter/favourite");
        }

        public Transitions GetIssueTransitions(string issueRef)
        {
            return restClient.Get<Transitions>(HttpStatusCode.OK, $"issue/{issueRef}/transitions?expand=transitions.fields");
        }

        public void TransitionIssue(string issueRef, string transitionName)
        {
            if (transitionName == null) throw new ArgumentNullException(nameof(transitionName));
            var transitions = GetIssueTransitions(issueRef);
            var transition = transitions.transitions.FirstOrDefault(t => t.name == transitionName);

            if (transition == null)
            {
                throw new ClientException($"Unable to locate transition '{transitionName}'");
            }

            var postData = new Dictionary<string, object>
            {
                { "transition", new { id = transition.id } }
            };

            restClient.Post(HttpStatusCode.NoContent, $"issue/{issueRef}/transitions", postData);
        }

        public void AddWorkLog(string issueRef, WorkLogStrategy workLogStrategy, string comment, TimeSpan timeSpent, DateTime logDate, TimeSpan? remainingTime = null)
        {
            if (logDate.Kind != DateTimeKind.Local) logDate = DateTime.SpecifyKind(logDate, DateTimeKind.Local);

            var postData = new Dictionary<string, object>
            {
                { "started", $"{logDate.ToString("yyyy-MM-ddTHH:mm:ss.fff")}{logDate.ToString("zzz").Replace(":", "")}"},
                { "comment", comment },
                { "timeSpent", $"{timeSpent.Hours}h {timeSpent.Minutes}m"},
            };
            var adjustmentMethod = string.Empty;
            var newEstimate = string.Empty;

            if (remainingTime.HasValue)
            {
                newEstimate = $"{remainingTime.Value.Hours}h {remainingTime.Value.Minutes}m";
            }

            switch (workLogStrategy)
            {
                case WorkLogStrategy.Automatic:
                    adjustmentMethod = "auto";
                    break;
                case WorkLogStrategy.LeaveRemaining:
                    adjustmentMethod = "leave";
                    break;
                case WorkLogStrategy.SetValue:
                    adjustmentMethod = "new";
                    break;
            }

            restClient.Post(HttpStatusCode.Created, $"issue/{issueRef}/worklog?adjustEstimate={adjustmentMethod}&newEstimate={newEstimate}&reduceBy=", postData);
        }

        public void AssignIssue(string issueRef, string userName)
        {
            var postData = new Dictionary<string, object>
            {
                { "name", userName }
            };

            restClient.Put(HttpStatusCode.NoContent, $"issue/{issueRef}/assignee", postData);
        }

        public void AddComment(string issueRef, string comment)
        {
            var postData = new Dictionary<string, object>
            {
                { "body", comment }
            };

            restClient.Post(HttpStatusCode.Created, $"issue/{issueRef}/comment", postData);
        }

        private static WorkLogs FilterWorklogsToUser(string rawJson, string user)
        {
            var jsonObject = JObject.Parse(rawJson);
            var filtered = jsonObject["worklogs"].Children().Where(x => ((string)x["author"]["name"]).ToLower() == user.ToLower());
            jsonObject["worklogs"] = new JArray(filtered);
            return jsonObject.ToObject<WorkLogs>();
        }

        private static List<string> GetErrorMessages(string jsonString)
        {
            var errors = JsonConvert.DeserializeObject<Error>(jsonString);
            if (errors.errorMessages == null)
            {
                return new List<string>();
            }
            else
            {
                return errors.errorMessages;
            }
        }
    }
}
