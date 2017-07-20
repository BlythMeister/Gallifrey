using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Jira.Model;
using Gallifrey.JiraTimers;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class Export
    {
        private readonly ModelHelpers modelHelpers;
        private ExportModel DataModel => (ExportModel)DataContext;
        private readonly ProgressDialogHelper progressDialogHelper;

        public Export(ModelHelpers modelHelpers, Guid timerId, TimeSpan? exportTime, bool skipJiraCheck = false)
        {
            this.modelHelpers = modelHelpers;
            InitializeComponent();
            progressDialogHelper = new ProgressDialogHelper(modelHelpers.DialogContext);
            SetupContext(modelHelpers.Gallifrey.JiraTimerCollection.GetTimer(timerId), exportTime, skipJiraCheck);
        }

        private async void SetupContext(JiraTimer timerToShow, TimeSpan? exportTime, bool skipJiraCheck)
        {
            await Task.Delay(50);
            modelHelpers.HideFlyout(this);
            if (timerToShow.LocalTimer)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Local Timer", "You Cannot Export A Local Timer!");
                modelHelpers.CloseFlyout(this);
                return;
            }

            DataContext = new ExportModel(timerToShow, exportTime, modelHelpers.Gallifrey.Settings.ExportSettings);

            if (!skipJiraCheck) //Previously loaded bulk export
            {
                Issue jiraIssue = null;
                var showError = false;
                try
                {
                    var jiraDownloadResult = await progressDialogHelper.Do(() => modelHelpers.Gallifrey.JiraConnection.GetJiraIssue(timerToShow.JiraReference), "Downloading Jira Work Logs To Ensure Accurate Export", true, false);

                    switch (jiraDownloadResult.Status)
                    {
                        case ProgressResult.JiraHelperStatus.Cancelled:
                            modelHelpers.CloseFlyout(this);
                            return;
                        case ProgressResult.JiraHelperStatus.Errored:
                            showError = true;
                            break;
                        case ProgressResult.JiraHelperStatus.Success:
                            jiraIssue = jiraDownloadResult.RetVal;
                            break;
                    }
                }
                catch (Exception)
                {
                    showError = true;
                }

                if (showError)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Unable To Locate Jira", $"Unable To Locate Jira {timerToShow.JiraReference}!\nCannot Export Time\nPlease Verify/Correct Jira Reference");
                    modelHelpers.CloseFlyout(this);
                    return;
                }

                if (!timerToShow.LastJiraTimeCheck.HasValue || timerToShow.LastJiraTimeCheck < DateTime.UtcNow.AddMinutes(-5))
                {
                    var logs = modelHelpers.Gallifrey.JiraConnection.GetWorkLoggedForDatesFilteredIssues(new List<DateTime> { timerToShow.DateStarted }, new List<string> { timerToShow.JiraReference });
                    var time = TimeSpan.Zero;
                    foreach (var standardWorkLog in logs.Where(x => x.JiraRef == timerToShow.JiraReference && x.LoggedDate.Date == timerToShow.DateStarted.Date))
                    {
                        time = time.Add(standardWorkLog.TimeSpent);
                    }
                    modelHelpers.Gallifrey.JiraTimerCollection.RefreshFromJira(timerToShow.UniqueId, jiraIssue, time);
                    timerToShow = modelHelpers.Gallifrey.JiraTimerCollection.GetTimer(timerToShow.UniqueId);
                }

                DataModel.UpdateTimer(timerToShow, jiraIssue);
            }

            if (timerToShow.FullyExported)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Nothing To Export", "There Is No Time To Export");
                modelHelpers.CloseFlyout(this);
                return;
            }

            if (timerToShow.IsRunning)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Timer Is Running", "You Cannot Export A Timer While It Is Running");
                modelHelpers.CloseFlyout(this);
                return;
            }

            await modelHelpers.OpenFlyout(this);
        }

        private async void ExportButton(object sender, RoutedEventArgs e)
        {
            if (DataModel.Timer.TimeToExport < DataModel.ToExport)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Export", $"You Cannot Export More Than The Timer States Un-Exported\nThis Value Is {DataModel.ToExport.ToString(@"hh\:mm")}!");
                return;
            }

            Task<MessageDialogResult> dialog = null;
            try
            {
                var jiraRef = DataModel.JiraRef;
                var date = DataModel.ExportDate;
                var toExport = DataModel.ToExport;
                var strategy = DataModel.WorkLogStrategy;
                var comment = DataModel.Comment;
                var remaining = DataModel.Remaining;
                var standardComment = DataModel.StandardComment;
                var result = await progressDialogHelper.Do(() => modelHelpers.Gallifrey.JiraConnection.LogTime(jiraRef, date, toExport, strategy, standardComment, comment, remaining), "Exporting Time To Jira", false, true);
                if (result.Status == ProgressResult.JiraHelperStatus.Success)
                {
                    modelHelpers.Gallifrey.JiraTimerCollection.AddJiraExportedTime(DataModel.Timer.UniqueId, DataModel.ToExportHours ?? 0, DataModel.ToExportMinutes ?? 0);
                    if (DataModel.ChangeStatus)
                    {
                        try
                        {
                            var transitionsAvaliable = modelHelpers.Gallifrey.JiraConnection.GetTransitions(DataModel.JiraRef);

                            var timeSelectorDialog = (BaseMetroDialog)this.Resources["TransitionSelector"];
                            await DialogCoordinator.Instance.ShowMetroDialogAsync(modelHelpers.DialogContext, timeSelectorDialog);

                            var comboBox = timeSelectorDialog.FindChild<ComboBox>("Items");
                            comboBox.ItemsSource = transitionsAvaliable.Select(x => x.name).ToList();

                            var messageBox = timeSelectorDialog.FindChild<TextBlock>("Message");
                            messageBox.Text = $"Please Select The Status Update You Would Like To Perform To {jiraRef}";
                        }
                        catch (Exception)
                        {
                            await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Status Update Error", "Unable To Change The Status Of This Issue");
                        }
                    }
                    else
                    {
                        modelHelpers.CloseFlyout(this);
                    }
                }
                else
                {
                    throw new WorkLogException("Did not export");
                }
            }
            catch (WorkLogException ex)
            {
                if (ex.InnerException != null)
                {
                    dialog = DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Error Exporting", $"Unable To Log Work!\nError Message From Jira: {ex.InnerException.Message}");
                }
                else
                {
                    dialog = DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Error Exporting", "Unable To Log Work!");
                }
            }
            catch (CommentException)
            {
                dialog = DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Error Adding Comment", "The Comment Was Not Added");
            }

            if (dialog != null)
            {
                await dialog;
                DataModel.Timer.ClearLastJiraCheck();
                SetupContext(DataModel.Timer, DataModel.ToExportMaxTime, false);
                Focus();
            }
        }

        private async void TransitionSelected(object sender, RoutedEventArgs e)
        {
            var selectedTransition = string.Empty;

            try
            {
                var dialog = (BaseMetroDialog)this.Resources["TransitionSelector"];
                var comboBox = dialog.FindChild<ComboBox>("Items");

                selectedTransition = (string)comboBox.SelectedItem;

                await DialogCoordinator.Instance.HideMetroDialogAsync(modelHelpers.DialogContext, dialog);

                modelHelpers.Gallifrey.JiraConnection.TransitionIssue(DataModel.JiraRef, selectedTransition);
            }
            catch (StateChangedException)
            {
                if (string.IsNullOrWhiteSpace(selectedTransition))
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Status Update Error", "Unable To Change The Status Of This Issue");
                }
                else
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Status Update Error", $"Unable To Change The Status Of This Issue To {selectedTransition}");
                }
            }

            modelHelpers.CloseFlyout(this);
        }

        private async void CancelTransition(object sender, RoutedEventArgs e)
        {
            var dialog = (BaseMetroDialog)this.Resources["TransitionSelector"];
            await DialogCoordinator.Instance.HideMetroDialogAsync(modelHelpers.DialogContext, dialog);

            modelHelpers.CloseFlyout(this);
        }

        private void ChangeStatusClick(object sender, RoutedEventArgs e)
        {
            if (!modelHelpers.Gallifrey.Settings.InternalSettings.IsPremium)
            {
                if (DataModel.ChangeStatus)
                {
                    modelHelpers.ShowGetPremiumMessage("Without Gallifrey Premium You Cannot Change Jira Status.");
                    DataModel.ChangeStatus = false;
                    Focus();
                }
            }
        }
    }
}
