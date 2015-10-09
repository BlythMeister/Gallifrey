using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Helpers;
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
        private SearchModel DataModel { get { return (SearchModel)DataContext; } }

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
            try
            {
                JiraHelperResult<IEnumerable<Issue>> searchResult = null;
                DataModel.SetIsSearching();

                if (!string.IsNullOrWhiteSpace(DataModel.SearchTerm))
                {
                    searchResult = await JiraHelper.Do(() => viewModel.Gallifrey.JiraConnection.GetJiraIssuesFromSearchText(DataModel.SearchTerm), viewModel, "Search In Progress", true);
                }
                else if (!string.IsNullOrWhiteSpace(DataModel.SelectedFilter))
                {
                    searchResult = await JiraHelper.Do(() => viewModel.Gallifrey.JiraConnection.GetJiraIssuesFromFilter(DataModel.SelectedFilter), viewModel, "Search In Progress", true);
                }
                else
                {
                    DialogCoordinator.Instance.ShowMessageAsync(viewModel, "No Results", "Your Search Returned No Results");
                }

                if (searchResult != null)
                {
                    if (searchResult.Cancelled)
                    {
                        DataModel.ClearSearchResults();
                    }
                    else
                    {
                        DataModel.UpdateSearchResults(searchResult.RetVal);
                    }
                }
            }
            catch (Exception)
            {
                DialogCoordinator.Instance.ShowMessageAsync(viewModel, "No Results", "There Was An Error Getting Search Results");
                DataModel.ClearSearchResults();
            }
        }

        private void AddTimer(object sender, RoutedEventArgs e)
        {
            if (DataModel.SelectedSearchResult == null)
            {
                DialogCoordinator.Instance.ShowMessageAsync(viewModel, "No Selected Item", "You Need To Select An Item To Add A Timer For It");
                return;
            }

            if (openFromAdd)
            {
                SelectedJira = DataModel.SelectedSearchResult;
            }
            else
            {
                var addFlyout = new AddTimer(viewModel, DataModel.SelectedSearchResult.Reference);
                viewModel.OpenFlyout(addFlyout);
            }

            IsOpen = false;
        }
    }
}
