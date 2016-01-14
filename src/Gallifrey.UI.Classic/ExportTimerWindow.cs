using System;
using System.Windows.Forms;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Jira.Enum;
using Gallifrey.Jira.Model;
using Gallifrey.JiraTimers;
using Gallifrey.Settings;

namespace Gallifrey.UI.Classic
{
    public partial class ExportTimerWindow : Form
    {
        private readonly IBackend gallifrey;
        private readonly JiraTimer timerToShow;
        private readonly Issue jiraIssue;
        internal bool DisplayForm { get; private set; }

        public ExportTimerWindow(IBackend gallifrey, Guid timerGuid)
        {
            DisplayForm = true;
            this.gallifrey = gallifrey;
            timerToShow = gallifrey.JiraTimerCollection.GetTimer(timerGuid);
            InitializeComponent();

            var requireRefresh = !timerToShow.LastJiraTimeCheck.HasValue || timerToShow.LastJiraTimeCheck < DateTime.UtcNow.AddMinutes(-15);

            try
            {
                jiraIssue = gallifrey.JiraConnection.GetJiraIssue(timerToShow.JiraReference, requireRefresh);
            }
            catch (NoResultsFoundException)
            {
                MessageBox.Show($"Unable To Locate Jira {timerToShow.JiraReference}!\nCannot Export Time\nPlease Verify/Correct Jira Reference", "Unable To Locate Jira", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisplayForm = false;
            }

            if (requireRefresh)
            {
                gallifrey.JiraTimerCollection.RefreshFromJira(timerGuid, jiraIssue, gallifrey.JiraConnection.CurrentUser);

                timerToShow = gallifrey.JiraTimerCollection.GetTimer(timerGuid);    
            }

            if (timerToShow.FullyExported)
            {
                MessageBox.Show("There Is No Time To Export", "Nothing To Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DisplayForm = false;
            }

            txtJiraRef.Text = timerToShow.JiraReference;
            txtDescription.Text = timerToShow.JiraName;
            if (timerToShow.HasParent)
            {
                txtParentRef.Text = timerToShow.JiraParentReference;
                txtParentDesc.Text = timerToShow.JiraParentName;
            }
            else
            {
                txtParentRef.Visible = false;
                txtParentDesc.Visible = false;
                lblParentRef.Visible = false;
                lblParentDesc.Visible = false;
            }

            txtTotalHours.Text = timerToShow.ExactCurrentTime.Hours.ToString();
            txtTotalMinutes.Text = timerToShow.ExactCurrentTime.Minutes.ToString();
            txtExportedHours.Text = timerToShow.ExportedTime.Hours.ToString();
            txtExportedMins.Text = timerToShow.ExportedTime.Minutes.ToString();
            txtExportHours.Text = timerToShow.TimeToExport.Hours.ToString();
            txtExportMins.Text = timerToShow.TimeToExport.Minutes.ToString();

            if (jiraIssue.fields.timetracking == null)
            {
                txtRemainingHours.Text = "N/A";
                txtRemainingMinutes.Text = "N/A";
            }
            else
            {
                var remainingTime = jiraIssue.fields.timetracking != null ? TimeSpan.FromSeconds(jiraIssue.fields.timetracking.remainingEstimateSeconds) : new TimeSpan();
                var hours = (remainingTime.Days * 24) + remainingTime.Hours;
                txtRemainingHours.Text = hours.ToString();
                txtRemainingMinutes.Text = remainingTime.Minutes.ToString();
            }

            

            if (timerToShow.DateStarted.Date != DateTime.Now.Date)
            {
                calExportDate.Value = timerToShow.DateStarted.Date.AddHours(12);
            }
            else
            {
                calExportDate.Value = DateTime.Now;
            }

            radAutoAdjust.Checked = gallifrey.Settings.ExportSettings.DefaultRemainingValue == DefaultRemaining.Auto;
            radLeaveRemaining.Checked = gallifrey.Settings.ExportSettings.DefaultRemainingValue == DefaultRemaining.Leave;
            radSetValue.Checked = gallifrey.Settings.ExportSettings.DefaultRemainingValue == DefaultRemaining.Set;
            radSetValue_CheckedChanged(this, null);

            TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;

            txtComment.AltEnterEvent += btnOK_Click;
        }
        
        public void PreLoadExportTime(TimeSpan exportTime)
        {
            if (timerToShow.TimeToExport > exportTime)
            {
                txtExportHours.Text = exportTime.Hours.ToString();
                txtExportMins.Text = exportTime.Minutes.ToString();
                txtExportHours.Enabled = false;
                txtExportMins.Enabled = false;
            }
        }

        private bool ExportTime()
        {
            int hours, minutes;

            if (!int.TryParse(txtExportHours.Text, out hours) || !int.TryParse(txtExportMins.Text, out minutes))
            {
                MessageBox.Show("Invalid Hours/Minutes Entered For Export!", "Invalid Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (minutes > 60)
            {
                MessageBox.Show("You Cannot Export More Than 60 Minutes!", "Invalid Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (hours > 10)
            {
                MessageBox.Show("You Cannot Export More Than 10 Hours!", "Invalid Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }


            var worklogStrategy = GetWorklogStrategy();
            TimeSpan? newEstimate = null;
            if (worklogStrategy == WorkLogStrategy.SetValue)
            {
                newEstimate = GetNewEstimate();
                if (newEstimate == null)
                {
                    return false;
                }
            }

            var exportTimespan = new TimeSpan(hours, minutes, 0);

            if (timerToShow.TimeToExport < exportTimespan)
            {
                MessageBox.Show($"You Cannot Export More Than The Timer States Un-Exported\nThis Value Is {timerToShow.TimeToExport.ToString(@"hh\:mm")}!", "Invalid Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                gallifrey.JiraConnection.LogTime(jiraIssue.key, calExportDate.Value, exportTimespan, worklogStrategy, txtComment.Text, newEstimate);
            }
            catch (WorkLogException)
            {
                MessageBox.Show("Unable To Log Work!", "Error Exporting", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (StateChangedException)
            {
                MessageBox.Show("Unable To Re-Close A The Jira, Manually Check!!", "Error Exporting", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            gallifrey.JiraTimerCollection.AddJiraExportedTime(timerToShow.UniqueId, hours, minutes);


            return true;
        }

        private TimeSpan? GetNewEstimate()
        {
            int hours, minutes;

            if (!int.TryParse(txtSetValueHours.Text, out hours) || !int.TryParse(txtSetValueMins.Text, out minutes))
            {
                MessageBox.Show("Invalid Hours/Minutes Entered For Remaining Estimate!", "Invalid Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            if (minutes > 60)
            {
                MessageBox.Show("You Cannot Set Remaining To More Than 60 Minutes!", "Invalid Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return new TimeSpan(hours, minutes, 0);
        }

        private WorkLogStrategy GetWorklogStrategy()
        {
            WorkLogStrategy worklogStrategy;
            if (radAutoAdjust.Checked)
            {
                worklogStrategy = WorkLogStrategy.Automatic;
            }
            else if (radLeaveRemaining.Checked)
            {
                worklogStrategy = WorkLogStrategy.LeaveRemaining;
            }
            else
            {
                worklogStrategy = WorkLogStrategy.SetValue;
            }
            return worklogStrategy;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            TopMost = false;
            if (ExportTime())
            {
                Close();
            }
            else
            {
                DialogResult = DialogResult.None;
            }
            TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
        }

        private void radSetValue_CheckedChanged(object sender, EventArgs e)
        {
            txtSetValueHours.Enabled = radSetValue.Checked;
            txtSetValueMins.Enabled = radSetValue.Checked;
        }

        private void ExportTimerWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.Enter)
            {
                btnOK_Click(sender, null);
            }
        }
    }
}
