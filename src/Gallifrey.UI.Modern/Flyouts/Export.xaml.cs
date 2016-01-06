using System;
using System.Threading.Tasks;
using System.Windows;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class Export
    {
        private readonly ModelHelpers modelHelpers;
        private ExportModel DataModel => (ExportModel)DataContext;
        private readonly JiraHelper jiraHelper;

        public Export(ModelHelpers modelHelpers, Guid timerId, TimeSpan? exportTime)
        {
            this.modelHelpers = modelHelpers;
            InitializeComponent();
            jiraHelper = new JiraHelper(modelHelpers.DialogContext);
            SetupContext(timerId, exportTime);
        }

        private async void SetupContext(Guid timerId, TimeSpan? exportTime)
        {
            var timerToShow = modelHelpers.Gallifrey.JiraTimerCollection.GetTimer(timerId);
            Issue jiraIssue = null;

            DataContext = new ExportModel(timerToShow, exportTime, modelHelpers.Gallifrey.Settings.ExportSettings.DefaultRemainingValue);

            var requireRefresh = !timerToShow.LastJiraTimeCheck.HasValue || timerToShow.LastJiraTimeCheck < DateTime.UtcNow.AddMinutes(-15);

            var showError = false;
            try
            {
                var jiraDownloadResult = await jiraHelper.Do(() => modelHelpers.Gallifrey.JiraConnection.GetJiraIssue(timerToShow.JiraReference, requireRefresh), "Downloading Jira Work Logs To Ensure Accurate Export", true, false);

                switch (jiraDownloadResult.Status)
                {
                    case JiraHelperResult<Issue>.JiraHelperStatus.Cancelled:
                        modelHelpers.CloseFlyout(this);
                        return;
                    case JiraHelperResult<Issue>.JiraHelperStatus.Errored:
                        showError = true;
                        break;
                    case JiraHelperResult<Issue>.JiraHelperStatus.Success:
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

            if (requireRefresh)
            {
                modelHelpers.Gallifrey.JiraTimerCollection.RefreshFromJira(timerId, jiraIssue, modelHelpers.Gallifrey.JiraConnection.CurrentUser);

                timerToShow = modelHelpers.Gallifrey.JiraTimerCollection.GetTimer(timerId);
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

            DataModel.UpdateTimer(timerToShow, jiraIssue);
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
                await jiraHelper.Do(() => modelHelpers.Gallifrey.JiraConnection.LogTime(jiraRef, date, toExport, strategy, comment, remaining), "Exporting Time To Jira", false, true);
            }
            catch (WorkLogException)
            {
                dialog = DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Error Exporting", "Unable To Log Work!");
            }
            catch (StateChangedException)
            {
                dialog = DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Error Exporting", "Unable To Re-Close A The Jira, Manually Check!!");
            }

            if (dialog != null)
            {
                await dialog;
                Focus();
                return;
            }

            modelHelpers.Gallifrey.JiraTimerCollection.AddJiraExportedTime(DataModel.Timer.UniqueId, DataModel.ToExportHours, DataModel.ToExportMinutes);
            modelHelpers.CloseFlyout(this);
        }
    }
}
