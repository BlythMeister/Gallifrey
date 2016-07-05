using System;
using System.Threading.Tasks;
using System.Windows;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Jira.Model;
using Gallifrey.JiraTimers;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

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
            if (timerToShow.TempTimer)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Temp Timer", "You Cannot Export A Temporary Timer!");
                modelHelpers.CloseHiddenFlyout(this);
                return;
            }

            DataContext = new ExportModel(timerToShow, exportTime, modelHelpers.Gallifrey.Settings.ExportSettings);

            if (!skipJiraCheck)
            {
                Issue jiraIssue = null;
                var requireRefresh = !timerToShow.LastJiraTimeCheck.HasValue || timerToShow.LastJiraTimeCheck < DateTime.UtcNow.AddMinutes(-15);
                var showError = false;
                try
                {
                    var jiraDownloadResult = await progressDialogHelper.Do(() => modelHelpers.Gallifrey.JiraConnection.GetJiraIssue(timerToShow.JiraReference, requireRefresh), "Downloading Jira Work Logs To Ensure Accurate Export", true, false);

                    switch (jiraDownloadResult.Status)
                    {
                        case ProgressResult.JiraHelperStatus.Cancelled:
                            modelHelpers.CloseHiddenFlyout(this);
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
                    modelHelpers.CloseHiddenFlyout(this);
                    return;
                }

                if (requireRefresh)
                {
                    modelHelpers.Gallifrey.JiraTimerCollection.RefreshFromJira(timerToShow.UniqueId, jiraIssue, modelHelpers.Gallifrey.JiraConnection.CurrentUser);
                    timerToShow = modelHelpers.Gallifrey.JiraTimerCollection.GetTimer(timerToShow.UniqueId);
                }

                DataModel.UpdateTimer(timerToShow, jiraIssue);
            }

            if (timerToShow.FullyExported)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Nothing To Export", "There Is No Time To Export");
                modelHelpers.CloseHiddenFlyout(this);
                return;
            }

            if (timerToShow.IsRunning)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Timer Is Running", "You Cannot Export A Timer While It Is Running");
                modelHelpers.CloseHiddenFlyout(this);
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
                    modelHelpers.CloseFlyout(this);
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
                Focus();
            }
        }
    }
}
