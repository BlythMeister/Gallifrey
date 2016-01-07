using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class EditTimer
    {
        private readonly ModelHelpers modelHelpers;
        private EditTimerModel DataModel => (EditTimerModel) DataContext;
        public Guid EditedTimerId { get; set; }

        public EditTimer(ModelHelpers modelHelpers, Guid selected)
        {
            this.modelHelpers = modelHelpers;
            InitializeComponent();

            EditedTimerId = selected;
            DataContext = new EditTimerModel(modelHelpers.Gallifrey, EditedTimerId);
        }

        private async void SaveButton(object sender, RoutedEventArgs e)
        {
            if (DataModel.HasModifiedRunDate)
            {
                if (!DataModel.RunDate.HasValue)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Missing Date", "You Must Enter A Start Date");
                    Focus();
                    return;
                }

                if (DataModel.RunDate.Value < DataModel.MinDate || DataModel.RunDate.Value > DataModel.MaxDate)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Date", $"You Must Enter A Start Date Between {DataModel.MinDate.ToShortDateString()} And {DataModel.MaxDate.ToShortDateString()}");
                    Focus();
                    return;
                }

                try
                {
                    EditedTimerId = modelHelpers.Gallifrey.JiraTimerCollection.ChangeTimerDate(EditedTimerId, DataModel.RunDate.Value);
                }
                catch (DuplicateTimerException ex)
                {
                    var handlerd = await MergeTimers(ex);
                    if (!handlerd)
                    {
                        Focus();
                        return;
                    }
                }
            }
            else if (DataModel.HasModifiedJiraReference)
            {
                Issue jiraIssue;
                try
                {
                    jiraIssue = modelHelpers.Gallifrey.JiraConnection.GetJiraIssue(DataModel.JiraReference);
                }
                catch (NoResultsFoundException)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Jira", "Unable To Locate The Jira");
                    Focus();
                    return;
                }

                var result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Correct Jira?", $"Jira found!\n\nRef: {jiraIssue.key}\nName: {jiraIssue.fields.summary}\n\nIs that correct?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative});

                if (result == MessageDialogResult.Negative)
                {
                    return;
                }

                try
                {
                    EditedTimerId = modelHelpers.Gallifrey.JiraTimerCollection.RenameTimer(EditedTimerId, jiraIssue);
                }
                catch (DuplicateTimerException ex)
                {
                    var handlerd = await MergeTimers(ex);
                    if (!handlerd)
                    {
                        Focus();
                        return;
                    }
                }
            }

            if (DataModel.HasModifiedTime)
            {
                var originalTime = new TimeSpan(DataModel.OriginalHours, DataModel.OriginalMinutes, 0);
                var newTime = new TimeSpan(DataModel.Hours, DataModel.Minutes, 0);
                var difference = newTime.Subtract(originalTime);
                var addTime = difference.TotalSeconds > 0;

                modelHelpers.Gallifrey.JiraTimerCollection.AdjustTime(EditedTimerId, Math.Abs(difference.Hours), Math.Abs(difference.Minutes), addTime);
            }

            modelHelpers.RefreshModel();
            modelHelpers.SetSelectedTimer(EditedTimerId);
            modelHelpers.CloseFlyout(this);
        }

        private async void SubtractTime(object sender, RoutedEventArgs e)
        {
            DataModel.SetNotDefaultButton();

            var result = await DialogCoordinator.Instance.ShowInputAsync(modelHelpers.DialogContext, "Enter Time Adjustment", "Enter The Number Of Minutes To Subtract\nThis Can Be 90 for 1 Hour & 30 Minutes");

            if (result == null)
            {
                Focus();
                return;
            }

            int minutesAdjustment;
            if (!int.TryParse(result, out minutesAdjustment))
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Time Entry", $"The Value '{result}' was not a number of minutes.");
                Focus();
                return;
            }

            DataModel.AdjustTime(TimeSpan.FromMinutes(minutesAdjustment), false);
        }

        private async void AddTime(object sender, RoutedEventArgs e)
        {
            DataModel.SetNotDefaultButton();

            var result = await DialogCoordinator.Instance.ShowInputAsync(modelHelpers.DialogContext, "Enter Time Adjustment", "Enter The Number Of Minutes To Add\nThis Can Be 90 for 1 Hour & 30 Minutes");

            if (result == null)
            {
                Focus();
                return;
            }

            int minutesAdjustment;
            if (!int.TryParse(result, out minutesAdjustment))
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Time Entry", $"The Value '{result}' was not a number of minutes.");
                Focus();
                return;
            }

            DataModel.AdjustTime(TimeSpan.FromMinutes(minutesAdjustment), true);
        }

        private async Task<bool> MergeTimers(DuplicateTimerException ex)
        {
            var duplicateTimerMerge = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Timer Already Exists", "This Timer Already Exists On That With That Name On That Date, Would You Like To Merge Them?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative});

            if (duplicateTimerMerge == MessageDialogResult.Negative)
            {
                return false;
            }

            modelHelpers.Gallifrey.JiraTimerCollection.AdjustTime(ex.TimerId, DataModel.OriginalHours, DataModel.OriginalMinutes, true);
            modelHelpers.Gallifrey.JiraTimerCollection.RemoveTimer(EditedTimerId);
            EditedTimerId = ex.TimerId;
            return true;
        }
    }
}
