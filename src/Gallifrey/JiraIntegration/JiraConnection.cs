using System;
using System.Collections.Generic;
using System.Linq;
using Gallifrey.Exceptions.IntergrationPoints;
using Gallifrey.Jira;
using Gallifrey.Settings;

namespace Gallifrey.JiraIntegration
{
    public interface IJiraConnection
    {
        void ReConnect(IJiraConnectionSettings newJiraConnectionSettings, IExportSettings newExportSettings);
        bool DoesJiraExist(string jiraRef);
        Issue GetJiraIssue(string jiraRef);
        IEnumerable<string> GetJiraFilters();
        IEnumerable<Issue> GetJiraIssuesFromFilter(string filterName);
        IEnumerable<Issue> GetJiraIssuesFromSearchText(string searchText);
        TimeSpan GetCurrentLoggedTimeForDate(string jiraRef, DateTime date);
        void LogTime(string jiraRef, DateTime exportTimeStamp, TimeSpan exportTime, WorkLogStrategy strategy, string comment = "", TimeSpan? remainingTime = null);
        IEnumerable<Issue> GetJiraCurrentUserOpenIssues();
        IEnumerable<JiraProject> GetJiraProjects();
        IEnumerable<RecentJira> GetRecentJirasFound();
        void UpdateCache();
    }

    public class JiraConnection : IJiraConnection
    {
        private readonly IRecentJiraCollection recentJiraCollection;
        private readonly List<JiraProject> jiraProjectCache;
        private IJiraConnectionSettings jiraConnectionSettings;
        private IExportSettings exportSettings;
        private JiraClient jira;

        public JiraConnection(IJiraConnectionSettings jiraConnectionSettings, IExportSettings exportSettings)
        {
            recentJiraCollection = new RecentJiraCollection();
            jiraProjectCache = new List<JiraProject>();

            this.jiraConnectionSettings = jiraConnectionSettings;
            this.exportSettings = exportSettings;
            CheckAndConnectJira();
            UpdateJiraProjectCache();
        }

        public void ReConnect(IJiraConnectionSettings newJiraConnectionSettings, IExportSettings newExportSettings)
        {
            exportSettings = newExportSettings;
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
                    jira = new JiraClient(jiraConnectionSettings.JiraUrl.Replace("/secure/Dashboard.jspa", ""), jiraConnectionSettings.JiraUsername, jiraConnectionSettings.JiraPassword);
                    jira.GetAllStatuses();
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
                recentJiraCollection.Remove(jiraRef);
                return false;
            }

            recentJiraCollection.Remove(jiraRef);
            return false;
        }

        public Issue GetJiraIssue(string jiraRef)
        {
            try
            {
                CheckAndConnectJira();
                var issue = jira.GetIssue(jiraRef);
                recentJiraCollection.AddRecentJira(issue.key, issue.fields.project.key, issue.fields.summary);
                return issue;
            }
            catch (Exception ex)
            {
                recentJiraCollection.Remove(jiraRef);
                throw new NoResultsFoundException(string.Format("Unable to locate Jira {0}", jiraRef), ex);
            }
        }

        public IEnumerable<string> GetJiraFilters()
        {
            try
            {
                CheckAndConnectJira();
                var returnedFilters = jira.GetFilters();
                return returnedFilters.Select(returned => returned.name);
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
                var issues = jira.GetIssuesFromFilter(filterName);
                foreach (var issue in issues)
                {
                    recentJiraCollection.AddRecentJira(issue.key, issue.fields.project.key, issue.fields.summary);
                }
                return issues;
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
                var issues = jira.GetIssuesFromJql(GetJql(searchText));
                foreach (var issue in issues)
                {
                    recentJiraCollection.AddRecentJira(issue.key, issue.fields.project.key, issue.fields.summary);
                }
                return issues;
            }
            catch (Exception ex)
            {
                throw new NoResultsFoundException("Error loading jiras from search text", ex);
            }
        }

        public TimeSpan GetCurrentLoggedTimeForDate(string jiraRef, DateTime date)
        {
            var loggedTime = new TimeSpan();

            var issue = jira.GetIssue(jiraRef);

            foreach (var worklog in issue.fields.worklog.worklogs.Where(worklog => worklog.created.Date == date.Date &&
                                                                             worklog.author.name.ToLower() == jiraConnectionSettings.JiraUsername.ToLower()))
            {
                loggedTime = loggedTime.Add(new TimeSpan(0, 0, (int)worklog.timeSpentSeconds));
            }

            return loggedTime;
        }



        public IEnumerable<Issue> GetJiraCurrentUserOpenIssues()
        {
            try
            {
                CheckAndConnectJira();
                var issues = jira.GetIssuesFromJql("assignee in (currentUser()) AND status not in (Closed,Resolved)");
                foreach (var issue in issues)
                {
                    recentJiraCollection.AddRecentJira(issue.key, issue.fields.project.key, issue.fields.summary);
                }
                return issues;
            }
            catch (Exception ex)
            {
                throw new NoResultsFoundException("Error loading jiras from search text", ex);
            }
        }

        private void UpdateJiraProjectCache()
        {
            try
            {
                CheckAndConnectJira();
                var projects = jira.GetProjects();
                jiraProjectCache.Clear();
                jiraProjectCache.AddRange(projects.Select(project => new JiraProject(project.key, project.name)));
            }
            catch (Exception) { }
        }

        public IEnumerable<JiraProject> GetJiraProjects()
        {
            return jiraProjectCache;
        }

        public IEnumerable<RecentJira> GetRecentJirasFound()
        {
            return recentJiraCollection.GetRecentJiraCollection();
        }

        public void UpdateCache()
        {
            recentJiraCollection.RemoveExpiredCache();
            UpdateJiraProjectCache();
        }

        public void LogTime(string jiraRef, DateTime exportTimeStamp, TimeSpan exportTime, WorkLogStrategy strategy, string comment = "", TimeSpan? remainingTime = null)
        {
            var jiraIssue = jira.GetIssue(jiraRef);

            var wasClosed = TryReopenJira(jiraIssue);

            if (string.IsNullOrWhiteSpace(comment)) comment = "No Comment Entered";
            if (!string.IsNullOrWhiteSpace(exportSettings.ExportCommentPrefix))
            {
                comment = string.Format("{0}: {1}", exportSettings.ExportCommentPrefix, comment);
            }

            try
            {
                jira.AddWorkLog(jiraRef, strategy, comment, exportTime, DateTime.SpecifyKind(exportTimeStamp, DateTimeKind.Local), remainingTime);
            }
            catch (Exception ex)
            {
                throw new WorkLogException("Error logging work", ex);
            }
            
            if (wasClosed)
            {
                try
                {
                    ReCloseJira(jiraRef);
                }
                catch (Exception ex)
                {
                    throw new StateChangedException("Time Logged, but state is now open", ex);
                }
            }
        }

        private void ReCloseJira(string jiraRef)
        {
            try
            {
                jira.TransitionIssue(jiraRef, "Close Issue");
            }
            catch (Exception)
            {
                jira.TransitionIssue(jiraRef, "Closed");
            }
        }

        private bool TryReopenJira(Issue jiraIssue)
        {
            var wasClosed = false;
            if (jiraIssue.fields.status.name == "Closed")
            {
                try
                {
                    jira.TransitionIssue(jiraIssue.key, "Reopen Issue");
                    wasClosed = true;
                }
                catch (Exception)
                {
                    try
                    {
                        jira.TransitionIssue(jiraIssue.key, "Open");
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
            var projects = jira.GetProjects();
            foreach (var keyword in searchText.Split(' '))
            {
                var foundProject = false;
                if (projects.Any(project => project.key == keyword))
                {
                    jql = string.Format("project = \"{0}\"", keyword);
                    foundProject = true;
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
