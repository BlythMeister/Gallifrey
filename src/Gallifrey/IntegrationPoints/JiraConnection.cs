using System;
using System.Collections.Generic;
using System.Linq;
using Atlassian.Jira;
using Gallifrey.Exceptions.IntergrationPoints;
using Gallifrey.Settings;

namespace Gallifrey.IntegrationPoints
{
    public interface IJiraConnection
    {
        void ReConnect(JiraConnectionSettings newJiraConnectionSettings);
        bool DoesJiraExist(string jiraRef);
        Issue GetJiraIssue(string jiraRef);
        IEnumerable<string> GetJiraFilters();
        IEnumerable<Issue> GetJiraIssuesFromFilter(string filterName);
        IEnumerable<Issue> GetJiraIssuesFromSearchText(string searchText);
        TimeSpan GetCurrentLoggedTimeForDate(Issue jiraIssue, DateTime date);
        void LogTime(Issue jiraIssue, DateTime exportTimeStamp, TimeSpan exportTime, WorklogStrategy strategy, string comment = "", TimeSpan? remainingTime = null);
    }

    public class JiraConnection : IJiraConnection
    {
        private JiraConnectionSettings jiraConnectionSettings;
        private Jira jira;

        public JiraConnection(JiraConnectionSettings jiraConnectionSettings)
        {
            this.jiraConnectionSettings = jiraConnectionSettings;
            CheckAndConnectJira();
        }

        public void ReConnect(JiraConnectionSettings newJiraConnectionSettings)
        {
            jiraConnectionSettings = newJiraConnectionSettings;
            jira = null;
            CheckAndConnectJira();
        }

        private void CheckAndConnectJira()
        {
            if (jira == null)
            {
                if (string.IsNullOrWhiteSpace(jiraConnectionSettings.JiraUrl) ||
                    string.IsNullOrWhiteSpace(jiraConnectionSettings.JiraUsername) ||
                    string.IsNullOrWhiteSpace(jiraConnectionSettings.JiraPassword))
                {
                    throw new MissingJiraConfigException("Required settings to create connection to jira are missing");
                }

                try
                {
                    jira = new Jira(jiraConnectionSettings.JiraUrl, jiraConnectionSettings.JiraUsername, jiraConnectionSettings.JiraPassword);
                    jira.GetIssuePriorities();
                }
                catch (Exception ex)
                {
                    throw new JiraConnectionException("Error creating instance of Jira", ex);
                }
            }
        }

        public bool DoesJiraExist(string jiraRef)
        {
            try
            {
                CheckAndConnectJira();
                var issue = GetJiraIssue(jiraRef);
                if (issue != null)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public Issue GetJiraIssue(string jiraRef)
        {
            try
            {
                CheckAndConnectJira();
                return jira.GetIssue(jiraRef);
            }
            catch (Exception ex)
            {
                throw new NoResultsFoundException(string.Format("Unable to locate Jira {0}", jiraRef), ex);
            }
        }

        public IEnumerable<string> GetJiraFilters()
        {
            try
            {
                CheckAndConnectJira();
                var returnedFilters = jira.GetFilters();
                return returnedFilters.Select(returned => returned.Name);
            }
            catch (Exception ex)
            {
                throw new NoResultsFoundException("Error loading filters", ex);
            }
        }

        public IEnumerable<Issue> GetJiraIssuesFromFilter(string filterName)
        {
            try
            {
                CheckAndConnectJira();
                return jira.GetIssuesFromFilter(filterName, 0, 999);
            }
            catch (Exception ex)
            {
                throw new NoResultsFoundException("Error loading jiras from filter", ex);
            }
        }

        public IEnumerable<Issue> GetJiraIssuesFromSearchText(string searchText)
        {
            try
            {
                CheckAndConnectJira();
                return jira.GetIssuesFromJql(GetJql(searchText), 0, 999);
            }
            catch (Exception ex)
            {
                throw new NoResultsFoundException("Error loading jiras from search text", ex);
            }
        }

        public TimeSpan GetCurrentLoggedTimeForDate(Issue jiraIssue, DateTime date)
        {
            var loggedTime = new TimeSpan();

            foreach (var worklog in jiraIssue.GetWorklogs().Where(worklog => worklog.StartDate.HasValue &&
                                                                             worklog.StartDate.Value.Date == date.Date &&
                                                                             worklog.Author.ToLower() == jiraConnectionSettings.JiraUsername.ToLower()))
            {
                loggedTime = loggedTime.Add(new TimeSpan(0, 0, (int)worklog.TimeSpentInSeconds));
            }

            return loggedTime;
        }

        public void LogTime(Issue jiraIssue, DateTime exportTimeStamp, TimeSpan exportTime, WorklogStrategy strategy, string comment = "", TimeSpan? remainingTime = null)
        {
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
            }
            catch (Exception ex)
            {
                throw new WorkLogException("Error logging work", ex);
            }


            if (wasClosed)
            {
                try
                {
                    ReCloseJira(jiraIssue);
                }
                catch (Exception ex)
                {
                    throw new StateChangedException("Time Logged, but state is now open", ex);
                }
            }
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
