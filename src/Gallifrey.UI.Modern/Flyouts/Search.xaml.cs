using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
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
            var model = (SearchModel)DataContext;
            Task<IEnumerable<Issue>> searchTask = null;
            model.SetIsSearching();
            var cancellationTokenSource = new CancellationTokenSource();

            if (!string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                searchTask = Task.Factory.StartNew(() => viewModel.Gallifrey.JiraConnection.GetJiraIssuesFromSearchText(model.SearchTerm), cancellationTokenSource.Token);
            }
            else if (!string.IsNullOrWhiteSpace(model.SelectedFilter))
            {
                searchTask = Task.Factory.StartNew(() => viewModel.Gallifrey.JiraConnection.GetJiraIssuesFromFilter(model.SelectedFilter), cancellationTokenSource.Token);
            }
            else
            {
                viewModel.MainWindow.ShowMessageAsync("No Results", "Your Search Returned No Results");
            }

            if (searchTask != null)
            {
                try
                {
                    var controller = await viewModel.MainWindow.ShowProgressAsync("Please Wait", "Search In Progress", true);
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
                            model.UpdateSearchResults(searchTask.Result);
                        }
                        else
                        {
                            viewModel.MainWindow.ShowMessageAsync("No Results", "There Was An Error Getting Search Results");
                            model.ClearSearchResults();
                        }
                    }
                    else
                    {
                        model.ClearSearchResults();
                    }
                }
                catch (Exception)
                {
                    viewModel.MainWindow.ShowMessageAsync("No Results", "There Was An Error Getting Search Results");
                    model.ClearSearchResults();
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
}
