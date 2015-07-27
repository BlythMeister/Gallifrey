using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gallifrey.Jira.Enum;
using Gallifrey.Jira.Model;

namespace Gallifrey.Jira
{
    public interface IJiraClient
    {
        Task<User> GetCurrentUser();
        Task<Issue> GetIssue(string issueRef);
        Task<Issue> GetIssueWithWorklogs(string issueRef);
        Task<IEnumerable<Issue>> GetIssuesFromFilter(string filterName);
        Task<IEnumerable<Issue>> GetIssuesFromJql(string jql);
        Task<IEnumerable<Project>> GetProjects();
        Task<IEnumerable<Filter>> GetFilters();
        Task<Transitions> GetIssueTransitions(string issueRef);
        Task TransitionIssue(string issueRef, string transitionName);
        Task AddWorkLog(string issueRef, WorkLogStrategy workLogStrategy, string comment, TimeSpan timeSpent, DateTime logDate, TimeSpan? remainingTime = null);
        Task AssignIssue(string issueRef, string userName);
    }
}