using Gallifrey.Jira.Enum;
using Gallifrey.Jira.Model;
using System;
using System.Collections.Generic;

namespace Gallifrey.Jira
{
    public interface IJiraClient
    {
        User GetCurrentUser();
        Issue GetIssue(string issueRef);
        string GetJqlForFilter(string filterName);
        IEnumerable<Issue> GetIssuesFromFilter(string filterName);
        IEnumerable<Issue> GetIssuesFromJql(string jql);
        IEnumerable<Project> GetProjects();
        IEnumerable<Filter> GetFilters();
        IEnumerable<StandardWorkLog> GetWorkLoggedForDatesFilteredIssues(List<DateTime> queryDates, List<string> issueRefs);
        Transitions GetIssueTransitions(string issueRef);
        void TransitionIssue(string issueRef, string transitionName);
        void AddWorkLog(string issueRef, WorkLogStrategy workLogStrategy, string comment, TimeSpan timeSpent, DateTime logDate, TimeSpan? remainingTime = null);
        void AssignIssue(string issueRef, string userName);
        void AddComment(string issueRef, string comment);
    }
}