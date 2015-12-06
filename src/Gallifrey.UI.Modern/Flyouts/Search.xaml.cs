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
        private readonly JiraHelper jiraHelper;

        public Search(MainViewModel viewModel, bool openFromAdd)
        {
            this.viewModel = viewModel;
            this.openFromAdd = openFromAdd;
            InitializeComponent();
            jiraHelper = new JiraHelper(viewModel.DialogContext);

            var filters = viewModel.Gallifrey.JiraConnection.GetJiraFilters();
            var issues = viewModel.Gallifrey.JiraConnection.GetJiraCurrentUserOpenIssues();
            var recent = viewModel.Gallifrey.JiraTimerCollection.GetJiraReferencesForLastDays(100);
            DataContext = new SearchModel(filters, recent, issues);
        }

        private async void SearchButton(object sender, RoutedEventArgs e)
        {
            try
            {
                JiraHelperResult<IEnumerable<Issue>> searchResult = null;
                DataModel.SetIsSearching();

                Func<IEnumerable<Issue>> searchFunc;
                if (!string.IsNullOrWhiteSpace(DataModel.SearchTerm))
                {
                    var searchTerm = DataModel.SearchTerm;
                    searchFunc = () => viewModel.Gallifrey.JiraConnection.GetJiraIssuesFromSearchText(searchTerm);
                }
                else if (!string.IsNullOrWhiteSpace(DataModel.SelectedFilter))
                {
                    var searchFilter = DataModel.SelectedFilter;
                    searchFunc = () => viewModel.Gallifrey.JiraConnection.GetJiraIssuesFromFilter(searchFilter);
                }
                else
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "No Results", "Your Search Returned No Results");
                    return;
                }

                searchResult = await jiraHelper.Do(searchFunc, "Search In Progress", true, false);

                switch (searchResult.Status)
                {
                    case JiraHelperResult<IEnumerable<Issue>>.JiraHelperStatus.Cancelled:
                        DataModel.ClearSearchResults();
                        return;
                    case JiraHelperResult<IEnumerable<Issue>>.JiraHelperStatus.Errored:
                        throw new Exception();
                    default:
                        DataModel.UpdateSearchResults(searchResult.RetVal);
                        break;
                }
            }
            catch (Exception)
            {
                DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "No Results", "There Was An Error Getting Search Results");
                DataModel.ClearSearchResults();
            }
        }

        private void AddTimer(object sender, RoutedEventArgs e)
        {
            if (DataModel.SelectedSearchResult == null)
            {
                DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "No Selected Item", "You Need To Select An Item To Add A Timer For It");
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
