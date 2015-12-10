using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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
        private EditTimerModel DataModel { get { return (EditTimerModel)DataContext; } }
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
                    DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext,"Missing Date", "You Must Enter A Start Date");
                    return;
                }

                if (DataModel.RunDate.Value < DataModel.MinDate || DataModel.RunDate.Value > DataModel.MaxDate)
                {
                    DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext,"Invalid Date", string.Format("You Must Enter A Start Date Between {0} And {1}", DataModel.MinDate.ToShortDateString(), DataModel.MaxDate.ToShortDateString()));
                    return;
                }

                try
                {
                    EditedTimerId = viewModel.Gallifrey.JiraTimerCollection.ChangeTimerDate(EditedTimerId, DataModel.RunDate.Value);
                }
                catch (Exception)
                {
                    DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext,"Duplicate Timer", "This Timer Already Exists On That Date!");
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
                    DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext,"Invalid Jira", "Unable To Locate The Jira");
                    return;
                }

                var result = await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext,"Correct Jira?", string.Format("Jira found!\n\nRef: {0}\nName: {1}\n\nIs that correct?", jiraIssue.key, jiraIssue.fields.summary), MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

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
                    DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext,"Duplicate Timer", "This Timer Already Exists On That Date!");
                    return;
                }
            }

            if (DataModel.HasModifiedTime())
            {
                var orignalTime = new TimeSpan(DataModel.OriginalHours, DataModel.OriginalMinutes, 0);
                var newTime = new TimeSpan(DataModel.Hours, DataModel.Minutes, 0);
                var difference = newTime.Subtract(orignalTime);
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
                return;
            }

            int minutesAdjustment;
            if (!int.TryParse(result, out minutesAdjustment))
            {
                await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "Invalid Time Entry", string.Format("The Value '{0}' was not a number of minutes.", result));
                return;
            }

            var timeAdjustmentTimespan = TimeSpan.FromMinutes(minutesAdjustment);
            viewModel.Gallifrey.JiraTimerCollection.AdjustTime(EditedTimerId, timeAdjustmentTimespan.Hours, timeAdjustmentTimespan.Minutes, false);
            DataContext = new EditTimerModel(viewModel.Gallifrey, EditedTimerId);
        }

        private async void AddTime(object sender, RoutedEventArgs e)
        {
            DataModel.SetNotDefaultButton();

            var result = await DialogCoordinator.Instance.ShowInputAsync(viewModel.DialogContext, "Enter Time Adjustment", "Enter The Number Of Minutes To Add\nThis Can Be 90 for 1 Hour & 30 Minutes");

            if (result == null)
            {
                return;
            }

            int minutesAdjustment;
            if (!int.TryParse(result, out minutesAdjustment))
            {
                await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "Invalid Time Entry", string.Format("The Value '{0}' was not a number of minutes.", result));
                return;
            }

            var timeAdjustmentTimespan = TimeSpan.FromMinutes(minutesAdjustment);
            viewModel.Gallifrey.JiraTimerCollection.AdjustTime(EditedTimerId, timeAdjustmentTimespan.Hours, timeAdjustmentTimespan.Minutes, true);
            DataContext = new EditTimerModel(viewModel.Gallifrey, EditedTimerId);
        }
    }
}
