using System;
using System.Windows;
using System.Windows.Controls;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for AddTimer.xaml
    /// </summary>
    public partial class AddTimer
    {
        private readonly MainViewModel viewModel;
        private AddTimerModel DataModel { get { return (AddTimerModel)DataContext; } }

        public AddTimer(MainViewModel viewModel, string jiraRef = null, DateTime? startDate = null, bool? enableDateChange = null, TimeSpan? preloadTime = null, bool? startNow = null)
        {
            this.viewModel = viewModel;
            InitializeComponent();
            DataContext = new AddTimerModel(viewModel.Gallifrey, jiraRef, startDate, enableDateChange, preloadTime, startNow);
        }

        private async void AddButton(object sender, RoutedEventArgs e)
        {
            if (!DataModel.StartDate.HasValue)
            {
                DialogCoordinator.Instance.ShowMessageAsync(viewModel, "Missing Date", "You Must Enter A Start Date");
                return;
            }

            if (DataModel.StartDate.Value < DataModel.MinDate || DataModel.StartDate.Value > DataModel.MaxDate)
            {
                DialogCoordinator.Instance.ShowMessageAsync(viewModel, "Invalid Date", string.Format("You Must Enter A Start Date Between {0} And {1}", DataModel.MinDate.ToShortDateString(), DataModel.MaxDate.ToShortDateString()));
                return;
            }

            Issue jiraIssue;
            try
            {
                jiraIssue = viewModel.Gallifrey.JiraConnection.GetJiraIssue(DataModel.JiraReference);
            }
            catch (NoResultsFoundException)
            {
                DialogCoordinator.Instance.ShowMessageAsync(viewModel, "Invalid Jira", "Unable To Locate The Jira");
                return;
            }

            if (DataModel.JiraReferenceEditable)
            {
                var result = await DialogCoordinator.Instance.ShowMessageAsync(viewModel, "Correct Jira?", string.Format("Jira found!\n\nRef: {0}\nName: {1}\n\nIs that correct?", jiraIssue.key, jiraIssue.fields.summary), MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

                if (result == MessageDialogResult.Negative)
                {
                    return;
                }
            }

            Guid NewTimerId;
            try
            {
                var seedTime = new TimeSpan(DataModel.StartHours, DataModel.StartMinutes, 0);
                NewTimerId = viewModel.Gallifrey.JiraTimerCollection.AddTimer(jiraIssue, DataModel.StartDate.Value, seedTime, DataModel.StartNow);
            }
            catch (DuplicateTimerException)
            {
                DialogCoordinator.Instance.ShowMessageAsync(viewModel, "Duplicate Timer", "This Timer Already Exists!");
                return;
            }

            if (DataModel.AssignToMe)
            {
                try
                {
                    viewModel.Gallifrey.JiraConnection.AssignToCurrentUser(DataModel.JiraReference);
                }
                catch (JiraConnectionException)
                {
                    DialogCoordinator.Instance.ShowMessageAsync(viewModel, "Assign Jira Error", "Unable To Locate Assign Jira To Current User");
                }
            }

            if (DataModel.InProgress)
            {
                try
                {
                    viewModel.Gallifrey.JiraConnection.SetInProgress(DataModel.JiraReference);
                }
                catch (StateChangedException)
                {
                    DialogCoordinator.Instance.ShowMessageAsync(viewModel, "Error Changing Status", "Unable To Set Issue As In Progress");
                }
            }

            viewModel.RefreshModel();
            viewModel.SetSelectedTimer(NewTimerId);
            IsOpen = false;
        }

        private void SearchButton(object sender, RoutedEventArgs e)
        {
            var searchFlyout = new Search(viewModel, true);
            
            searchFlyout.IsOpenChanged += (o, args) =>
            {
                if (!searchFlyout.IsOpen)
                {
                    if (searchFlyout.SelectedJira != null)
                    {
                        DataModel.SetJiraReference(searchFlyout.SelectedJira.Reference);
                    }
                }
            };
            
            viewModel.OpenFlyout(searchFlyout);
            
        }

        private void StartDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataModel.StartDate.HasValue && DataModel.StartDate.Value.Date != DateTime.Now.Date)
            {
                DataModel.SetStartNowEnabled(false);
            }
            else
            {
                DataModel.SetStartNowEnabled(true);
            }
        }
    }
}
