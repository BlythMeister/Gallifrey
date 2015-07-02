using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Gallifrey.Jira.Enum;
using Gallifrey.Jira.Exception;
using Gallifrey.Jira.Model;
using RestSharp;
using RestSharp.Deserializers;

namespace Gallifrey.Jira
{
    public class JiraRestClient : IJiraClient
    {
        private readonly string username;
        private readonly string password;
        private readonly RestClient client;
        private readonly JsonDeserializer deserializer;

        public JiraRestClient(string baseUrl, string username, string password)
        {
            this.username = username;
            this.password = password;
            deserializer = new JsonDeserializer();
            client = new RestClient { BaseUrl = new Uri(baseUrl + (baseUrl.EndsWith("/") ? "" : "/") + "rest/api/2/") };
        }

        private RestRequest CreateRequest(Method method, string path)
        {
            var request = new RestRequest { Method = method, Resource = path, RequestFormat = DataFormat.Json };
            request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password))));
            return request;
        }

        /// <exception cref="JiraClientException">When the status returned from Jira was not the expected status or an error message was included.</exception>
        private void AssertStatus(IRestResponse response, HttpStatusCode status)
        {
            if (response.ErrorException != null)
                throw new JiraClientException("Transport level error: " + response.ErrorMessage, response.ErrorException);
            if (response.StatusCode != status)
            {
                try
                {
                    var errorMessages = deserializer.Deserialize<Error>(response);

                    if (errorMessages.errorMessages.Any())
                    {
                        throw new JiraClientException(string.Format("JIRA returned wrong status: {0}. Message: {1}", response.StatusDescription, errorMessages.errorMessages[0]));
                    }
                    else
                    {
                        throw new JiraClientException(string.Format("JIRA returned wrong status: {0}", response.StatusDescription));
                    }
                }
                catch (JiraClientException)
                {
                    throw;
                }
                catch (System.Exception ex)
                {
                    throw new JiraClientException(string.Format("JIRA returned wrong status: {0}", response.StatusDescription), ex);
                }
            }
        }

        private IRestResponse ExectuteRequest(Method method, HttpStatusCode expectedStatus, string path, Dictionary<string, object> data = null)
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

        public User GetCurrentUser()
        {
            var response = ExectuteRequest(Method.GET, HttpStatusCode.OK, "myself");

            return deserializer.Deserialize<User>(response);
        }

        public Issue GetIssue(string issueRef)
        {
            var response = ExectuteRequest(Method.GET, HttpStatusCode.OK, string.Format("issue/{0}", issueRef));

            return deserializer.Deserialize<Issue>(response);
        }

        public Issue GetIssueWithWorklogs(string issueRef)
        {
            var issue = GetIssue(issueRef);

            if (issue.fields.worklog.total > issue.fields.worklog.maxResults)
            {
                var worklogResponse = ExectuteRequest(Method.GET, HttpStatusCode.OK, string.Format("issue/{0}/worklog", issueRef));

                var worklogs = deserializer.Deserialize<WorkLogs>(worklogResponse);

                issue.fields.worklog.worklogs = worklogs.worklogs;
            }

            return issue;
        }

        public IEnumerable<Issue> GetIssuesFromFilter(string filterName)
        {
            var filterResponse = ExectuteRequest(Method.GET, HttpStatusCode.OK, "filter/favourite");

            var filters = deserializer.Deserialize<List<Filter>>(filterResponse);
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
                var response = ExectuteRequest(Method.GET, HttpStatusCode.OK, string.Format("search?jql={0}&maxResults=999&startAt={1}&fields=summary,project,parent", jql, startAt));

                var searchResult = deserializer.Deserialize<SearchResult>(response);

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
            var response = ExectuteRequest(Method.GET, HttpStatusCode.OK, "project");

            return deserializer.Deserialize<List<Project>>(response);
        }

        public IEnumerable<Filter> GetFilters()
        {
            var response = ExectuteRequest(Method.GET, HttpStatusCode.OK, "filter/favourite");

            return deserializer.Deserialize<List<Filter>>(response);
        }

        public Transitions GetIssueTransitions(string issueRef)
        {
            var response = ExectuteRequest(Method.GET, HttpStatusCode.OK, string.Format("issue/{0}/transitions?expand=transitions.fields", issueRef));

            return deserializer.Deserialize<Transitions>(response);
        }

        /// <exception cref="JiraClientException">When unable to transition issue.</exception>
        public void TransitionIssue(string issueRef, string transitionName)
        {
            var transitions = GetIssueTransitions(issueRef);
            var transition = transitions.transitions.FirstOrDefault(t => t.name == transitionName);

            if (transition == null)
            {
                throw new JiraClientException(string.Format("Unable to locate transition '{0}'", transitionName));
            }

            var postData = new Dictionary<string, object>
            {
                { "transition", new { id = transition.id } }
            };

            ExectuteRequest(Method.POST, HttpStatusCode.NoContent, string.Format("issue/{0}/transitions", issueRef), postData);
        }

        public void AddWorkLog(string issueRef, WorkLogStrategy workLogStrategy, string comment, TimeSpan timeSpent, DateTime logDate, TimeSpan? remainingTime = null)
        {
            if (logDate.Kind != DateTimeKind.Local) logDate = DateTime.SpecifyKind(logDate, DateTimeKind.Local);

            var postData = new Dictionary<string, object>
            {
                { "started", string.Format("{0}{1}", logDate.ToString("yyyy-MM-ddTHH:mm:ss.fff"), logDate.ToString("zzz").Replace(":","")) },
                { "comment", comment },
                { "timeSpent", string.Format("{0}h {1}m", timeSpent.Hours, timeSpent.Minutes) },
            };
            var adjustmentMethod = string.Empty;
            var newEstimate = string.Empty;

            if (remainingTime.HasValue)
            {
                newEstimate = string.Format("{0}h {1}m", remainingTime.Value.Hours, remainingTime.Value.Minutes);
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

            ExectuteRequest(Method.POST, HttpStatusCode.Created, string.Format("issue/{0}/worklog?adjustEstimate={1}&newEstimate={2}&reduceBy=", issueRef, adjustmentMethod, newEstimate), postData);
        }

        public void AssignIssue(string issueRef, string userName)
        {
            var postData = new Dictionary<string, object>
            {
                { "name", userName }
            };

            ExectuteRequest(Method.PUT, HttpStatusCode.NoContent, string.Format("issue/{0}/assignee", issueRef), postData);
        }
    }
}
