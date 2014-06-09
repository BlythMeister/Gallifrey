using System;
using System.Windows.Forms;
using Atlassian.Jira;
using Gallifrey.Exceptions.IntergrationPoints;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.JiraTimers;

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

            jiraIssue = gallifrey.JiraConnection.GetJiraIssue(timerToShow.JiraReference);
            var loggedTime = new TimeSpan();
            foreach (var worklog in jiraIssue.GetWorklogs())
            {
                if (worklog.StartDate.HasValue && worklog.StartDate.Value.Date == timerToShow.DateStarted.Date && worklog.Author.ToLower() == gallifrey.Settings.JiraConnectionSettings.JiraUsername.ToLower())
                {
                    loggedTime = loggedTime.Add(new TimeSpan(0, 0, (int)worklog.TimeSpentInSeconds));
                }
            }
            gallifrey.JiraTimerCollection.SetJiraExportedTime(timerGuid, loggedTime);

            timerToShow = gallifrey.JiraTimerCollection.GetTimer(timerGuid);

            if (timerToShow.TimeToExport.TotalMinutes < 1)
            {
                MessageBox.Show("There Is No Time To Export", "Nothing To Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DisplayForm = false;
            }

            txtJiraRef.Text = timerToShow.JiraReference;
            txtDescription.Text = timerToShow.JiraName;
            txtTotalHours.Text = timerToShow.ExactCurrentTime.Hours.ToString();
            txtTotalMinutes.Text = timerToShow.ExactCurrentTime.Minutes.ToString();
            txtExportedHours.Text = timerToShow.ExportedTime.Hours.ToString();
            txtExportedMins.Text = timerToShow.ExportedTime.Minutes.ToString();
            txtExportHours.Text = timerToShow.TimeToExport.Hours.ToString();
            txtExportMins.Text = timerToShow.TimeToExport.Minutes.ToString();

            if (timerToShow.DateStarted.Date != DateTime.Now.Date)
            {
                calExportDate.Value = timerToShow.DateStarted.Date.AddHours(12);
            }
            else
            {
                calExportDate.Value = DateTime.Now;
            }

            TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
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
            if (worklogStrategy == WorklogStrategy.NewRemainingEstimate)
            {
                newEstimate = GetNewEstimate();
                if (newEstimate == null)
                {
                    return false;
                }
            }


            try
            {
                gallifrey.JiraConnection.LogTime(jiraIssue, calExportDate.Value, new TimeSpan(hours, minutes, 0), worklogStrategy, txtComment.Text, newEstimate);
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

        private WorklogStrategy GetWorklogStrategy()
        {
            WorklogStrategy worklogStrategy;
            if (radAutoAdjust.Checked)
            {
                worklogStrategy = WorklogStrategy.AutoAdjustRemainingEstimate;
            }
            else if (radLeaveRemaining.Checked)
            {
                worklogStrategy = WorklogStrategy.RetainRemainingEstimate;
            }
            else
            {
                worklogStrategy = WorklogStrategy.NewRemainingEstimate;
            }
            return worklogStrategy;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ExportTime())
            {
                Close();
            }
            else
            {
                DialogResult = DialogResult.None;
            }
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
