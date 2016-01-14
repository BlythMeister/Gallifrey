using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Gallifrey.Jira.Model;
using Gallifrey.JiraIntegration;

namespace Gallifrey.UI.Modern.Models
{
    public class SearchModel : INotifyPropertyChanged
    {
        private string searchTerm;
        private string selectedFilter;

        public event PropertyChangedEventHandler PropertyChanged;

        public string SearchTerm
        {
            get { return searchTerm; }
            set
            {
                searchTerm = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasSearchTerm"));
            }
        }

        public string SelectedFilter
        {
            get { return selectedFilter; }
            set
            {
                selectedFilter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasFilter"));
            }
        }

        public bool HasFilter => !string.IsNullOrWhiteSpace(SelectedFilter);
        public bool HasSearchTerm => !string.IsNullOrWhiteSpace(SearchTerm);

        public bool IsSearching { get; set; }
        public ObservableCollection<string> UserFilters { get; set; }
        public ObservableCollection<JiraIssueDisplayModel> SearchResults { get; set; }
        public JiraIssueDisplayModel SelectedSearchResult { get; set; }

        public SearchModel(IEnumerable<string> filters, IEnumerable<RecentJira> recent, IEnumerable<Issue> issues)
        {
            UserFilters = new ObservableCollection<string>(filters);

            var recentDisplay = recent.Select(x => new JiraIssueDisplayModel(x)).ToList();
            var issuesDisplay = issues.Select(x => new JiraIssueDisplayModel(x)).ToList();
            recentDisplay.AddRange(issuesDisplay);

            SearchResults = new ObservableCollection<JiraIssueDisplayModel>(recentDisplay.Distinct().ToList());
        }

        public void UpdateSearchResults(IEnumerable<Issue> jiraIssues)
        {
            SearchResults = new ObservableCollection<JiraIssueDisplayModel>(jiraIssues.Select(x => new JiraIssueDisplayModel(x)));
            IsSearching = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSearching"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SearchResults"));
        }

        public void ClearSearchResults()
        {
            SearchResults = new ObservableCollection<JiraIssueDisplayModel>();
            IsSearching = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSearching"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SearchResults"));
        }

        public void SetIsSearching()
        {
            SearchResults = new ObservableCollection<JiraIssueDisplayModel>();
            IsSearching = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSearching"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SearchResults"));
        }
    }
}