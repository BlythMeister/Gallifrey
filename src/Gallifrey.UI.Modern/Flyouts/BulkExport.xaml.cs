using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Gallifrey.Comparers;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Jira.Model;
using Gallifrey.JiraTimers;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class BulkExport
    {
        private readonly ModelHelpers modelHelpers;
        private BulkExportContainerModel DataModel => (BulkExportContainerModel)DataContext;
        private readonly ProgressDialogHelper progressDialogHelper;
        private const int NonPremiumMaxExport = 5;
        private bool lastChangeStatusSent;
        private string currentChangeStatusJiraRef = string.Empty;

        public BulkExport(ModelHelpers modelHelpers, List<JiraTimer> timers)
        {
            this.modelHelpers = modelHelpers;
            InitializeComponent();
            progressDialogHelper = new ProgressDialogHelper(modelHelpers.DialogContext);
            DataContext = new BulkExportContainerModel();
            SetupContext(timers);
        }

        private async void SetupContext(List<JiraTimer> timers, List<BulkExportModel> oldModels = null)
        {
            await Task.Delay(50);
            modelHelpers.HideFlyout(this);
            var timersToShow = new List<BulkExportModel>();
            var showError = false;
            try
            {
                var jiraDownloadResult = await progressDialogHelper.Do(controller => GetTimers(controller, timers), "Downloading Jira Work Logs To Ensure Accurate Export", true, true);

                switch (jiraDownloadResult.Status)
                {
                    case ProgressResult.JiraHelperStatus.Cancelled:
                        modelHelpers.CloseHiddenFlyout(this);
                        return;
                    case ProgressResult.JiraHelperStatus.Errored:
                        await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Unable To Locate Jira", "There Was An Error Getting Work Logs");
                        showError = true;
                        break;
                    case ProgressResult.JiraHelperStatus.Success:
                        timersToShow = jiraDownloadResult.RetVal;
                        break;
                }
            }
            catch (BulkExportException ex)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Unable To Locate Jira", ex.Message);
                showError = true;
            }

            if (showError)
            {
                modelHelpers.CloseHiddenFlyout(this);
                return;
            }

            if (!timersToShow.Any())
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Nothing To Export", "There Is No Time To Export");
                modelHelpers.CloseHiddenFlyout(this);
            }
            else if (timersToShow.Count == 1)
            {
                modelHelpers.CloseHiddenFlyout(this);
                modelHelpers.OpenFlyout(new Export(modelHelpers, timersToShow.First().Timer.UniqueId, null, true));
            }
            else
            {
                var jiraComparer = new JiraReferenceComparer();
                timersToShow.Sort((a, b) =>
                {
                    int cmp = b.ExportDate.Date.CompareTo(a.ExportDate.Date);
                    if (cmp == 0)
                    {
                        cmp = jiraComparer.Compare(a.JiraRef, b.JiraRef);
                    }
                    return cmp;
                });
                timersToShow.ForEach(x => DataModel.BulkExports.Add(x));
                if (oldModels != null)
                {
                    foreach (var oldModel in oldModels)
                    {
                        foreach (var newModel in DataModel.BulkExports)
                        {
                            if (oldModel.JiraRef == newModel.JiraRef && oldModel.ExportDate.Date == newModel.ExportDate.Date)
                            {
                                newModel.ShouldExport = oldModel.ShouldExport;
                                newModel.ToExportHours = oldModel.ToExportHours;
                                newModel.ToExportMinutes = oldModel.ToExportMinutes;
                                newModel.WorkLogStrategy = oldModel.WorkLogStrategy;
                                newModel.RemainingHours = oldModel.RemainingHours;
                                newModel.RemainingMinutes = oldModel.RemainingMinutes;
                                newModel.Comment = oldModel.Comment;
                                newModel.StandardComment = oldModel.StandardComment;
                                newModel.ChangeStatus = oldModel.ChangeStatus;
                            }
                        }
                    }
                }

                modelHelpers.OpenFlyout(this);
            }
        }

        private async void ExportButton(object sender, RoutedEventArgs e)
        {
            var timersToExport = DataModel.BulkExports.Where(x => x.ShouldExport).Reverse().ToList();
            foreach (var exportModel in timersToExport)
            {
                if (exportModel.Timer.TimeToExport < exportModel.ToExport)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, $"Invalid Export Timer - {exportModel.JiraRef}", $"You Cannot Export More Than The Timer States Un-Exported\nThis Value Is {exportModel.ToExport.ToString(@"hh\:mm")}!");
                    return;
                }
            }

            try
            {
                modelHelpers.HideFlyout(this);
                await progressDialogHelper.Do(controller => DoExport(controller, timersToExport), "Exporting Selected Timers", false, true);
            }
            catch (BulkExportException ex)
            {
                modelHelpers.OpenFlyout(this);
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Error Exporting", $"{ex.Message}");
                foreach (var timer in timersToExport.Where(x => !x.Timer.FullyExported))
                {
                    timer.Timer.ClearLastJiraCheck();
                }

                var timersToShow = DataModel.BulkExports.Where(bulkExportModel => !bulkExportModel.Timer.FullyExported).ToList();
                DataModel.BulkExports.Clear();
                SetupContext(timersToShow.Select(x => x.Timer).ToList(), timersToShow);
                Focus();
                return;
            }

            var changeStatusExports = timersToExport.Where(x => x.ChangeStatus);
            if (changeStatusExports.Any())
            {
                modelHelpers.OpenFlyout(this);
                foreach (var timer in changeStatusExports)
                {
                    while (!string.IsNullOrWhiteSpace(currentChangeStatusJiraRef))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    currentChangeStatusJiraRef = timer.JiraRef;
                    try
                    {
                        var transitionsAvaliable = modelHelpers.Gallifrey.JiraConnection.GetTransitions(timer.JiraRef);

                        var timeSelectorDialog = (BaseMetroDialog)this.Resources["TransitionSelector"];
                        await DialogCoordinator.Instance.ShowMetroDialogAsync(modelHelpers.DialogContext, timeSelectorDialog);

                        var comboBox = timeSelectorDialog.FindChild<ComboBox>("Items");
                        comboBox.ItemsSource = transitionsAvaliable.Select(x => x.name).ToList();

                        var messageBox = timeSelectorDialog.FindChild<TextBlock>("Message");
                        messageBox.Text = $"Please Select The Status Update You Would Like To Perform To {timer.JiraRef}";
                    }
                    catch (Exception)
                    {
                        await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Status Update Error", "Unable To Change The Status Of This Issue");
                    }
                }
                lastChangeStatusSent = true;
            }

            modelHelpers.CloseFlyout(this);
        }

        private void ExportAllButton(object sender, RoutedEventArgs e)
        {
            if (modelHelpers.Gallifrey.Settings.InternalSettings.IsPremium || DataModel.BulkExports.Count <= NonPremiumMaxExport)
            {
                foreach (var bulkExportModel in DataModel.BulkExports)
                {
                    bulkExportModel.ShouldExport = true;
                }
            }
            else
            {
                modelHelpers.ShowGetPremiumMessage("Without Gallifrey Premium You Are Limited To A Maximum Of 5 Bulk Exports");
                for (int i = 0; i < NonPremiumMaxExport; i++)
                {
                    DataModel.BulkExports[i].ShouldExport = true;
                }
            }
        }

        private void ExportSingleClick(object sender, RoutedEventArgs e)
        {
            if (modelHelpers.Gallifrey.Settings.InternalSettings.IsPremium || DataModel.BulkExports.Count(x => x.ShouldExport) <= NonPremiumMaxExport) return;

            modelHelpers.ShowGetPremiumMessage("Without Gallifrey Premium You Are Limited To A Maximum Of 5 Bulk Exports");
            var checkBox = (CheckBox)sender;
            checkBox.IsChecked = false;
        }

        private List<BulkExportModel> GetTimers(ProgressDialogController dialogController, List<JiraTimer> timers)
        {
            var timersToShow = new List<BulkExportModel>();
            var issuesRetrieved = new List<Issue>();
            var timersToGet = timers.Where(x => !x.LocalTimer && !x.IsRunning).ToList();

            for (var i = 0; i < timersToGet.Count; i++)
            {
                var timerToShow = timersToGet[i];

                var requireRefresh = !timerToShow.LastJiraTimeCheck.HasValue || timerToShow.LastJiraTimeCheck < DateTime.UtcNow.AddMinutes(-15);
                var model = new BulkExportModel(timerToShow, modelHelpers.Gallifrey.Settings.ExportSettings);
                var jiraIssue = issuesRetrieved.FirstOrDefault(x => x.key == timerToShow.JiraReference);

                if (i == 0)
                {
                    dialogController.SetMessage($"Downloading Jira Work Logs For {timerToShow.JiraReference} To Ensure Accurate Export");
                }
                else
                {
                    dialogController.SetMessage($"Downloading Jira Work Logs For {timerToShow.JiraReference} To Ensure Accurate Export\nDone {i} Of {timersToGet.Count}");
                }

                if (jiraIssue == null)
                {
                    try
                    {
                        jiraIssue = modelHelpers.Gallifrey.JiraConnection.GetJiraIssue(timerToShow.JiraReference, requireRefresh);
                        issuesRetrieved.Add(jiraIssue);
                    }
                    catch (Exception)
                    {
                        throw new BulkExportException($"Unable To Locate Jira {timerToShow.JiraReference}!\nCannot Export Time\nPlease Verify/Correct Jira Reference");
                    }
                }

                if (requireRefresh)
                {
                    modelHelpers.Gallifrey.JiraTimerCollection.RefreshFromJira(timerToShow.UniqueId, jiraIssue, modelHelpers.Gallifrey.JiraConnection.CurrentUser);
                    timerToShow = modelHelpers.Gallifrey.JiraTimerCollection.GetTimer(timerToShow.UniqueId);
                }

                if (!timerToShow.FullyExported)
                {
                    model.UpdateTimer(timerToShow, jiraIssue);
                    timersToShow.Add(model);
                }
            }

            return timersToShow;
        }

        private void DoExport(ProgressDialogController dialogController, List<BulkExportModel> timersToExport)
        {
            for (var i = 0; i < timersToExport.Count; i++)
            {
                var exportModel = timersToExport[i];

                if (i == 0)
                {
                    dialogController.SetMessage($"Exporting Timer {exportModel.JiraRef} For {exportModel.ExportDate.Date.ToString("ddd, dd MMM")}");
                }
                else
                {
                    dialogController.SetMessage($"Exporting Timer {exportModel.JiraRef} For {exportModel.ExportDate.Date.ToString("ddd, dd MMM")}\nDone {i} Of {timersToExport.Count}");
                }

                try
                {
                    var jiraRef = exportModel.JiraRef;
                    var date = exportModel.ExportDate;
                    var toExport = exportModel.ToExport;
                    var strategy = exportModel.WorkLogStrategy;
                    var comment = exportModel.Comment;
                    var remaining = exportModel.Remaining;
                    var standardComment = exportModel.StandardComment;
                    modelHelpers.Gallifrey.JiraConnection.LogTime(jiraRef, date, toExport, strategy, standardComment, comment, remaining);
                    modelHelpers.Gallifrey.JiraTimerCollection.AddJiraExportedTime(exportModel.Timer.UniqueId, exportModel.ToExportHours ?? 0, exportModel.ToExportMinutes ?? 0);
                }
                catch (WorkLogException ex)
                {
                    throw new BulkExportException($"Error Logging Work To {exportModel.JiraRef}\n\nError Message From Jira: { ex.InnerException.Message }");
                }
                catch (CommentException)
                {
                    throw new BulkExportException($"The Comment Was Not Added To Jira {exportModel.JiraRef}");
                }
            }
        }

        private class BulkExportException : Exception
        {
            public BulkExportException(string message) : base(message)
            {
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

                modelHelpers.Gallifrey.JiraConnection.TransitionIssue(currentChangeStatusJiraRef, selectedTransition);
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

            currentChangeStatusJiraRef = string.Empty;
            if (lastChangeStatusSent)
            {
                modelHelpers.CloseFlyout(this);
            }
        }

        private async void CancelTransition(object sender, RoutedEventArgs e)
        {
            var dialog = (BaseMetroDialog)this.Resources["TransitionSelector"];
            await DialogCoordinator.Instance.HideMetroDialogAsync(modelHelpers.DialogContext, dialog);

            currentChangeStatusJiraRef = string.Empty;
            if (lastChangeStatusSent)
            {
                modelHelpers.CloseFlyout(this);
            }
        }

        private void ChangeStatusClick(object sender, RoutedEventArgs e)
        {
            if (!modelHelpers.Gallifrey.Settings.InternalSettings.IsPremium)
            {
                var changeStatusTicked = DataModel.BulkExports.Where(x => x.ChangeStatus).ToList();

                if (changeStatusTicked.Any())
                {
                    modelHelpers.ShowGetPremiumMessage("Without Gallifrey Premium You Cannot Change Jira Status.");
                    foreach (var bulkExportModel in changeStatusTicked)
                    {
                        bulkExportModel.ChangeStatus = false;
                    }
                    Focus();
                }
            }
        }
    }
}
