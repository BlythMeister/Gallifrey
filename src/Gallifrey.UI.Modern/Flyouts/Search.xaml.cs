using Gallifrey.Jira.Model;
using Gallifrey.JiraIntegration;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class Search
    {
        public JiraIssueDisplayModel SelectedJira { get; private set; }
        private SearchModel DataModel => (SearchModel)DataContext;
        private readonly ModelHelpers modelHelpers;
        private readonly bool openFromAdd;
        private readonly bool openFromEdit;
        private readonly DateTime selectedDateTab;
        private readonly ProgressDialogHelper progressDialogHelper;

        public Search(ModelHelpers modelHelpers, bool openFromAdd = false, bool openFromEdit = false, DateTime? selectedDateTab = null)
        {
            this.modelHelpers = modelHelpers;
            this.openFromAdd = openFromAdd;
            this.openFromEdit = openFromEdit;
            this.selectedDateTab = selectedDateTab ?? DateTime.Now.Date;
            InitializeComponent();
            progressDialogHelper = new ProgressDialogHelper(modelHelpers.DialogContext);

            LoadSearch();
        }

        private async void LoadSearch()
        {
            Func<SearchModel> getSearchModel = () =>
            {
                var recent = new List<RecentJira>();
                var filters = new List<string>();
                var issues = new List<Issue>();

                try
                {
                    recent = modelHelpers.Gallifrey.JiraTimerCollection.GetJiraReferencesForLastDays(50).ToList();
                }
                catch (Exception)
                {
                    //Ignore
                }

                try
                {
                    filters = modelHelpers.Gallifrey.JiraConnection.GetJiraFilters().ToList();
                }
                catch (Exception)
                {
                    //Ignore
                }

                try
                {
                    issues = modelHelpers.Gallifrey.JiraConnection.GetJiraCurrentUserOpenIssues().ToList();
                }
                catch (Exception)
                {
                    //Ignore
                }

                return new SearchModel(filters, recent, issues, openFromEdit);
            };

            var result = await progressDialogHelper.Do(getSearchModel, "Loading Search Information", true, false);

            switch (result.Status)
            {
                case ProgressResult.JiraHelperStatus.Cancelled:
                    modelHelpers.CloseFlyout(this);
                    break;
                case ProgressResult.JiraHelperStatus.Errored:
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Error", "There Was An Error Loading Default Search Results");
                    modelHelpers.CloseFlyout(this);
                    break;
                case ProgressResult.JiraHelperStatus.Success:
                    DataContext = result.RetVal;
                    break;
            }
        }

        private async void SearchButton(object sender, RoutedEventArgs e)
        {
            try
            {
                DataModel.SetIsSearching();

                Func<IEnumerable<Issue>> searchFunc;

                if (!string.IsNullOrWhiteSpace(DataModel.SearchTerm) && !string.IsNullOrWhiteSpace(DataModel.SelectedFilter))
                {
                    var searchTerm = DataModel.SearchTerm;
                    var searchFilter = DataModel.SelectedFilter;
                    searchFunc = () =>
                    {
                        var searchFilterResults = modelHelpers.Gallifrey.JiraConnection.GetJiraIssuesFromFilter(searchFilter).ToList();
                        var filteredToSearchText = modelHelpers.Gallifrey.JiraConnection.GetJiraIssuesFromSearchTextLimitedToKeys(searchTerm, searchFilterResults.Select(x => x.key));
                        return searchFilterResults.Where(x => filteredToSearchText.Any(f => f.key == x.key));
                    };
                }
                else if (!string.IsNullOrWhiteSpace(DataModel.SearchTerm))
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
            if (openFromAdd || openFromEdit)
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
                    await modelHelpers.OpenFlyout(this);
                }
            }
        }
    }
}
