using Exceptionless;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class EditTimer
    {
        private readonly ModelHelpers modelHelpers;
        private readonly ProgressDialogHelper progressDialogHelper;
        private EditTimerModel DataModel => (EditTimerModel)DataContext;
        public Guid EditedTimerId { get; set; }

        public EditTimer(ModelHelpers modelHelpers, Guid selected)
        {
            this.modelHelpers = modelHelpers;
            InitializeComponent();

            progressDialogHelper = new ProgressDialogHelper(modelHelpers.DialogContext);

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

                if (DataModel.RunDate.Value.Date < DataModel.MinDate.Date || DataModel.RunDate.Value.Date > DataModel.MaxDate.Date)
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
                if (DataModel.LocalTimer)
                {
                    var currentTimer = modelHelpers.Gallifrey.JiraTimerCollection.GetTimer(EditedTimerId);
                    if (!currentTimer.LocalTimer)
                    {
                        if (!modelHelpers.Gallifrey.Settings.InternalSettings.IsPremium)
                        {
                            var localTimersCount = modelHelpers.Gallifrey.JiraTimerCollection.GetAllLocalTimers().Count();
                            if (localTimersCount >= 2)
                            {
                                modelHelpers.ShowGetPremiumMessage("Without Gallifrey Premium You Are Limited To A Maximum Of 2 Local Timers");
                                Focus();
                                return;
                            }
                        }
                    }

                    try
                    {
                        EditedTimerId = modelHelpers.Gallifrey.JiraTimerCollection.ChangeLocalTimerDescription(EditedTimerId, DataModel.LocalTimerDescription);
                    }
                    catch (DuplicateTimerException)
                    {
                        await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Something Went Wrong", "An Error Occured Trying To Edit That Timer, Please Try Again");
                        Focus();
                        return;
                    }
                }
                else
                {
                    Issue jiraIssue;
                    var jiraRef = string.Empty;
                    try
                    {
                        jiraRef = DataModel.JiraReference;
                        var jiraDownloadResult = await progressDialogHelper.Do(() => modelHelpers.Gallifrey.JiraConnection.GetJiraIssue(jiraRef), "Searching For Jira Issue", true, true);

                        if (jiraDownloadResult.Status == ProgressResult.JiraHelperStatus.Success)
                        {
                            jiraIssue = jiraDownloadResult.RetVal;
                        }
                        else
                        {
                            Focus();
                            return;
                        }
                    }
                    catch (NoResultsFoundException ex)
                    {
                        ExceptionlessClient.Default.SubmitException(ex);
                        await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Jira", $"Unable To Locate The Jira '{jiraRef}'");
                        Focus();
                        return;
                    }

                    var result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Correct Jira?", $"Jira found!\n\nRef: {jiraIssue.key}\nName: {jiraIssue.fields.summary}\n\nIs that correct?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

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
            }

            if (DataModel.HasModifiedTime)
            {
                var originalTime = new TimeSpan(DataModel.OriginalHours, DataModel.OriginalMinutes, 0);
                var newTime = new TimeSpan(DataModel.Hours ?? 0, DataModel.Minutes ?? 0, 0);
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

            var result = await DialogCoordinator.Instance.ShowInputAsync(modelHelpers.DialogContext, "Enter Time Adjustment", "Enter The Number Of Minutes or Time To Subtract\nThis Can Be '90' or '1:30' for 1 Hour & 30 Minutes");

            if (result != null)
            {
                DoAdjustment(result, false);
            }
            Focus();
        }

        private async void AddTime(object sender, RoutedEventArgs e)
        {
            DataModel.SetNotDefaultButton();

            var result = await DialogCoordinator.Instance.ShowInputAsync(modelHelpers.DialogContext, "Enter Time Adjustment", "Enter The Number Of Minutes Or Time To Add\nThis Can Be '90' or '1:30' for 1 Hour & 30 Minutes");

            if (result != null)
            {
                DoAdjustment(result, true);
            }
            Focus();
        }

        private async void DoAdjustment(string enteredValue, bool addTime)
        {
            TimeSpan adjustmentTimespan;
            if (enteredValue.Contains(":"))
            {
                if (!TimeSpan.TryParse(enteredValue, out adjustmentTimespan))
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Time Entry", $"The Value '{enteredValue}' was not a valid time representation (e.g. '1:30').");
                    Focus();
                    return;
                }
            }
            else
            {
                if (!int.TryParse(enteredValue, out var minutesAdjustment))
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Time Entry", $"The Value '{enteredValue}' was not a number of minutes.");
                    Focus();
                    return;
                }

                adjustmentTimespan = TimeSpan.FromMinutes(minutesAdjustment);
            }

            DataModel.AdjustTime(adjustmentTimespan, addTime);
        }

        private async Task<bool> MergeTimers(DuplicateTimerException ex)
        {
            var duplicateTimerMerge = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Timer Already Exists", "This Timer Already Exists On That With That Name On That Date, Would You Like To Merge Them?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

            if (duplicateTimerMerge == MessageDialogResult.Negative)
            {
                return false;
            }

            modelHelpers.Gallifrey.JiraTimerCollection.AdjustTime(ex.TimerId, DataModel.OriginalHours, DataModel.OriginalMinutes, true);
            modelHelpers.Gallifrey.JiraTimerCollection.RemoveTimer(EditedTimerId);
            EditedTimerId = ex.TimerId;
            return true;
        }

        private async void SearchButton(object sender, RoutedEventArgs e)
        {
            modelHelpers.HideFlyout(this);
            var searchFlyout = new Search(modelHelpers, openFromEdit: true);
            await modelHelpers.OpenFlyout(searchFlyout);
            if (searchFlyout.SelectedJira != null)
            {
                DataModel.SetJiraReference(searchFlyout.SelectedJira.Reference);
            }
            await modelHelpers.OpenFlyout(this);
        }
    }
}
