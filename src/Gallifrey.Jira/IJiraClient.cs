using System;
using System.Collections.Generic;
using Gallifrey.Jira.Enum;
using Gallifrey.Jira.Model;

namespace Gallifrey.Jira
{
    public interface IJiraClient
    {
        User GetCurrentUser();
        Issue GetIssue(string issueRef);
        Issue GetIssueWithWorklogs(string issueRef, string user);
        IEnumerable<Issue> GetIssuesFromFilter(string filterName);
        IEnumerable<Issue> GetIssuesFromJql(string jql);
        IEnumerable<Project> GetProjects();
        IEnumerable<Filter> GetFilters();
        Transitions GetIssueTransitions(string issueRef);
        void TransitionIssue(string issueRef, string transitionName);
        void AddWorkLog(string issueRef, WorkLogStrategy workLogStrategy, string comment, TimeSpan timeSpent, DateTime logDate, TimeSpan? remainingTime = null);
        void AssignIssue(string issueRef, string userName);
    }
}