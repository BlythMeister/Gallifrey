using Gallifrey.Comparers;
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
    public partial class BulkExport
    {
        private readonly ModelHelpers modelHelpers;
        private BulkExportContainerModel DataModel => (BulkExportContainerModel)DataContext;
        private readonly ProgressDialogHelper progressDialogHelper;
        private bool lastChangeStatusSent;
        private string currentChangeStatusJiraRef = string.Empty;

        public BulkExport(ModelHelpers modelHelpers, List<JiraTimer> timers)
        {
            this.modelHelpers = modelHelpers;
            InitializeComponent();
            progressDialogHelper = new ProgressDialogHelper(modelHelpers);
            DataContext = new BulkExportContainerModel();
            SetupContext(timers);
        }

        private async void SetupContext(List<JiraTimer> timers, List<BulkExportModel> oldModels = null)
        {
            await Task.Delay(50);
            modelHelpers.HideFlyout(this);
            var timersToShow = new List<BulkExportModel>();
            try
            {
                var jiraDownloadResult = await progressDialogHelper.Do(controller => GetTimers(timers), "Downloading Jira Work Logs To Ensure Accurate Export", true, true);

                switch (jiraDownloadResult.Status)
                {
                    case ProgressResult.JiraHelperStatus.Cancelled:
                        modelHelpers.CloseFlyout(this);
                        return;

                    case ProgressResult.JiraHelperStatus.Success:
                        timersToShow = jiraDownloadResult.RetVal;
                        break;
                }
            }
            catch (BulkExportException ex)
            {
                await modelHelpers.ShowMessageAsync("Unable To Locate Jira", ex.Message);
                modelHelpers.CloseFlyout(this);
                return;
            }

            if (!timersToShow.Any())
            {
                await modelHelpers.ShowMessageAsync("Nothing To Export", "There Is No Time To Export");
                modelHelpers.CloseFlyout(this);
            }
            else if (timersToShow.Count == 1)
            {
                modelHelpers.CloseFlyout(this);
                await modelHelpers.OpenFlyout(new Export(modelHelpers, timersToShow.First().Timer.UniqueId, null, true));
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

                await modelHelpers.OpenFlyout(this);
            }
        }

        private async void ExportButton(object sender, RoutedEventArgs e)
        {
            var timersToExport = DataModel.BulkExports.Where(x => x.ShouldExport).Reverse().ToList();
            foreach (var exportModel in timersToExport)
            {
                if (exportModel.Timer.TimeToExport < exportModel.ToExport)
                {
                    await modelHelpers.ShowMessageAsync($"Invalid Export Timer - {exportModel.JiraRef}", $"You Cannot Export More Than The Timer States Un-Exported\nThis Value Is {exportModel.ToExport:hh\\:mm}!");
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
                await modelHelpers.ShowMessageAsync("Error Exporting", $"{ex.Message}");
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

            var changeStatusExports = timersToExport.Where(x => x.ChangeStatus).ToList();
            if (changeStatusExports.Any())
            {
                await modelHelpers.OpenFlyout(this);
                foreach (var timer in changeStatusExports)
                {
                    while (!string.IsNullOrWhiteSpace(currentChangeStatusJiraRef))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    currentChangeStatusJiraRef = timer.JiraRef;
                    try
                    {
                        var transitionsAvailable = modelHelpers.Gallifrey.JiraConnection.GetTransitions(timer.JiraRef);

                        var timeSelectorDialog = (BaseMetroDialog)Resources["TransitionSelector"];
                        await modelHelpers.ShowDialogAsync(timeSelectorDialog);

                        var comboBox = timeSelectorDialog.FindChild<ComboBox>("Items");
                        comboBox.ItemsSource = transitionsAvailable.Select(x => x.name).ToList();

                        var messageBox = timeSelectorDialog.FindChild<TextBlock>("Message");
                        messageBox.Text = $"Please Select The Status Update You Would Like To Perform To {timer.JiraRef}";
                    }
                    catch (Exception)
                    {
                        await modelHelpers.ShowMessageAsync("Status Update Error", "Unable To Change The Status Of This Issue");
                    }
                }
                lastChangeStatusSent = true;
            }

            modelHelpers.CloseFlyout(this);
        }

        private void ExportAllButton(object sender, RoutedEventArgs e)
        {
            foreach (var bulkExportModel in DataModel.BulkExports)
            {
                bulkExportModel.ShouldExport = true;
            }
        }

        private List<BulkExportModel> GetTimers(List<JiraTimer> timers)
        {
            var timersToShow = new List<BulkExportModel>();
            var issuesRetrieved = new List<Issue>();
            var timersToGet = timers.Where(x => !x.LocalTimer && !x.IsRunning).ToList();

            var dates = new List<DateTime>();
            var references = new List<string>();

            foreach (var timerToShow in timersToGet)
            {
                var jiraIssue = issuesRetrieved.FirstOrDefault(x => x.key == timerToShow.JiraReference);

                if (jiraIssue == null)
                {
                    try
                    {
                        if (modelHelpers.Gallifrey.JiraConnection.DoesJiraExist(timerToShow.JiraReference))
                        {
                            jiraIssue = modelHelpers.Gallifrey.JiraConnection.GetJiraIssue(timerToShow.JiraReference);
                            issuesRetrieved.Add(jiraIssue);
                        }
                        else
                        {
                            throw new BulkExportException($"Unable To Locate Jira {timerToShow.JiraReference}!\nCannot Export Time\nPlease Verify/Correct Jira Reference");
                        }
                    }
                    catch (BulkExportException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        throw new BulkExportException($"Unable To Locate Jira {timerToShow.JiraReference}!\nCannot Export Time\nPlease Verify/Correct Jira Reference");
                    }
                }

                dates.Add(timerToShow.DateStarted.Date);
                references.Add(timerToShow.JiraReference);
            }

            List<StandardWorkLog> logs;
            try
            {
                logs = modelHelpers.Gallifrey.JiraConnection.GetWorkLoggedForDatesFilteredIssues(dates.Distinct(), references.Distinct()).ToList();
            }
            catch (Exception)
            {
                throw new BulkExportException("Unable To Get WorkLogs!\nCannot Export Time");
            }

            foreach (var timerToShow in timersToGet)
            {
                var jiraIssue = issuesRetrieved.FirstOrDefault(x => x.key == timerToShow.JiraReference);

                var time = TimeSpan.Zero;
                foreach (var standardWorkLog in logs.Where(x => x.JiraRef == timerToShow.JiraReference && x.LoggedDate.Date == timerToShow.DateStarted.Date))
                {
                    time = time.Add(standardWorkLog.TimeSpent);
                }
                modelHelpers.Gallifrey.JiraTimerCollection.RefreshFromJira(timerToShow.UniqueId, jiraIssue, time);

                var updatedTimer = modelHelpers.Gallifrey.JiraTimerCollection.GetTimer(timerToShow.UniqueId);
                if (!updatedTimer.FullyExported)
                {
                    var model = new BulkExportModel(updatedTimer, jiraIssue, modelHelpers.Gallifrey.Settings.ExportSettings);
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

                dialogController.SetMessage(i == 0 ? $"Exporting Timer {exportModel.JiraRef} For {exportModel.ExportDate.Date:ddd, dd MMM}" : $"Exporting Timer {exportModel.JiraRef} For {exportModel.ExportDate.Date:ddd, dd MMM}\nDone {i} Of {timersToExport.Count}");

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
                    if (ex.InnerException != null)
                    {
                        throw new BulkExportException($"Error Logging Work To {exportModel.JiraRef}\n\nError Message From Jira: { ex.InnerException.Message }");
                    }

                    throw new BulkExportException($"Error Logging Work To {exportModel.JiraRef}");
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
                var dialog = (BaseMetroDialog)Resources["TransitionSelector"];
                var comboBox = dialog.FindChild<ComboBox>("Items");

                selectedTransition = (string)comboBox.SelectedItem;

                await modelHelpers.HideDialogAsync(dialog);

                modelHelpers.Gallifrey.JiraConnection.TransitionIssue(currentChangeStatusJiraRef, selectedTransition);
            }
            catch (StateChangedException)
            {
                if (string.IsNullOrWhiteSpace(selectedTransition))
                {
                    await modelHelpers.ShowMessageAsync("Status Update Error", "Unable To Change The Status Of This Issue");
                }
                else
                {
                    await modelHelpers.ShowMessageAsync("Status Update Error", $"Unable To Change The Status Of This Issue To {selectedTransition}");
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
            var dialog = (BaseMetroDialog)Resources["TransitionSelector"];
            await modelHelpers.HideDialogAsync(dialog);

            currentChangeStatusJiraRef = string.Empty;
            if (lastChangeStatusSent)
            {
                modelHelpers.CloseFlyout(this);
            }
        }
    }
}
