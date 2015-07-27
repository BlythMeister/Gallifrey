using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atlassian.Jira;
using Gallifrey.Jira.Enum;
using Gallifrey.Jira.Model;
using Issue = Gallifrey.Jira.Model.Issue;
using Project = Gallifrey.Jira.Model.Project;

namespace Gallifrey.Jira
{
    public class JiraSoapClient : IJiraClient
    {
        private readonly string username;
        private readonly Atlassian.Jira.Jira client;

        public JiraSoapClient(string baseUrl, string username, string password)
        {
            this.username = username;
            client = new Atlassian.Jira.Jira(baseUrl, username, password) { MaxIssuesPerRequest = 999 };
        }

        public async Task<User> GetCurrentUser()
        {
            var token = await Task.Run(() => client.GetAccessToken());

            return new User
            {
                key = username,
                name = username,
                active = true
            };
        }

        public async Task<Issue> GetIssue(string issueRef)
        {
            var issue = await Task.Run(() => client.GetIssue(issueRef));

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

        public async Task<Issue> GetIssueWithWorklogs(string issueRef)
        {
            var issue = await Task.Run(() => client.GetIssue(issueRef));
            var worklogs = await Task.Run(() => issue.GetWorklogs().Select(worklog => new WorkLog { author = new User { name = worklog.Author }, comment = worklog.Comment, started = worklog.StartDate.Value, timeSpent = worklog.TimeSpent, timeSpentSeconds = worklog.TimeSpentInSeconds }).ToList());

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
                    worklog = new WorkLogs
                    {
                        maxResults = worklogs.Count(),
                        total = worklogs.Count(),
                        worklogs = worklogs
                    },
                    summary = issue.Summary,
                }
            };
        }

        public async Task<IEnumerable<Issue>> GetIssuesFromFilter(string filterName)
        {
            var issues = await Task.Run(() => client.GetIssuesFromFilter(filterName, 0, 999));

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

        public async Task<IEnumerable<Issue>> GetIssuesFromJql(string jql)
        {
            var issues = await Task.Run(() => client.GetIssuesFromJql(jql, 999));

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

        public async Task<IEnumerable<Project>> GetProjects()
        {
            var projects = await Task.Run(() => client.GetProjects());
            return projects.Select(project => new Project { key = project.Key, name = project.Name });
        }

        public async Task<IEnumerable<Filter>> GetFilters()
        {
            var returnedFilters = await Task.Run(() => client.GetFilters());
            return returnedFilters.Select(filter => new Filter { name = filter.Name });
        }

        public async Task<Transitions> GetIssueTransitions(string issueRef)
        {
            var issue = await Task.Run(() => client.GetIssue(issueRef));
            var statuses = await Task.Run(() => issue.GetAvailableActions().Select(action => new Status { name = action.Name, id = action.Id }));
            return new Transitions { transitions = statuses.ToList() };
        }

        public async Task TransitionIssue(string issueRef, string transitionName)
        {
            var issue = await Task.Run(() => client.GetIssue(issueRef));
            issue.WorkflowTransition(transitionName);
        }

        public async Task AddWorkLog(string issueRef, WorkLogStrategy workLogStrategy, string comment, TimeSpan timeSpent, DateTime logDate, TimeSpan? remainingTime = null)
        {
            if (logDate.Kind != DateTimeKind.Local) logDate = DateTime.SpecifyKind(logDate, DateTimeKind.Local);

            var issue = await Task.Run(() => client.GetIssue(issueRef));

            var worklog = new Worklog(string.Format("{0}h {1}m", timeSpent.Hours, timeSpent.Minutes), logDate, comment);
            string remaining = null;
            if (remainingTime.HasValue)
            {
                remaining = string.Format("{0}h {1}m", remainingTime.Value.Hours, remainingTime.Value.Minutes);
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

            await Task.Run(() => issue.AddWorklog(worklog, strategy, remaining));
        }

        public async Task AssignIssue(string issueRef, string userName)
        {
            var issue = await Task.Run(() => client.GetIssue(issueRef));
            issue.Assignee = userName;
            issue.SaveChanges();
        }
    }
}