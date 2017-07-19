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
        private readonly ISimpleRestClient restClient;
        private readonly User myUser;
        public bool HasTempo { get; }

        public JiraRestClient(string baseUrl, string username, string password)
        {
            var url = baseUrl + (baseUrl.EndsWith("/") ? "" : "/") + "rest/";
            restClient = new SimpleRestClient(url, username, password, GetErrorMessages);
            myUser = GetCurrentUser();
            try
            {
                //NOTE THIS IS NOT THE RIGHT TYPE, BUT NOT EXPECTING VALID DATA ANYWAY
                restClient.Get<List<string>>(HttpStatusCode.OK, "tempo-timesheets/3/worklogs?dateFrom=1990-01-01&dateTo=1990-01-02");
                HasTempo = true;
            }
            catch (Exception)
            {
                HasTempo = false;
            }
        }

        public User GetCurrentUser()
        {
            return restClient.Get<User>(HttpStatusCode.OK, "api/2/myself");
        }

        public Issue GetIssue(string issueRef)
        {
            return restClient.Get<Issue>(HttpStatusCode.OK, $"api/2/issue/{issueRef}");
        }

        public string GetJqlForFilter(string filterName)
        {
            var filters = restClient.Get<List<Filter>>(HttpStatusCode.OK, "api/2/filter/favourite");

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
                var searchResult = restClient.Get<SearchResult>(HttpStatusCode.OK, $"api/2/search?jql={jql}&maxResults=999&startAt={startAt}&fields=summary,project,parent");

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
            return restClient.Get<List<Project>>(HttpStatusCode.OK, "api/2/project");
        }

        public IEnumerable<Filter> GetFilters()
        {
            return restClient.Get<List<Filter>>(HttpStatusCode.OK, "api/2/filter/favourite");
        }

        public IReadOnlyDictionary<string, TimeSpan> GetWorkLoggedForDate(DateTime queryDate)
        {
            var exportedRefs = new Dictionary<string, TimeSpan>();

            if (hasTempo)
            {
                var logs = restClient.Get<List<TempoWorkLog>>(HttpStatusCode.OK, $"tempo-timesheets/3/worklogs?dateFrom={queryDate.ToString("yyyy-MM-dd")}&dateTo={queryDate.AddDays(1).ToString("yyyy-MM-dd")}&username={myUser.key}");
                foreach (var tempoWorkLog in logs)
                {
                    var workLogTime = TimeSpan.FromSeconds(tempoWorkLog.timeSpentSeconds);
                    if (exportedRefs.ContainsKey(tempoWorkLog.issue.key))
                    {
                        exportedRefs[tempoWorkLog.issue.key].Add(workLogTime);
                    }
                    else
                    {
                        exportedRefs.Add(tempoWorkLog.issue.key, workLogTime);
                    }
                }
            }
            else
            {
                var issuesExportedTo = GetIssuesFromJql($"worklogAuthor = currentUser() and worklogDate = {queryDate.ToString("yyyy-MM-dd")}");

                foreach (var issue in issuesExportedTo)
                {
                    var logs = restClient.Get(HttpStatusCode.OK, $"api/2/issue/{issue.key}/worklog", customDeserialize: s => FilterWorklogsToUser(s, myUser.key));
                    var timeSpent = TimeSpan.FromSeconds(logs.worklogs.Where(x => x.started.Date == queryDate.Date).Sum(x => x.timeSpentSeconds));
                    exportedRefs.Add(issue.key, timeSpent);
                }
            }

            return exportedRefs;
        }

        public TimeSpan GetWorkLoggedOnIssue(string issueRef, DateTime queryDate)
        {
            if (hasTempo)
            {
                var logs = restClient.Get<List<TempoWorkLog>>(HttpStatusCode.OK, $"tempo-timesheets/3/worklogs?dateFrom={queryDate.ToString("yyyy-MM-dd")}&dateTo={queryDate.AddDays(1).ToString("yyyy-MM-dd")}&username={myUser.key}");
                return TimeSpan.FromSeconds(logs.Where(x => x.issue.key == issueRef).Sum(x => x.timeSpentSeconds));
            }
            else
            {
                var issue = GetIssue(issueRef);
                var logs = restClient.Get(HttpStatusCode.OK, $"api/2/issue/{issue.key}/worklog", customDeserialize: s => FilterWorklogsToUser(s, myUser.key));
                return TimeSpan.FromSeconds(logs.worklogs.Where(x => x.started.Date == queryDate.Date).Sum(x => x.timeSpentSeconds));
            }
        }

        public Transitions GetIssueTransitions(string issueRef)
        {
            return restClient.Get<Transitions>(HttpStatusCode.OK, $"api/2/issue/{issueRef}/transitions?expand=transitions.fields");
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

            restClient.Post(HttpStatusCode.NoContent, $"api/2/issue/{issueRef}/transitions", postData);
        }

        public void AddWorkLog(string issueRef, WorkLogStrategy workLogStrategy, string comment, TimeSpan timeSpent, DateTime logDate, TimeSpan? remainingTime = null)
        {
            if (logDate.Kind != DateTimeKind.Local) logDate = DateTime.SpecifyKind(logDate, DateTimeKind.Local);

            if (hasTempo)
            {
                double? remaining = 0;
                switch (workLogStrategy)
                {
                    case WorkLogStrategy.Automatic:
                        remaining = GetIssue(issueRef).fields.timetracking.remainingEstimateSeconds - timeSpent.TotalSeconds;
                        break;
                    case WorkLogStrategy.LeaveRemaining:
                        remaining = null;
                        break;
                    case WorkLogStrategy.SetValue:
                        remaining = remainingTime?.TotalSeconds ?? 0;
                        break;
                }

                //remove seconds
                timeSpent = new TimeSpan(timeSpent.Hours, timeSpent.Minutes, 0);

                var tempoWorkLog = new TempoWorkLog { issue = new TempoWorkLog.TempoWorkLogIssue { key = issueRef, remainingEstimateSeconds = remaining }, timeSpentSeconds = timeSpent.TotalSeconds, dateStarted = $"{logDate:s}.000", comment = comment, author = new TempoWorkLog.TempoWorkLogUser { key = myUser.key, name = myUser.name, displayName = myUser.displayName } };
                restClient.Post(HttpStatusCode.OK, "tempo-timesheets/3/worklogs", tempoWorkLog);
            }
            else
            {
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

                restClient.Post(HttpStatusCode.Created, $"api/2/issue/{issueRef}/worklog?adjustEstimate={adjustmentMethod}&newEstimate={newEstimate}&reduceBy=", postData);
            }
        }

        public void AssignIssue(string issueRef, string userName)
        {
            var postData = new Dictionary<string, object>
            {
                { "name", userName }
            };

            restClient.Put(HttpStatusCode.NoContent, $"api/2/issue/{issueRef}/assignee", postData);
        }

        public void AddComment(string issueRef, string comment)
        {
            var postData = new Dictionary<string, object>
            {
                { "body", comment }
            };

            restClient.Post(HttpStatusCode.Created, $"api/2/issue/{issueRef}/comment", postData);
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
