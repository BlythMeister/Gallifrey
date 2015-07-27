using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gallifrey.AppTracking;
using Gallifrey.Comparers;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Jira;
using Gallifrey.Jira.Enum;
using Gallifrey.Jira.Model;
using Gallifrey.Settings;

namespace Gallifrey.JiraIntegration
{
    public interface IJiraConnection
    {
        Task ReConnect(IJiraConnectionSettings newJiraConnectionSettings, IExportSettings newExportSettings);
        Task<bool> DoesJiraExist(string jiraRef);
        Task<Issue> GetJiraIssue(string jiraRef, bool includeWorkLogs = false);
        Task<IEnumerable<string>> GetJiraFilters();
        Task<IEnumerable<Issue>> GetJiraIssuesFromFilter(string filterName);
        Task<IEnumerable<Issue>> GetJiraIssuesFromSearchText(string searchText);
        Task LogTime(string jiraRef, DateTime exportTimeStamp, TimeSpan exportTime, WorkLogStrategy strategy, string comment = "", TimeSpan? remainingTime = null);
        Task<IEnumerable<Issue>> GetJiraCurrentUserOpenIssues();
        IEnumerable<JiraProject> GetJiraProjects();
        IEnumerable<RecentJira> GetRecentJirasFound();
        Task UpdateCache();
        Task AssignToCurrentUser(string jiraRef);
        User CurrentUser { get; }
        Task SetInProgress(string jiraRef);
    }

    public class JiraConnection : IJiraConnection
    {
        private readonly ITrackUsage trackUsage;
        private readonly IRecentJiraCollection recentJiraCollection;
        private readonly List<JiraProject> jiraProjectCache;
        private IJiraConnectionSettings jiraConnectionSettings;
        private IExportSettings exportSettings;
        private IJiraClient jira;

        public User CurrentUser { get; private set; }

        public JiraConnection(ITrackUsage trackUsage)
        {
            this.trackUsage = trackUsage;
            recentJiraCollection = new RecentJiraCollection();
            jiraProjectCache = new List<JiraProject>();
        }

        public async Task ReConnect(IJiraConnectionSettings newJiraConnectionSettings, IExportSettings newExportSettings)
        {
            exportSettings = newExportSettings;
            jiraConnectionSettings = newJiraConnectionSettings;
            jira = null;
            await CheckAndConnectJira();
            await UpdateJiraProjectCache();
        }

        private async Task CheckAndConnectJira(bool useRestApi = true)
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
                    if (useRestApi)
                    {
                        jira = new JiraRestClient(jiraConnectionSettings.JiraUrl.Replace("/secure/Dashboard.jspa", ""), jiraConnectionSettings.JiraUsername, jiraConnectionSettings.JiraPassword);
                    }
                    else
                    {
                        jira = new JiraSoapClient(jiraConnectionSettings.JiraUrl.Replace("/secure/Dashboard.jspa", ""), jiraConnectionSettings.JiraUsername, jiraConnectionSettings.JiraPassword); 
                    }

                    CurrentUser = await jira.GetCurrentUser();
                }
                catch (Exception ex)
                {
                    throw new JiraConnectionException("Error creating instance of Jira", ex);    
                }
            }
        }

        public async Task<bool> DoesJiraExist(string jiraRef)
        {
            try
            {
                await CheckAndConnectJira();
                var issue = await GetJiraIssue(jiraRef);
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

        public async Task<Issue> GetJiraIssue(string jiraRef, bool includeWorkLogs = false)
        {
            try
            {
                await CheckAndConnectJira();
                var issue = includeWorkLogs ? await jira.GetIssueWithWorklogs(jiraRef) :
                                              await jira.GetIssue(jiraRef);

                recentJiraCollection.AddRecentJira(issue);
                return issue;
            }
            catch (Exception ex)
            {
                recentJiraCollection.Remove(jiraRef);
                throw new NoResultsFoundException(string.Format("Unable to locate Jira {0}", jiraRef), ex);
            }
        }

        public async Task<IEnumerable<string>> GetJiraFilters()
        {
            try
            {
                await CheckAndConnectJira();
                var returnedFilters = await jira.GetFilters();
                return returnedFilters.Select(returned => returned.name);
            }
            catch (Exception ex)
            {
                throw new NoResultsFoundException("Error loading filters", ex);
            }
        }

        public async Task<IEnumerable<Issue>> GetJiraIssuesFromFilter(string filterName)
        {
            try
            {
                await CheckAndConnectJira();
                var issues = await jira.GetIssuesFromFilter(filterName);
                recentJiraCollection.AddRecentJiras(issues);
                return issues.OrderBy(x => x.key, new JiraReferenceComparer());
            }
            catch (Exception ex)
            {
                throw new NoResultsFoundException("Error loading jiras from filter", ex);
            }
        }

        public async Task<IEnumerable<Issue>> GetJiraIssuesFromSearchText(string searchText)
        {
            try
            {
                await CheckAndConnectJira();
                var issues = await jira.GetIssuesFromJql(await GetJql(searchText));
                recentJiraCollection.AddRecentJiras(issues);
                return issues.OrderBy(x => x.key, new JiraReferenceComparer());
            }
            catch (Exception ex)
            {
                throw new NoResultsFoundException("Error loading jiras from search text", ex);
            }
        }

        /// <exception cref="NoResultsFoundException">Error loading jiras from search text</exception>
        public async Task<IEnumerable<Issue>> GetJiraCurrentUserOpenIssues()
        {
            try
            {
                await CheckAndConnectJira();
                var issues = await jira.GetIssuesFromJql("assignee in (currentUser()) AND status not in (Closed,Resolved)");
                recentJiraCollection.AddRecentJiras(issues);
                return issues.OrderBy(x => x.key, new JiraReferenceComparer());
            }
            catch (Exception ex)
            {
                throw new NoResultsFoundException("Error loading jiras from search text", ex);
            }
        }

        private async Task UpdateJiraProjectCache()
        {
            try
            {
                await CheckAndConnectJira();
                var projects = await jira.GetProjects();
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

        public async Task UpdateCache()
        {
            recentJiraCollection.RemoveExpiredCache();
            await UpdateJiraProjectCache();
        }

        public async Task AssignToCurrentUser(string jiraRef)
        {
            try
            {
                await CheckAndConnectJira();
                await jira.AssignIssue(jiraRef, CurrentUser.name);
            }
            catch (Exception ex)
            {
                throw new JiraConnectionException("Unable to assign issue.", ex);
            }

        }

        public async Task SetInProgress(string jiraRef)
        {
            var inProgressStatuses = new List<string>
            {
                "In Progress",
                "Start Progress"
            };

            await TransitionIssue(jiraRef, inProgressStatuses);
        }

        public async Task LogTime(string jiraRef, DateTime exportTimeStamp, TimeSpan exportTime, WorkLogStrategy strategy, string comment = "", TimeSpan? remainingTime = null)
        {
            trackUsage.TrackAppUsage(TrackingType.ExportOccured);

            var jiraIssue = await jira.GetIssue(jiraRef);
            var wasClosed = await TryReopenJira(jiraIssue);

            if (string.IsNullOrWhiteSpace(comment)) comment = exportSettings.EmptyExportComment;
            if (!string.IsNullOrWhiteSpace(exportSettings.ExportCommentPrefix))
            {
                comment = string.Format("{0}: {1}", exportSettings.ExportCommentPrefix, comment);
            }

            try
            {
                await jira.AddWorkLog(jiraRef, strategy, comment, exportTime, DateTime.SpecifyKind(exportTimeStamp, DateTimeKind.Local), remainingTime);
            }
            catch (Exception ex)
            {
                throw new WorkLogException("Error logging work", ex);
            }

            if (wasClosed)
            {
                try
                {
                    await ReCloseJira(jiraRef);
                }
                catch (Exception ex)
                {
                    throw new StateChangedException("Time Logged, but state is now open", ex);
                }
            }
        }

        private async Task ReCloseJira(string jiraRef)
        {
            var inProgressStatuses = new List<string>
            {
                "Close Issue",
                "Closed"
            };

            await TransitionIssue(jiraRef, inProgressStatuses);
        }

        private async Task<bool> TryReopenJira(Issue jiraIssue)
        {
            var wasClosed = false;
            if (jiraIssue.fields.status.name == "Closed")
            {
                try
                {
                    var inProgressStatuses = new List<string>
                    {
                        "Reopen Issue",
                        "Open"
                    };

                    await TransitionIssue(jiraIssue.key, inProgressStatuses);
                    wasClosed = true;
                }
                catch (Exception)
                {
                    wasClosed = false;
                }
            }
            return wasClosed;
        }

        private async Task TransitionIssue(string jiraRef, IEnumerable<string> possibleTransitionValues)
        {
            Exception lastex = null;
            foreach (var possibleTransitionValue in possibleTransitionValues)
            {
                try
                {
                    await jira.TransitionIssue(jiraRef, possibleTransitionValue);
                    return;
                }
                catch (Exception ex)
                {
                    lastex = ex;
                }
            }

            throw new StateChangedException("Cannot Set In Progress Status", lastex);
        }

        private async Task<string> GetJql(string searchText)
        {
            var jql = string.Empty;
            var searchTerm = string.Empty;
            var projects = await jira.GetProjects();
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
