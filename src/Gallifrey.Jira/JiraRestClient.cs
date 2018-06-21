using Gallifrey.Jira.Enum;
using Gallifrey.Jira.Model;
using Gallifrey.Rest;
using Gallifrey.Rest.Exception;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Gallifrey.Jira
{
    public class JiraRestClient : IJiraClient
    {
        private readonly ISimpleRestClient jiraClient;
        private readonly ISimpleRestClient tempoClient;
        private readonly User myUser;

        public JiraRestClient(string baseUrl, string username, string password, bool useTempo, string tempoToken)
        {
            var url = baseUrl + (baseUrl.EndsWith("/") ? "" : "/") + "rest/api/2";
            jiraClient = SimpleRestClient.WithBasicAuthenticaion(url, username, password, GetErrorMessages);

            myUser = GetCurrentUser();
            if (useTempo)
            {
                tempoClient = SimpleRestClient.WithBearerAuthentication("https://api.tempo.io/rest-legacy/tempo-timesheets/3", tempoToken, null);

                var queryDate = DateTime.UtcNow;
                tempoClient.Get<List<TempoWorkLog>>(HttpStatusCode.OK, $"worklogs?dateFrom={queryDate:yyyy-MM-dd}&dateTo={queryDate:yyyy-MM-dd}&username={myUser.key}");
            }
            else
            {
                tempoClient = null;
            }
        }

        public User GetCurrentUser()
        {
            return jiraClient.Get<User>(HttpStatusCode.OK, "myself");
        }

        public Issue GetIssue(string issueRef)
        {
            return jiraClient.Get<Issue>(HttpStatusCode.OK, $"issue/{issueRef}");
        }

        public string GetJqlForFilter(string filterName)
        {
            var filters = jiraClient.Get<List<Filter>>(HttpStatusCode.OK, "filter/favourite");

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
                var searchResult = jiraClient.Get<SearchResult>(HttpStatusCode.OK, $"search?jql={jql}&maxResults=999&startAt={startAt}&fields=summary,project,parent");

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
            return jiraClient.Get<List<Project>>(HttpStatusCode.OK, "project");
        }

        public IEnumerable<Filter> GetFilters()
        {
            return jiraClient.Get<List<Filter>>(HttpStatusCode.OK, "filter/favourite");
        }

        public IEnumerable<StandardWorkLog> GetWorkLoggedForDatesFilteredIssues(List<DateTime> queryDates, List<string> issueRefs)
        {
            var workLogs = new List<StandardWorkLog>();

            if (tempoClient != null)
            {
                foreach (var queryDate in queryDates)
                {
                    var logs = tempoClient.Get<List<TempoWorkLog>>(HttpStatusCode.OK, $"worklogs?dateFrom={queryDate:yyyy-MM-dd}&dateTo={queryDate:yyyy-MM-dd}&username={myUser.key}");
                    foreach (var tempoWorkLog in logs)
                    {
                        if (issueRefs == null || issueRefs.Any(x => string.Equals(x, tempoWorkLog.issue.key, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            var workLogReturn = workLogs.FirstOrDefault(x => x.JiraRef == tempoWorkLog.issue.key && x.LoggedDate.Date == queryDate.Date);
                            if (workLogReturn != null)
                            {
                                workLogReturn.AddTime(tempoWorkLog.timeSpentSeconds);
                            }
                            else
                            {
                                workLogs.Add(new StandardWorkLog(tempoWorkLog.issue.key, queryDate.Date, tempoWorkLog.timeSpentSeconds));
                            }
                        }
                    }
                }
            }
            else
            {
                var workLogCache = new Dictionary<string, WorkLogs>();
                foreach (var queryDate in queryDates)
                {
                    var issuesExportedTo = GetIssuesFromJql($"worklogAuthor = currentUser() and worklogDate = {queryDate:yyyy-MM-dd}");
                    foreach (var issue in issuesExportedTo)
                    {
                        if (issueRefs == null || issueRefs.Any(x => string.Equals(x, issue.key, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            WorkLogs logs;
                            if (workLogCache.ContainsKey(issue.key))
                            {
                                logs = workLogCache[issue.key];
                            }
                            else
                            {
                                logs = jiraClient.Get(HttpStatusCode.OK, $"issue/{issue.key}/worklog", customDeserialize: s => FilterWorklogsToUser(s, myUser));
                                workLogCache.Add(issue.key, logs);
                            }

                            foreach (var workLog in logs.worklogs.Where(x => x.started.Date == queryDate.Date))
                            {
                                var workLogReturn = workLogs.FirstOrDefault(x => x.JiraRef == issue.key && x.LoggedDate.Date == queryDate.Date);
                                if (workLogReturn != null)
                                {
                                    workLogReturn.AddTime(workLog.timeSpentSeconds);
                                }
                                else
                                {
                                    workLogs.Add(new StandardWorkLog(issue.key, queryDate.Date, workLog.timeSpentSeconds));
                                }

                            }
                        }
                    }
                }
            }

            return workLogs;
        }

        public Transitions GetIssueTransitions(string issueRef)
        {
            return jiraClient.Get<Transitions>(HttpStatusCode.OK, $"issue/{issueRef}/transitions?expand=transitions.fields");
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
                { "transition", new {transition.id } }
            };

            jiraClient.Post(HttpStatusCode.NoContent, $"issue/{issueRef}/transitions", postData);
        }

        public void AddWorkLog(string issueRef, WorkLogStrategy workLogStrategy, string comment, TimeSpan timeSpent, DateTime logDate, TimeSpan? remainingTime = null)
        {
            if (logDate.Kind != DateTimeKind.Local) logDate = DateTime.SpecifyKind(logDate, DateTimeKind.Local);
            timeSpent = new TimeSpan(timeSpent.Hours, timeSpent.Minutes, 0);

            if (tempoClient != null)
            {
                if (string.IsNullOrWhiteSpace(comment)) comment = "N/A";
                var issue = new TempoWorkLog.TempoWorkLogIssue();
                switch (workLogStrategy)
                {
                    case WorkLogStrategy.Automatic:
                        var remaining = GetIssue(issueRef).fields.timetracking.remainingEstimateSeconds - timeSpent.TotalSeconds;
                        issue = new TempoWorkLog.TempoWorkLogIssue { key = issueRef, remainingEstimateSeconds = remaining };
                        break;
                    case WorkLogStrategy.LeaveRemaining:
                        issue = new TempoWorkLog.TempoWorkLogIssue { key = issueRef };
                        break;
                    case WorkLogStrategy.SetValue:
                        issue = new TempoWorkLog.TempoWorkLogIssue { key = issueRef, remainingEstimateSeconds = remainingTime?.TotalSeconds ?? 0 };
                        break;
                }

                var tempoWorkLog = new TempoWorkLog { issue = issue, timeSpentSeconds = timeSpent.TotalSeconds, dateStarted = $"{logDate:s}.000", comment = comment, author = new TempoWorkLog.TempoWorkLogUser { key = myUser.key, name = myUser.name } };
                jiraClient.Post(HttpStatusCode.OK, "worklogs", tempoWorkLog);
            }
            else
            {
                var postData = new Dictionary<string, object>
                {
                    { "started", $"{logDate:yyyy-MM-ddTHH:mm:ss.fff}{logDate.ToString("zzz").Replace(":", "")}"},
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

                jiraClient.Post(HttpStatusCode.Created, $"issue/{issueRef}/worklog?adjustEstimate={adjustmentMethod}&newEstimate={newEstimate}&reduceBy=", postData);
            }
        }

        public void AssignIssue(string issueRef, string userName)
        {
            var postData = new Dictionary<string, object>
            {
                { "name", userName }
            };

            jiraClient.Put(HttpStatusCode.NoContent, $"issue/{issueRef}/assignee", postData);
        }

        public void AddComment(string issueRef, string comment)
        {
            var postData = new Dictionary<string, object>
            {
                { "body", comment }
            };

            jiraClient.Post(HttpStatusCode.Created, $"issue/{issueRef}/comment", postData);
        }

        private static WorkLogs FilterWorklogsToUser(string rawJson, User user)
        {
            var jsonObject = JObject.Parse(rawJson);
            var filtered = jsonObject["worklogs"].Children().Where(x =>
            {
                var logName = ((string)x["author"]["name"]).ToLower();
                var userNameMatch = !string.IsNullOrWhiteSpace(user.name) && logName == user.name.ToLower();
                var userKeyMatch = !string.IsNullOrWhiteSpace(user.key) && logName == user.key.ToLower();
                return userNameMatch || userKeyMatch;
            });
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
