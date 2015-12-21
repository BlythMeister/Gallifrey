using System;
using System.Windows;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for EditTimer.xaml
    /// </summary>
    public partial class EditTimer
    {
        private readonly MainViewModel viewModel;
        private EditTimerModel DataModel => (EditTimerModel)DataContext;
        public Guid EditedTimerId { get; set; }

        public EditTimer(MainViewModel viewModel, Guid selected)
        {
            this.viewModel = viewModel;
            InitializeComponent();

            EditedTimerId = selected;
            DataContext = new EditTimerModel(viewModel.Gallifrey, EditedTimerId);
        }

        private async void SaveButton(object sender, RoutedEventArgs e)
        {
            if (DataModel.HasModifiedRunDate())
            {
                if (!DataModel.RunDate.HasValue)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext,"Missing Date", "You Must Enter A Start Date");
                    Focus();
                    return;
                }

                if (DataModel.RunDate.Value < DataModel.MinDate || DataModel.RunDate.Value > DataModel.MaxDate)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext,"Invalid Date", $"You Must Enter A Start Date Between {DataModel.MinDate.ToShortDateString()} And {DataModel.MaxDate.ToShortDateString()}");
                    Focus();
                    return;
                }

                try
                {
                    EditedTimerId = viewModel.Gallifrey.JiraTimerCollection.ChangeTimerDate(EditedTimerId, DataModel.RunDate.Value);
                }
                catch (Exception)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext,"Duplicate Timer", "This Timer Already Exists On That Date!");
                    Focus();
                    return;
                }
            }

            if (DataModel.HasModifiedJiraReference())
            {
                Issue jiraIssue;
                try
                {
                    jiraIssue = viewModel.Gallifrey.JiraConnection.GetJiraIssue(DataModel.JiraReference);
                }
                catch (NoResultsFoundException)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext,"Invalid Jira", "Unable To Locate The Jira");
                    Focus();
                    return;
                }

                var result = await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext,"Correct Jira?", $"Jira found!\n\nRef: {jiraIssue.key}\nName: {jiraIssue.fields.summary}\n\nIs that correct?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                if (result == MessageDialogResult.Negative)
                {
                    return;
                }

                try
                {
                    EditedTimerId = viewModel.Gallifrey.JiraTimerCollection.RenameTimer(EditedTimerId, jiraIssue);
                }
                catch (Exception)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext,"Duplicate Timer", "This Timer Already Exists On That Date!");
                    Focus();
                    return;
                }
            }

            if (DataModel.HasModifiedTime())
            {
                var originalTime = new TimeSpan(DataModel.OriginalHours, DataModel.OriginalMinutes, 0);
                var newTime = new TimeSpan(DataModel.Hours, DataModel.Minutes, 0);
                var difference = newTime.Subtract(originalTime);
                var addTime = difference.TotalSeconds > 0;

                viewModel.Gallifrey.JiraTimerCollection.AdjustTime(EditedTimerId, Math.Abs(difference.Hours), Math.Abs(difference.Minutes), addTime);
            }

            viewModel.RefreshModel();
            viewModel.SetSelectedTimer(EditedTimerId);
            IsOpen = false;
        }

        private async void SubtractTime(object sender, RoutedEventArgs e)
        {
            DataModel.SetNotDefaultButton();

            var result = await DialogCoordinator.Instance.ShowInputAsync(viewModel.DialogContext, "Enter Time Adjustment", "Enter The Number Of Minutes To Subtract\nThis Can Be 90 for 1 Hour & 30 Minutes");

            if (result == null)
            {
                Focus();
                return;
            }

            int minutesAdjustment;
            if (!int.TryParse(result, out minutesAdjustment))
            {
                await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "Invalid Time Entry", $"The Value '{result}' was not a number of minutes.");
                Focus();
                return;
            }

            DataModel.AdjustTime(TimeSpan.FromMinutes(minutesAdjustment), false);
        }

        private async void AddTime(object sender, RoutedEventArgs e)
        {
            DataModel.SetNotDefaultButton();

            var result = await DialogCoordinator.Instance.ShowInputAsync(viewModel.DialogContext, "Enter Time Adjustment", "Enter The Number Of Minutes To Add\nThis Can Be 90 for 1 Hour & 30 Minutes");

            if (result == null)
            {
                Focus();
                return;
            }

            int minutesAdjustment;
            if (!int.TryParse(result, out minutesAdjustment))
            {
                await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "Invalid Time Entry", $"The Value '{result}' was not a number of minutes.");
                Focus();
                return;
            }

            DataModel.AdjustTime(TimeSpan.FromMinutes(minutesAdjustment), true);
        }
    }
}
