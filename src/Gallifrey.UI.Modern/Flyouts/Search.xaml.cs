using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Search
    {
        public JiraIssueDisplayModel SelectedJira { get; private set; }

        private readonly MainViewModel viewModel;
        private readonly bool openFromAdd;

        public Search(MainViewModel viewModel, bool openFromAdd)
        {
            this.viewModel = viewModel;
            this.openFromAdd = openFromAdd;
            InitializeComponent();

            var filters = viewModel.Gallifrey.JiraConnection.GetJiraFilters();
            var issues = viewModel.Gallifrey.JiraConnection.GetJiraCurrentUserOpenIssues();
            DataContext = new SearchModel(filters, issues);
        }

        private async void SearchButton(object sender, RoutedEventArgs e)
        {
            //MAKE ASYNC

            var model = (SearchModel)DataContext;
            Task<IEnumerable<Issue>> searchTask = null;
            model.SetIsSearching();

            if (!string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                searchTask = Task.Factory.StartNew(() => viewModel.Gallifrey.JiraConnection.GetJiraIssuesFromSearchText(model.SearchTerm));
            }
            else if (!string.IsNullOrWhiteSpace(model.SelectedFilter))
            {
                searchTask = Task.Factory.StartNew(() => viewModel.Gallifrey.JiraConnection.GetJiraIssuesFromFilter(model.SelectedFilter));
            }
            else
            {
                viewModel.MainWindow.ShowMessageAsync("No Results", "Your Search Returned No Results");
            }

            if (searchTask != null)
            {
                try
                {
                    await searchTask;
                    if (searchTask.IsCompleted)
                    {
                        model.UpdateSearchResults(searchTask.Result);
                    }
                    else
                    {
                        viewModel.MainWindow.ShowMessageAsync("No Results", "There Was An Error Getting Search Results");
                    }
                }
                catch (Exception)
                {
                    viewModel.MainWindow.ShowMessageAsync("No Results", "There Was An Error Getting Search Results");
                }
            }
        }

        private void AddTimer(object sender, RoutedEventArgs e)
        {
            var model = (SearchModel)DataContext;
            if (model.SelectedSearchResult == null)
            {
                viewModel.MainWindow.ShowMessageAsync("No Selected Item", "You Need To Select An Item To Add A Timer For It");
                return;
            }

            if (openFromAdd)
            {
                SelectedJira = model.SelectedSearchResult;
            }
            else
            {
                var addFlyout = new AddTimer(viewModel, model.SelectedSearchResult.Reference);
                viewModel.MainWindow.OpenFlyout(addFlyout);
            }

            IsOpen = false;
        }
    }

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

        public void SetIsSearching()
        {
            SearchResults = new ObservableCollection<JiraIssueDisplayModel>();
            IsSearching = true;
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsSearching"));
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("SearchResults"));
        }
    }

    public class JiraIssueDisplayModel
    {
        public string Reference { get; set; }
        public string Description { get; set; }
        public bool HasParent { get { return !string.IsNullOrWhiteSpace(ParentReference); } }
        public string ParentReference { get; set; }
        public string ParentDescription { get; set; }

        public JiraIssueDisplayModel(Issue issue)
        {
            Reference = issue.key;
            Description = issue.fields.summary;
            if (issue.fields.parent != null)
            {
                ParentReference = issue.fields.parent.key;
                ParentDescription = issue.fields.parent.fields.summary;
            }
        }
    }
}
