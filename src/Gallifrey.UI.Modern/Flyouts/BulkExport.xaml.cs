using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Jira.Model;
using Gallifrey.JiraTimers;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;
using Gallifrey.Comparers;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class BulkExport
    {
        private readonly ModelHelpers modelHelpers;
        private BulkExportContainerModel DataModel => (BulkExportContainerModel)DataContext;
        private readonly ProgressDialogHelper progressDialogHelper;

        public BulkExport(ModelHelpers modelHelpers, List<JiraTimer> timers)
        {
            this.modelHelpers = modelHelpers;
            InitializeComponent();
            progressDialogHelper = new ProgressDialogHelper(modelHelpers.DialogContext);
            DataContext = new BulkExportContainerModel();
            SetupContext(timers);
        }

        private async void SetupContext(List<JiraTimer> timers)
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
                modelHelpers.OpenFlyout(this);
            }
        }

        private async void ExportButton(object sender, RoutedEventArgs e)
        {
            var timersToExport = DataModel.BulkExports.Where(x => x.ShouldExport).ToList();
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
                await progressDialogHelper.Do(controller => DoExport(controller, timersToExport), "Exporting Selected Timers", false, true);
            }
            catch (BulkExportException ex)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Error Exporting", ex.Message);

                var timersToShow = DataModel.BulkExports.Where(bulkExportModel => !bulkExportModel.Timer.FullyExported).ToList();
                DataModel.BulkExports.Clear();
                timersToShow.ForEach(x => DataModel.BulkExports.Add(x));
                Focus();
                return;
            }

            modelHelpers.CloseFlyout(this);
        }

        private List<BulkExportModel> GetTimers(ProgressDialogController dialogController, List<JiraTimer> timers)
        {
            var timersToShow = new List<BulkExportModel>();
            var issuesRetrieved = new List<Issue>();
            var timersToGet = timers.Where(x => !x.TempTimer && !x.IsRunning).ToList();
            
            for (var i = 0; i < timersToGet.Count; i++)
            {
                var timerToShow = timersToGet[i];

                var requireRefresh = !timerToShow.LastJiraTimeCheck.HasValue || timerToShow.LastJiraTimeCheck < DateTime.UtcNow.AddMinutes(-15);
                var model = new BulkExportModel(timerToShow, modelHelpers.Gallifrey.Settings.ExportSettings.DefaultRemainingValue);
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
                    modelHelpers.Gallifrey.JiraConnection.LogTime(jiraRef, date, toExport, strategy, comment, remaining);
                    modelHelpers.Gallifrey.JiraTimerCollection.AddJiraExportedTime(exportModel.Timer.UniqueId, exportModel.ToExportHours, exportModel.ToExportMinutes);
                }
                catch (WorkLogException)
                {
                    throw new BulkExportException($"Error Logging Work To {exportModel.JiraRef}");
                }
                catch (StateChangedException)
                {
                    throw new BulkExportException($"Unable To Re - Close A The Jira  {exportModel.JiraRef}, Manually Check!!");
                }
            }
        }

        private class BulkExportException : Exception
        {
            public BulkExportException(string message) : base(message)
            {
            }
        }
    }
}
