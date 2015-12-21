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
        private AddTimerModel DataModel => (AddTimerModel)DataContext;
        public bool AddedTimer { get; set; }
        public Guid NewTimerId { get; set; }

        public AddTimer(MainViewModel viewModel, string jiraRef = null, DateTime? startDate = null, bool? enableDateChange = null, TimeSpan? preloadTime = null, bool? enableTimeChange = null, bool? startNow = null)
        {
            this.viewModel = viewModel;
            InitializeComponent();

            DataContext = new AddTimerModel(viewModel.Gallifrey, jiraRef, startDate, enableDateChange, preloadTime, enableTimeChange, startNow);
            AddedTimer = false;
        }

        private async void AddButton(object sender, RoutedEventArgs e)
        {
            if (!DataModel.StartDate.HasValue)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "Missing Date", "You Must Enter A Start Date");
                Focus();
                return;
            }

            if (DataModel.StartDate.Value < DataModel.MinDate || DataModel.StartDate.Value > DataModel.MaxDate)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "Invalid Date", $"You Must Enter A Start Date Between {DataModel.MinDate.ToShortDateString()} And {DataModel.MaxDate.ToShortDateString()}");
                Focus();
                return;
            }

            Issue jiraIssue;
            try
            {
                jiraIssue = viewModel.Gallifrey.JiraConnection.GetJiraIssue(DataModel.JiraReference);
            }
            catch (NoResultsFoundException)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "Invalid Jira", "Unable To Locate The Jira");
                Focus();
                return;
            }

            if (DataModel.JiraReferenceEditable)
            {
                var result = await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "Correct Jira?", $"Jira found!\n\nRef: {jiraIssue.key}\nName: {jiraIssue.fields.summary}\n\nIs that correct?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                if (result == MessageDialogResult.Negative)
                {
                    Focus();
                    return;
                }
            }

            var seedTime = new TimeSpan(DataModel.StartHours, DataModel.StartMinutes, 0);
            try
            {
                NewTimerId = viewModel.Gallifrey.JiraTimerCollection.AddTimer(jiraIssue, DataModel.StartDate.Value, seedTime, DataModel.StartNow);
                AddedTimer = true;
            }
            catch (DuplicateTimerException ex)
            {
                var doneSomething = false;
                if (seedTime.TotalMinutes > 0)
                {
                    var result = await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "Duplicate Timer", "The Timer Already Exists, Would You Like To Add The Time?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                    if (result == MessageDialogResult.Affirmative)
                    {
                        viewModel.Gallifrey.JiraTimerCollection.AdjustTime(ex.TimerId, seedTime.Hours, seedTime.Minutes, true);
                        doneSomething = true;
                    }
                    else
                    {
                        Focus();
                        return;
                    }
                }

                if (DataModel.StartNow)
                {
                    viewModel.Gallifrey.JiraTimerCollection.StartTimer(ex.TimerId);
                    doneSomething = true;
                }

                if (!doneSomething)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "Duplicate Timer", "This Timer Already Exists!");
                    Focus();
                    return;
                }

                NewTimerId = ex.TimerId;
            }

            if (DataModel.AssignToMe)
            {
                try
                {
                    viewModel.Gallifrey.JiraConnection.AssignToCurrentUser(DataModel.JiraReference);
                }
                catch (JiraConnectionException)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "Assign Jira Error", "Unable To Locate Assign Jira To Current User");
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
                    await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "Error Changing Status", "Unable To Set Issue As In Progress");
                }
            }

            viewModel.RefreshModel();
            viewModel.SetSelectedTimer(NewTimerId);
            AddedTimer = true;
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
                    IsOpen = true;
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
