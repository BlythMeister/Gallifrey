using System;
using System.Collections.Generic;
using System.Linq;
using Atlassian.Jira;
using Gallifrey.JiraIntegration;
using Gallifrey.Settings;

namespace Gallifrey.IntegrationPoints
{
    public class JiraConnection
    {
        private readonly AppSettings appSettings;
        private readonly Jira jira;

        public JiraConnection(AppSettings appSettings)
        {
            this.appSettings = appSettings;
            jira = new Jira(appSettings.JiraUrl, appSettings.JiraUsername, appSettings.JiraPassword);
        }

        public bool DoesJiraExist(string jiraRef)
        {
            try
            {
                var issue = GetJiraIssue(jiraRef);
                if (issue != null)
                {
                    return issue.Result && issue.Issues.Any();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public GetIssueResult GetJiraIssue(string jiraRef)
        {
            var issues = new List<Issue>();

            try
            {
                issues.Add(jira.GetIssue(jiraRef));
            }
            catch (Exception ex)
            {
                return new GetIssueResult(false, null, ex.Message);
            }

            return new GetIssueResult(true, issues, string.Empty);
        }

        public GetFilterResult GetJiraFilters()
        {
            var filters = new List<JiraNamedEntity>();
            try
            {
                filters.AddRange(jira.GetFilters());
            }
            catch (Exception ex)
            {
                return new GetFilterResult(false, null, ex.Message);
            }

            return new GetFilterResult(true, filters, string.Empty);
        }

        public GetIssueResult GetJiraIssuesFromFilter(string filterName)
        {
            var issues = new List<Issue>();

            try
            {
                issues.AddRange(jira.GetIssuesFromFilter(filterName, 0, 999));
            }
            catch (Exception ex)
            {
                return new GetIssueResult(false, null, ex.Message);
            }

            return new GetIssueResult(true, issues, string.Empty);
        }

        public GetIssueResult GetJiraIssuesFromSearchText(string searchText)
        {
            var issues = new List<Issue>();

            try
            {
                issues.AddRange(jira.GetIssuesFromJql(GetJql(searchText), 0, 999));
            }
            catch (Exception ex)
            {
                return new GetIssueResult(false, null, ex.Message);
            }

            return new GetIssueResult(true, issues, string.Empty);
        }

        public TimeSpan GetCurrentLoggedTimeForDate(Issue jiraIssue, DateTime date)
        {
            var loggedTime = new TimeSpan();

            foreach (var worklog in jiraIssue.GetWorklogs().Where(worklog => worklog.StartDate.HasValue &&
                                                                             worklog.StartDate.Value.Date == date.Date &&
                                                                             worklog.Author.ToLower() == appSettings.JiraUsername.ToLower()))
            {
                loggedTime = loggedTime.Add(new TimeSpan(0, 0, (int)worklog.TimeSpentInSeconds));
            }

            return loggedTime;
        }

        public LogWorkResult LogTime(Issue jiraIssue, DateTime exportTimeStamp, TimeSpan exportTime, WorklogStrategy strategy, string comment = "", TimeSpan? remainingTime = null)
        {
            var result = false;
            var resultMessage = string.Empty;
            var wasClosed = TryReopenJira(jiraIssue);
            comment = "Gallifrey: " + comment;
            if (string.IsNullOrWhiteSpace(comment)) comment = "No Comment Entered";

            var worklog = new Worklog(string.Format("{0}h {1}m", exportTime.Hours, exportTime.Minutes), DateTime.SpecifyKind(exportTimeStamp, DateTimeKind.Local), comment);
            string remaining = null;
            if (remainingTime.HasValue)
            {
                remaining = string.Format("{0}h {1}m", exportTime.Hours, exportTime.Minutes);
            }
            try
            {
                jiraIssue.AddWorklog(worklog, strategy, remaining);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                resultMessage = ex.Message;
            }


            if (wasClosed)
            {
                try
                {
                    ReCloseJira(jiraIssue);
                }
                catch (Exception)
                {
                    if (!string.IsNullOrWhiteSpace(resultMessage)) resultMessage += "\n";

                    resultMessage += "Error Trying To Re-Close Jira";
                }
            }

            return new LogWorkResult(result, resultMessage);
        }

        private void ReCloseJira(Issue jiraIssue)
        {
            try
            {
                jiraIssue.WorkflowTransition("Close Issue");
            }
            catch (Exception)
            {
                jiraIssue.WorkflowTransition("Closed");
            }
        }

        private bool TryReopenJira(Issue jiraIssue)
        {
            var wasClosed = false;
            if (jiraIssue.Status.Name == "Closed")
            {
                try
                {
                    jiraIssue.WorkflowTransition("Reopen Issue");
                    wasClosed = true;
                }
                catch (Exception)
                {
                    try
                    {
                        jiraIssue.WorkflowTransition("Open");
                        wasClosed = true;
                    }
                    catch (Exception)
                    {
                        wasClosed = false;
                    }
                }
            }
            return wasClosed;
        }

        private string GetJql(string searchText)
        {
            var jql = string.Empty;
            var searchTerm = string.Empty;
            dynamic projects = jira.GetProjects();
            foreach (var keyword in searchText.Split(' '))
            {
                var foundProject = false;
                foreach (var project in projects)
                {
                    if (project.Key == keyword)
                    {
                        jql = string.Format("project = \"{0}\"", keyword);
                        foundProject = true;
                        break;
                    }
                }

                if (!foundProject)
                {
                    searchTerm += " " + keyword;
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                if (!string.IsNullOrWhiteSpace(jql))
                {
                    jql += " AND ";
                }
                jql += string.Format(" text ~ \"{0}\"", searchTerm);
            }

            try
            {
                if ((jira.GetIssue(searchText) != null))
                {
                    jql = string.Format("key = \"{0}\" OR ({1})", searchText, jql);
                }

            }
            catch
            {
            }

            return jql;
        }
    }
}
