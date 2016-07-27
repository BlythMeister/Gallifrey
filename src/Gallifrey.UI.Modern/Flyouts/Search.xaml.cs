using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class Search
    {
        public JiraIssueDisplayModel SelectedJira { get; private set; }
        private SearchModel DataModel => (SearchModel)DataContext;
        private readonly ModelHelpers modelHelpers;
        private readonly bool openFromAdd;
        private readonly DateTime selectedDateTab;
        private readonly ProgressDialogHelper progressDialogHelper;

        public Search(ModelHelpers modelHelpers, bool openFromAdd, DateTime selectedDateTab)
        {
            this.modelHelpers = modelHelpers;
            this.openFromAdd = openFromAdd;
            this.selectedDateTab = selectedDateTab;
            InitializeComponent();
            progressDialogHelper = new ProgressDialogHelper(modelHelpers.DialogContext);

            var recent = modelHelpers.Gallifrey.JiraTimerCollection.GetJiraReferencesForLastDays(50);

            List<string> filters;
            List<Issue> issues;

            try
            {
                 filters = modelHelpers.Gallifrey.JiraConnection.GetJiraFilters().ToList();
            }
            catch (Exception)
            {
                filters = new List<string>();
            }

            try
            {
                issues = modelHelpers.Gallifrey.JiraConnection.GetJiraCurrentUserOpenIssues().ToList();
            }
            catch (Exception)
            {
                issues = new List<Issue>();
            }
            
            DataContext = new SearchModel(filters, recent, issues);
        }

        private async void SearchButton(object sender, RoutedEventArgs e)
        {
            try
            {
                DataModel.SetIsSearching();

                Func<IEnumerable<Issue>> searchFunc;
                if (!string.IsNullOrWhiteSpace(DataModel.SearchTerm))
                {
                    var searchTerm = DataModel.SearchTerm;
                    searchFunc = () => modelHelpers.Gallifrey.JiraConnection.GetJiraIssuesFromSearchText(searchTerm);
                }
                else if (!string.IsNullOrWhiteSpace(DataModel.SelectedFilter))
                {
                    var searchFilter = DataModel.SelectedFilter;
                    searchFunc = () => modelHelpers.Gallifrey.JiraConnection.GetJiraIssuesFromFilter(searchFilter);
                }
                else
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Missing Search", "Please Choose Enter Search Term Or Choose A Filter");
                    Focus();
                    DataModel.ClearSearchResults();
                    return;
                }

                var searchResult = await progressDialogHelper.Do(searchFunc, "Search In Progress", true, false);

                switch (searchResult.Status)
                {
                    case ProgressResult.JiraHelperStatus.Cancelled:
                        DataModel.ClearSearchResults();
                        return;
                    case ProgressResult.JiraHelperStatus.Errored:
                        throw new Exception();
                    case ProgressResult.JiraHelperStatus.Success:
                        if (searchResult.RetVal.Any())
                        {
                            DataModel.UpdateSearchResults(searchResult.RetVal);
                        }
                        else
                        {
                            await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "No Results", "Your Search Returned No Results");
                            Focus();
                            DataModel.ClearSearchResults();
                        }
                        break;
                }
            }
            catch (Exception)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "No Results", "There Was An Error Getting Search Results");
                Focus();
                DataModel.ClearSearchResults();
            }
        }

        private async void AddTimer(object sender, RoutedEventArgs e)
        {
            if (DataModel.SelectedSearchResult == null)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "No Selected Item", "You Need To Select An Item To Add A Timer For It");
                Focus();
                return;
            }

            modelHelpers.CloseFlyout(this);
            if (openFromAdd)
            {
                SelectedJira = DataModel.SelectedSearchResult;
            }
            else
            {
                var addFlyout = new AddTimer(modelHelpers, DataModel.SelectedSearchResult.Reference, selectedDateTab);
                await modelHelpers.OpenFlyout(addFlyout);
                if (addFlyout.AddedTimer)
                {
                    modelHelpers.SetSelectedTimer(addFlyout.NewTimerId);
                }
                else
                {
                    modelHelpers.OpenFlyout(this);
                }
            }
        }
    }
}
