using Atlassian.Jira;
using Gallifrey.Jira.Enum;
using Gallifrey.Jira.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Issue = Gallifrey.Jira.Model.Issue;
using Project = Gallifrey.Jira.Model.Project;

namespace Gallifrey.Jira
{
    public class JiraSoapClient : IJiraClient
    {
        private readonly string username;
        private readonly Atlassian.Jira.Jira client;
        public bool HasTempo { get; }

        public JiraSoapClient(string baseUrl, string username, string password)
        {
            this.username = username;
            client = new Atlassian.Jira.Jira(baseUrl, username, password) { MaxIssuesPerRequest = 999 };
            client.GetAccessToken();
            HasTempo = false;
        }

        public User GetCurrentUser()
        {
            return new User
            {
                key = username,
                name = username,
                active = true
            };
        }

        public Issue GetIssue(string issueRef)
        {
            var issue = client.GetIssue(issueRef);
            return new Issue
            {
                key = issue.Key.Value,
                fields = new Fields
                {
                    project = new Project
                    {
                        key = issue.Project
                    },
                    status = new Status
                    {
                        name = issue.Status.Name,
                        id = issue.Status.Id
                    },
                    summary = issue.Summary
                }
            };
        }

        public string GetJqlForFilter(string filterName)
        {
            //NOT SUPPORTED IN SOAP API
            return string.Empty;
        }

        public IEnumerable<Issue> GetIssuesFromFilter(string filterName)
        {
            var issues = client.GetIssuesFromFilter(filterName, 0, 999);
            return issues.Select(issue => new Issue
            {
                key = issue.Key.Value,
                fields = new Fields
                {
                    project = new Project
                    {
                        key = issue.Project
                    },
                    status = new Status
                    {
                        name = issue.Status.Name,
                        id = issue.Status.Id
                    },
                    summary = issue.Summary
                }
            });
        }

        public IEnumerable<Issue> GetIssuesFromJql(string jql)
        {
            var issues = client.GetIssuesFromJql(jql, 999);
            return issues.Select(issue => new Issue
            {
                key = issue.Key.Value,
                fields = new Fields
                {
                    project = new Project
                    {
                        key = issue.Project
                    },
                    status = new Status
                    {
                        name = issue.Status.Name,
                        id = issue.Status.Id
                    },
                    summary = issue.Summary
                }
            });
        }

        public IEnumerable<Project> GetProjects()
        {
            var projects = client.GetProjects();
            return projects.Select(project => new Project { key = project.Key, name = project.Name });
        }

        public IEnumerable<Filter> GetFilters()
        {
            var returnedFilters = client.GetFilters();
            return returnedFilters.Select(filter => new Filter { name = filter.Name });
        }

        public IReadOnlyDictionary<string, TimeSpan> GetWorkLoggedForDate(DateTime queryDate)
        {
            var exportedRefs = new Dictionary<string, TimeSpan>();
            var issuesExportedTo = client.GetIssuesFromJql($"worklogAuthor = currentUser() and worklogDate = {queryDate.ToString("yyyy-MM-dd")}", 999);

            foreach (var issue in issuesExportedTo)
            {
                var logs = issue.GetWorklogs().Where(worklog => worklog.Author == username && worklog.StartDate.HasValue && worklog.StartDate.Value.Date == queryDate.Date);
                var timeSpent = TimeSpan.FromSeconds(logs.Sum(x => x.TimeSpentInSeconds));
                exportedRefs.Add(issue.Key.Value, timeSpent);
            }

            return exportedRefs;
        }

        public TimeSpan GetWorkLoggedOnIssue(string issueRef, DateTime queryDate)
        {
            var issue = client.GetIssue(issueRef);
            var logs = issue.GetWorklogs().Where(worklog => worklog.Author == username && worklog.StartDate.HasValue && worklog.StartDate.Value.Date == queryDate.Date);
            return TimeSpan.FromSeconds(logs.Sum(x => x.TimeSpentInSeconds));
        }

        public Transitions GetIssueTransitions(string issueRef)
        {
            var issue = client.GetIssue(issueRef);
            var statuses = issue.GetAvailableActions().Select(action => new Status { name = action.Name, id = action.Id });
            return new Transitions { transitions = statuses.ToList() };
        }

        public void TransitionIssue(string issueRef, string transitionName)
        {
            var issue = client.GetIssue(issueRef);
            issue.WorkflowTransition(transitionName);
        }

        public void AddWorkLog(string issueRef, WorkLogStrategy workLogStrategy, string comment, TimeSpan timeSpent, DateTime logDate, TimeSpan? remainingTime = null)
        {
            if (logDate.Kind != DateTimeKind.Local) logDate = DateTime.SpecifyKind(logDate, DateTimeKind.Local);

            var issue = client.GetIssue(issueRef);

            var worklog = new Worklog($"{timeSpent.Hours}h {timeSpent.Minutes}m", logDate, comment);
            string remaining = null;
            if (remainingTime.HasValue)
            {
                remaining = $"{remainingTime.Value.Hours}h {remainingTime.Value.Minutes}m";
            }

            WorklogStrategy strategy;
            switch (workLogStrategy)
            {
                case WorkLogStrategy.Automatic:
                    strategy = WorklogStrategy.AutoAdjustRemainingEstimate;
                    break;
                case WorkLogStrategy.LeaveRemaining:
                    strategy = WorklogStrategy.RetainRemainingEstimate;
                    break;
                case WorkLogStrategy.SetValue:
                    strategy = WorklogStrategy.NewRemainingEstimate;
                    break;
                default:
                    strategy = WorklogStrategy.RetainRemainingEstimate;
                    break;
            }

            issue.AddWorklog(worklog, strategy, remaining);
        }

        public void AssignIssue(string issueRef, string userName)
        {
            var issue = client.GetIssue(issueRef);
            issue.Assignee = userName;
            issue.SaveChanges();
        }

        public void AddComment(string issueRef, string comment)
        {
            var issue = client.GetIssue(issueRef);
            issue.AddComment(comment);
            issue.SaveChanges();
        }
    }
}