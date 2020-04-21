using Gallifrey.Comparers;
using Gallifrey.Jira.Model;
using Gallifrey.JiraIntegration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Gallifrey.UI.Modern.Models
{
    public class SearchModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string SearchTerm { get; set; }
        public string SelectedFilter { get; set; }
        public bool IsSearching { get; set; }
        public ObservableCollection<string> UserFilters { get; set; }
        public ObservableCollection<JiraIssueDisplayModel> SearchResults { get; set; }
        public JiraIssueDisplayModel SelectedSearchResult { get; set; }
        public bool OpenFromEdit { get; set; }

        public SearchModel(IEnumerable<string> filters, IEnumerable<RecentJira> recent, IEnumerable<Issue> issues, bool openFromEdit)
        {
            OpenFromEdit = openFromEdit;
            UserFilters = new ObservableCollection<string>(filters);

            var recentDisplay = recent.Select(x => new JiraIssueDisplayModel(x)).ToList();
            var issuesDisplay = issues.Select(x => new JiraIssueDisplayModel(x)).ToList();
            recentDisplay.AddRange(issuesDisplay);
            recentDisplay = recentDisplay.Distinct().OrderBy(x => x.Reference, new JiraReferenceComparer()).ToList();

            SearchResults = new ObservableCollection<JiraIssueDisplayModel>(recentDisplay);
        }

        public void UpdateSearchResults(IEnumerable<Issue> jiraIssues)
        {
            SearchResults = new ObservableCollection<JiraIssueDisplayModel>(jiraIssues.Select(x => new JiraIssueDisplayModel(x)));
            IsSearching = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSearching)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchResults)));
        }

        public void ClearSearchResults()
        {
            SearchResults = new ObservableCollection<JiraIssueDisplayModel>();
            IsSearching = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSearching)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchResults)));
        }

        public void SetIsSearching()
        {
            SearchResults = new ObservableCollection<JiraIssueDisplayModel>();
            IsSearching = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSearching)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchResults)));
        }
    }
}
