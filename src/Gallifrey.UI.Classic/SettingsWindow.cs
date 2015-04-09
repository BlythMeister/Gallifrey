using System;
using System.Linq;
using System.Windows.Forms;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Settings;

namespace Gallifrey.UI.Classic
{
    public partial class SettingsWindow : Form
    {
        private readonly IBackend gallifrey;

        public SettingsWindow(IBackend gallifrey)
        {
            this.gallifrey = gallifrey;
            InitializeComponent();

            if (gallifrey.Settings.JiraConnectionSettings.JiraUrl != null) txtJiraUrl.Text = gallifrey.Settings.JiraConnectionSettings.JiraUrl;
            if (gallifrey.Settings.JiraConnectionSettings.JiraUsername != null) txtJiraUsername.Text = gallifrey.Settings.JiraConnectionSettings.JiraUsername;
            if (gallifrey.Settings.JiraConnectionSettings.JiraPassword != null) txtJiraPassword.Text = gallifrey.Settings.JiraConnectionSettings.JiraPassword;
            chkUseRest.Checked = !gallifrey.Settings.JiraConnectionSettings.JiraUseSoapApi;
            
            chkAlert.Checked = gallifrey.Settings.AppSettings.AlertWhenNotRunning;
            chkAutoUpdate.Checked = gallifrey.Settings.AppSettings.AutoUpdate;
            txtAlertMins.Text = ((gallifrey.Settings.AppSettings.AlertTimeMilliseconds / 1000) / 60).ToString();
            txtTimerDays.Text = gallifrey.Settings.AppSettings.KeepTimersForDays.ToString();
            chkUsageTracking.Checked = gallifrey.Settings.AppSettings.UsageTracking;

            cmdDefaultExport.SelectedIndex = (int)gallifrey.Settings.ExportSettings.DefaultRemainingValue;
            txtCommentPrefix.Text = gallifrey.Settings.ExportSettings.ExportCommentPrefix;
            txtDefaultComment.Text = gallifrey.Settings.ExportSettings.EmptyExportComment;

            txtTargetHours.Text = gallifrey.Settings.AppSettings.TargetLogPerDay.Hours.ToString();
            txtTargetMins.Text = gallifrey.Settings.AppSettings.TargetLogPerDay.Minutes.ToString();

            for (var i = 0; i < chklstWorkingDays.Items.Count; i++)
            {
                var foundItem = gallifrey.Settings.AppSettings.ExportDays.Any(exportDay => exportDay.ToString().ToLower() == chklstWorkingDays.Items[i].ToString().ToLower());

                chklstWorkingDays.SetItemChecked(i, foundItem);
            }

            if (gallifrey.Settings.ExportSettings.ExportPrompt == null)
            {
                gallifrey.Settings.ExportSettings.ExportPrompt = new ExportPrompt();
            }

            for (var i = 0; i < chklstExportPrompt.Items.Count; i++)
            {
                switch (chklstExportPrompt.Items[i].ToString())
                {
                    case "Add Idle Time":
                        chklstExportPrompt.SetItemChecked(i, gallifrey.Settings.ExportSettings.ExportPrompt.OnAddIdle);
                        break;
                    case "Manual Timer Adjustment":
                        chklstExportPrompt.SetItemChecked(i, gallifrey.Settings.ExportSettings.ExportPrompt.OnManualAdjust);
                        break;
                    case "Stop Timer":
                        chklstExportPrompt.SetItemChecked(i, gallifrey.Settings.ExportSettings.ExportPrompt.OnStop);
                        break;
                    case "Add Pre-Loaded Timer":
                        chklstExportPrompt.SetItemChecked(i, gallifrey.Settings.ExportSettings.ExportPrompt.OnCreatePreloaded);
                        break;
                }
            }
            chkExportAll.Checked = gallifrey.Settings.ExportSettings.ExportPromptAll;

            cmdWeekStart.Text = gallifrey.Settings.AppSettings.StartOfWeek.ToString();

            chkAlwaysTop.Checked = gallifrey.Settings.UiSettings.AlwaysOnTop;
        }

        private void btnCancelEditSettings_Click(object sender, EventArgs e)
        {
            CloseWindow(true);
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            TopMost = false;

            int keepTimerDays, alertTime;
            if (!int.TryParse(txtAlertMins.Text, out alertTime)) alertTime = 0;
            if (!int.TryParse(txtTimerDays.Text, out keepTimerDays)) keepTimerDays = 28;

            gallifrey.Settings.AppSettings.AlertWhenNotRunning = chkAlert.Checked;
            gallifrey.Settings.AppSettings.AlertTimeMilliseconds = (alertTime * 60) * 1000;
            gallifrey.Settings.AppSettings.KeepTimersForDays = keepTimerDays;
            gallifrey.Settings.AppSettings.AutoUpdate = chkAutoUpdate.Checked;
            gallifrey.Settings.UiSettings.AlwaysOnTop = chkAlwaysTop.Checked;
            gallifrey.Settings.AppSettings.UsageTracking = chkUsageTracking.Checked; 

            gallifrey.Settings.ExportSettings.DefaultRemainingValue = (DefaultRemaining)cmdDefaultExport.SelectedIndex;
            gallifrey.Settings.ExportSettings.ExportCommentPrefix = txtCommentPrefix.Text;
            gallifrey.Settings.ExportSettings.EmptyExportComment = txtDefaultComment.Text;

            int hours, minutes;

            if (!int.TryParse(txtTargetHours.Text, out hours)) { hours = 7; }
            if (!int.TryParse(txtTargetMins.Text, out minutes)) { minutes = 30; }

            gallifrey.Settings.AppSettings.TargetLogPerDay = new TimeSpan(hours, minutes, 0);

            gallifrey.Settings.AppSettings.ExportDays = (from object t in chklstWorkingDays.CheckedItems select (DayOfWeek)Enum.Parse(typeof(DayOfWeek), t.ToString(), true));
            gallifrey.Settings.AppSettings.StartOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), cmdWeekStart.Text, true);

            for (var i = 0; i < chklstExportPrompt.Items.Count; i++)
            {
                switch (chklstExportPrompt.Items[i].ToString())
                {
                    case "Add Idle Time":
                        gallifrey.Settings.ExportSettings.ExportPrompt.OnAddIdle = chklstExportPrompt.GetItemChecked(i);
                        break;
                    case "Manual Timer Adjustment":
                        gallifrey.Settings.ExportSettings.ExportPrompt.OnManualAdjust = chklstExportPrompt.GetItemChecked(i);
                        break;
                    case "Stop Timers":
                        gallifrey.Settings.ExportSettings.ExportPrompt.OnStop = chklstExportPrompt.GetItemChecked(i);
                        break;
                    case "Add Pre-Loaded Timer":
                        gallifrey.Settings.ExportSettings.ExportPrompt.OnCreatePreloaded = chklstExportPrompt.GetItemChecked(i);
                        break;
                }
            }
            gallifrey.Settings.ExportSettings.ExportPromptAll = chkExportAll.Checked;

            gallifrey.Settings.JiraConnectionSettings.JiraUrl = txtJiraUrl.Text;
            gallifrey.Settings.JiraConnectionSettings.JiraUsername = txtJiraUsername.Text;
            gallifrey.Settings.JiraConnectionSettings.JiraPassword = txtJiraPassword.Text;
            gallifrey.Settings.JiraConnectionSettings.JiraUseSoapApi = !chkUseRest.Checked;

            TopMost = true;
            CloseWindow(false);
        }

        private void CloseWindow(bool cancelClicked)
        {
            TopMost = false;

            var settingsWork = false;

            try
            {
                gallifrey.SaveSettings(true);
                settingsWork = true;
            }
            catch (JiraConnectionException)
            {
                if (!HandleInvalidDetails(false));
            }
            catch (MissingJiraConfigException)
            {
                if (!HandleInvalidDetails(true));
            }

            if (settingsWork)
            {
                Close(); 
            }
            else if (cancelClicked)
            {
                if (MessageBox.Show("You Pressed Cancel, But Couldn't Connect To Jira\nDo You Want To Close The App?", "Sure You Want To Close", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
        }

        private bool HandleInvalidDetails(bool isMissingConfig)
        {
            if (isMissingConfig)
            {
                MessageBox.Show("You have to populate the Jira Credentials!\nAt present Gallifrey Required Active Connection To Jira", "Missing Config");
            }
            else
            {
                MessageBox.Show("Unable to connect to Jira with these settings!\nAt present Gallifrey Required Active Connection To Jira", "Unable to connect");
            }

            DialogResult = DialogResult.None;
            TopMost = true;
            return false;
        }

        private void chkAlert_CheckedChanged(object sender, EventArgs e)
        {
            txtAlertMins.Enabled = chkAlert.Checked;
        }

    }
}
