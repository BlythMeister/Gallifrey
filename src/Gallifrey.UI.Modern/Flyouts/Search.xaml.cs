using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Models;

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
            Task<IEnumerable<Issue>> searchTask = null;
            DataModel.SetIsSearching();
            var cancellationTokenSource = new CancellationTokenSource();

            //TODO This shouldn't do task factory!
            if (!string.IsNullOrWhiteSpace(DataModel.SearchTerm))
            {
                searchTask = Task.Factory.StartNew(() => viewModel.Gallifrey.JiraConnection.GetJiraIssuesFromSearchText(DataModel.SearchTerm), cancellationTokenSource.Token);
            }
            else if (!string.IsNullOrWhiteSpace(DataModel.SelectedFilter))
            {
                searchTask = Task.Factory.StartNew(() => viewModel.Gallifrey.JiraConnection.GetJiraIssuesFromFilter(DataModel.SelectedFilter), cancellationTokenSource.Token);
            }
            else
            {
                viewModel.DialogCoordinator.ShowMessageAsync(viewModel,"No Results", "Your Search Returned No Results");
            }

            if (searchTask != null)
            {
                try
                {
                    var controller = await viewModel.DialogCoordinator.ShowProgressAsync(viewModel, "Please Wait", "Search In Progress", true);
                    var controllerCancel = Task.Factory.StartNew(() => 
                    {
                        while (!controller.IsCanceled)
                        {
                            
                        }
                    });

                    var cancelled = false;
                    if (await Task.WhenAny(searchTask, controllerCancel) == controllerCancel)
                    {
                        cancellationTokenSource.Cancel();
                        cancelled = true;
                    }

                    await controller.CloseAsync();

                    if (!cancelled)
                    {
                        if (searchTask.IsCompleted)
                        {
                            DataModel.UpdateSearchResults(searchTask.Result);
                        }
                        else
                        {
                            throw new Exception("NoResults");
                        }
                    }
                    else
                    {
                        DataModel.ClearSearchResults();
                    }
                }
                catch (Exception)
                {
                    viewModel.DialogCoordinator.ShowMessageAsync(viewModel,"No Results", "There Was An Error Getting Search Results");
                    DataModel.ClearSearchResults();
                }
            }
        }

        private void AddTimer(object sender, RoutedEventArgs e)
        {
            if (DataModel.SelectedSearchResult == null)
            {
                viewModel.DialogCoordinator.ShowMessageAsync(viewModel,"No Selected Item", "You Need To Select An Item To Add A Timer For It");
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
