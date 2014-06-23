using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Atlassian.Jira;
using Gallifrey.Exceptions.IntergrationPoints;
using Gallifrey.IdleTimers;
using Gallifrey.JiraIntegration;
using Gallifrey.JiraTimers;

namespace Gallifrey.UI.Classic
{
    public partial class LockedTimerWindow : Form
    {
        private readonly IBackend gallifrey;
        private Guid? runningTimerId;
        internal bool DisplayForm { get; private set; }
        public Guid? NewTimerId { get; private set; }

        public LockedTimerWindow(IBackend gallifrey)
        {
            DisplayForm = true;
            this.gallifrey = gallifrey;
            InitializeComponent();

            BindIdleTimers();
            SetRunningTimer();
            BindRecentTimers();

            TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
        }

        private void BindRecentTimers()
        {
            cmbRecentJiras.DataSource = gallifrey.JiraTimerCollection.GetJiraReferencesForLastDays(7).ToList();
            cmbRecentJiras.Refresh();
            cmbRecentJiras.Text = string.Empty;
            cmbRecentJiras.SelectedText = "";
        }

        private void BindIdleTimers(bool showNoTimers = true)
        {
            var idleTimers = gallifrey.IdleTimerCollection.GetUnusedLockTimers().ToList();

            if (!idleTimers.Any())
            {
                if (showNoTimers)
                {
                    MessageBox.Show("No Idle Timers To Show!", "Nothing To See Here", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                DisplayForm = false;
                Close();
            }

            lstLockedTimers.DataSource = idleTimers;
            lstLockedTimers.Refresh();
        }

        private void SetRunningTimer()
        {
            runningTimerId = gallifrey.JiraTimerCollection.GetRunningTimerId();

            var selectedTimer = (IdleTimer)lstLockedTimers.SelectedItem;

            bool disableRunning;
            if (selectedTimer == null)
            {
                disableRunning = true;
            }
            else
            {
                disableRunning = selectedTimer.DateStarted.Date != DateTime.Now.Date;
            }
            
            if (disableRunning || !runningTimerId.HasValue)
            {
                radRunning.Checked = false;
                radRunning.Enabled = false;
                lblRunning.Text = "N/A";
                lblRunning.Enabled = false;
            }
            else if (runningTimerId.HasValue)
            {
                lblRunning.Text = gallifrey.JiraTimerCollection.GetTimer(runningTimerId.Value).ToString();
                lblRunning.Enabled = true;
                radRunning.Enabled = true;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (lstLockedTimers.SelectedItems.Count > 1)
            {
                MessageBox.Show("Cannot Apply Action Using Multiple Timers!", "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            var removeTimer = true;
            var idleTimer = (IdleTimer)lstLockedTimers.SelectedItem;

            if (radSelected.Checked)
            {
                var selectedRecent = (RecentJira)cmbRecentJiras.SelectedItem;

                var timersForADate = gallifrey.JiraTimerCollection.GetTimersForADate(idleTimer.DateStarted);
                Guid timerToAddTimeTo;

                if (timersForADate.Any(x => x.JiraReference == selectedRecent.JiraReference))
                {
                    timerToAddTimeTo = timersForADate.First(x => x.JiraReference == selectedRecent.JiraReference).UniqueId;
                }
                else
                {
                    Issue jiraIssue;
                    try
                    {
                        jiraIssue = gallifrey.JiraConnection.GetJiraIssue(selectedRecent.JiraReference);
                    }
                    catch (NoResultsFoundException)
                    {
                        MessageBox.Show("Unable To Locate The Jira", "Invalid Jira", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        BindRecentTimers();
                        return;
                    }

                    timerToAddTimeTo = gallifrey.JiraTimerCollection.AddTimer(jiraIssue, idleTimer.DateStarted, new TimeSpan(), false);
                }

                gallifrey.JiraTimerCollection.AddIdleTimer(timerToAddTimeTo, idleTimer);
                NewTimerId = timerToAddTimeTo;
            }
            else if (radRunning.Checked)
            {
                gallifrey.JiraTimerCollection.AddIdleTimer(runningTimerId.Value, idleTimer);
                NewTimerId = runningTimerId;
            }
            else if (radNew.Checked)
            {
                TopMost = false;
                var addForm = new AddTimerWindow(gallifrey);
                addForm.PreLoadTime(idleTimer.IdleTimeValue);
                addForm.ShowDialog();
                if (!addForm.NewTimerId.HasValue)
                {
                    removeTimer = false;
                }
                TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
                NewTimerId = addForm.NewTimerId;
            }
            else
            {
                removeTimer = false;
                MessageBox.Show("You Must Select An Operation When Pressing This Button", "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (removeTimer)
            {
                gallifrey.IdleTimerCollection.RemoveTimer(idleTimer.UniqueId);
                Close();
            }
        }

        private void lstIdleTimers_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetRunningTimer();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            foreach (IdleTimer idleTimer in lstLockedTimers.SelectedItems)
            {
                if (MessageBox.Show(string.Format("Are You Sure You Want To Remove Idle Timer For\n{0} Between {1} & {2}?", idleTimer.DateStarted.ToString("ddd, dd MMM"), idleTimer.DateStarted.ToString("HH:mm:ss"), idleTimer.DateFinished.Value.ToString("HH:mm:ss")), "Are You Sure", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    gallifrey.IdleTimerCollection.RemoveTimer(idleTimer.UniqueId);
                }
            }
            BindIdleTimers(false);
        }

        private void cmbRecentJiras_SelectedIndexChanged(object sender, EventArgs e)
        {
            radSelected.Checked = true;
        }
    }
}
