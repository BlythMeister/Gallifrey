using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Gallifrey.Jira.Enum;
using Gallifrey.Jira.Exception;
using Gallifrey.Jira.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Gallifrey.Jira
{
    public class JiraRestClient : IJiraClient
    {
        private readonly string username;
        private readonly string password;
        private readonly RestClient client;

        public JiraRestClient(string baseUrl, string username, string password)
        {
            this.username = username;
            this.password = password;
            client = new RestClient { BaseUrl = new Uri(baseUrl + (baseUrl.EndsWith("/") ? "" : "/") + "rest/api/2/") };
        }

        private RestRequest CreateRequest(Method method, string path)
        {
            var request = new RestRequest { Method = method, Resource = path, RequestFormat = DataFormat.Json };
            request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
            return request;
        }

        private void AssertStatus(IRestResponse response, HttpStatusCode status)
        {
            if (response.ErrorException != null)
                throw new JiraClientException("Transport level error: " + response.ErrorMessage, response.ErrorException);
            if (response.StatusCode != status)
            {
                try
                {
                    var errorMessages = JsonConvert.DeserializeObject<Error>(response.Content);

                    if (errorMessages.errorMessages.Any())
                    {
                        throw new JiraClientException($"JIRA returned wrong status: {response.StatusDescription}. Message: {errorMessages.errorMessages[0]}");
                    }
                    else
                    {
                        throw new JiraClientException($"JIRA returned wrong status: {response.StatusDescription}");
                    }
                }
                catch (JiraClientException)
                {
                    throw;
                }
                catch (System.Exception ex)
                {
                    throw new JiraClientException($"JIRA returned wrong status: {response.StatusDescription}", ex);
                }
            }
        }

        private IRestResponse ExecuteRequest(Method method, HttpStatusCode expectedStatus, string path, Dictionary<string, object> data = null)
        {
            var request = CreateRequest(method, path);
            if (data != null)
            {
                request.AddHeader("ContentType", "application/json");
                request.AddBody(data);
            }

            var response = client.Execute(request);

            AssertStatus(response, expectedStatus);

            return response;
        }

        private T ExecuteRequest<T>(HttpStatusCode expectedStatus, string path, Dictionary<string, object> data = null, Func<string, T> customDeserialize = null) where T : class
        {
            if (customDeserialize == null)
            {
                customDeserialize = JsonConvert.DeserializeObject<T>;
            }

            var response = ExecuteRequest(Method.GET, expectedStatus, path, data);

            return customDeserialize(response.Content);
        }

        public User GetCurrentUser()
        {
            return ExecuteRequest<User>(HttpStatusCode.OK, "myself");
        }

        public Issue GetIssue(string issueRef)
        {
            return ExecuteRequest<Issue>(HttpStatusCode.OK, $"issue/{issueRef}");
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
                var worklogs = ExecuteRequest(HttpStatusCode.OK, $"issue/{issueRef}/worklog", customDeserialize: s => FilterWorklogsToUser(s, user));

                issue.fields.worklog.worklogs = worklogs.worklogs;
                issue.fields.worklog.total = worklogs.total;
                issue.fields.worklog.maxResults = worklogs.maxResults;
            }

            return issue;
        }

        private static WorkLogs FilterWorklogsToUser(string rawJson, string user)
        {
            var jsonObject = JObject.Parse(rawJson);
            var filtered = jsonObject["worklogs"].Children().Where(x => ((string)x["author"]["name"]).ToLower() == user.ToLower());
            jsonObject["worklogs"] = new JArray(filtered);
            return jsonObject.ToObject<WorkLogs>();
        }

        public IEnumerable<Issue> GetIssuesFromFilter(string filterName)
        {
            var filters = ExecuteRequest<List<Filter>>(HttpStatusCode.OK, "filter/favourite");

            var selectedFilter = filters.FirstOrDefault(f => f.name == filterName);

            if (selectedFilter != null)
            {
                return GetIssuesFromJql(selectedFilter.jql);
            }
            else
            {
                return new List<Issue>();
            }
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
                var searchResult = ExecuteRequest<SearchResult>(HttpStatusCode.OK, $"search?jql={jql}&maxResults=999&startAt={startAt}&fields=summary,project,parent");

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
            return ExecuteRequest<List<Project>>(HttpStatusCode.OK, "project");
        }

        public IEnumerable<Filter> GetFilters()
        {
            return ExecuteRequest<List<Filter>>(HttpStatusCode.OK, "filter/favourite");
        }

        public Transitions GetIssueTransitions(string issueRef)
        {
            return ExecuteRequest<Transitions>(HttpStatusCode.OK, $"issue/{issueRef}/transitions?expand=transitions.fields");
        }

        public void TransitionIssue(string issueRef, string transitionName)
        {
            if (transitionName == null) throw new ArgumentNullException(nameof(transitionName));
            var transitions = GetIssueTransitions(issueRef);
            var transition = transitions.transitions.FirstOrDefault(t => t.name == transitionName);

            if (transition == null)
            {
                throw new JiraClientException($"Unable to locate transition '{transitionName}'");
            }

            var postData = new Dictionary<string, object>
            {
                { "transition", new { id = transition.id } }
            };

            ExecuteRequest(Method.POST, HttpStatusCode.NoContent, $"issue/{issueRef}/transitions", postData);
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

            ExecuteRequest(Method.POST, HttpStatusCode.Created, $"issue/{issueRef}/worklog?adjustEstimate={adjustmentMethod}&newEstimate={newEstimate}&reduceBy=", postData);
        }

        public void AssignIssue(string issueRef, string userName)
        {
            var postData = new Dictionary<string, object>
            {
                { "name", userName }
            };

            ExecuteRequest(Method.PUT, HttpStatusCode.NoContent, $"issue/{issueRef}/assignee", postData);
        }
    }
}
