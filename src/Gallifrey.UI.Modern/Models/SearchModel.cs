using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Gallifrey.Jira.Model;

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
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("HasSearchTerm"));
            }
        }

        public string SelectedFilter
        {
            get { return selectedFilter; }
            set
            {
                selectedFilter = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("HasFilter"));
            }
        }

        public bool HasFilter { get { return !string.IsNullOrWhiteSpace(SelectedFilter); } }
        public bool HasSearchTerm { get { return !string.IsNullOrWhiteSpace(SearchTerm); } }
        
        public bool IsSearching { get; set; }
        public ObservableCollection<string> UserFilters { get; set; }
        public ObservableCollection<JiraIssueDisplayModel> SearchResults { get; set; }
        public JiraIssueDisplayModel SelectedSearchResult { get; set; }
        
        public SearchModel(IEnumerable<string> filters, IEnumerable<Issue> jiraIssues)
        {
            UserFilters = new ObservableCollection<string>(filters);
            SearchResults = new ObservableCollection<JiraIssueDisplayModel>(jiraIssues.Select(x=>new JiraIssueDisplayModel(x)));
        }

        public void UpdateSearchResults(IEnumerable<Issue> jiraIssues)
        {
            SearchResults = new ObservableCollection<JiraIssueDisplayModel>(jiraIssues.Select(x => new JiraIssueDisplayModel(x)));
            IsSearching = false;
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsSearching"));
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("SearchResults"));
        }

        public void ClearSearchResults()
        {
            SearchResults = new ObservableCollection<JiraIssueDisplayModel>();
            IsSearching = false;
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsSearching"));
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("SearchResults"));
        }

        public void SetIsSearching()
        {
            SearchResults = new ObservableCollection<JiraIssueDisplayModel>();
            IsSearching = true;
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsSearching"));
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("SearchResults"));
        }
    }
}