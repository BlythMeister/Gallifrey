using System;
using System.Linq;
using System.Windows.Forms;
using Gallifrey.IdleTimers;
using Gallifrey.JiraTimers;

namespace Gallifrey.UI.Classic
{
    public partial class LockedTimerWindow : Form
    {
        private readonly IBackend gallifrey;
        public bool DisplayForm = true;
        private Guid? runningTimerId;

        public LockedTimerWindow(IBackend gallifrey)
        {
            this.gallifrey = gallifrey;
            InitializeComponent();

            BindIdleTimers();

            runningTimerId = gallifrey.JiraTimerCollection.GetRunningTimerId();

            if (runningTimerId.HasValue)
            {
                lblRunning.Text = gallifrey.JiraTimerCollection.GetTimer(runningTimerId.Value).ToString();
            }
            else
            {
                lblRunning.Text = "N/A";
                radRunning.Enabled = false;
                lblRunning.Enabled = false;
            }

            TopMost = gallifrey.AppSettings.UiAlwaysOnTop;
        }

        private void BindIdleTimers()
        {
            var idleTimers = gallifrey.IdleTimerCollection.GetUnusedLockTimers().ToList();

            if (!idleTimers.Any())
            {
                MessageBox.Show("No Idle Timers To Show!", "Nothing To See Here", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DisplayForm = false;
                Close();
            }

            lstLockedTimers.DataSource = idleTimers;
            lstLockedTimers.Refresh();

            SetDayTimers();
        }

        private void SetDayTimers()
        {
            var selectedTimer = (IdleTimer)lstLockedTimers.SelectedItem;
            if (selectedTimer == null)
            {
                cmbDayTimers.DataSource = null;
                cmbDayTimers.Refresh();
                radSelected.Enabled = false;
                cmbDayTimers.Enabled = false;
            }
            else
            {
                radSelected.Enabled = true;
                cmbDayTimers.Enabled = true;
                cmbDayTimers.DataSource = gallifrey.JiraTimerCollection.GetTimersForADate(selectedTimer.DateStarted.Date).ToList();
                cmbDayTimers.Refresh();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            bool removeTimer = true;
            var idleTimer = (IdleTimer)lstLockedTimers.SelectedItem;

            if (radSelected.Checked)
            {
                var selectedTimer = (JiraTimer)cmbDayTimers.SelectedItem;
                gallifrey.JiraTimerCollection.AddIdleTimer(selectedTimer.UniqueId, idleTimer);
            }
            else if (radRunning.Checked)
            {
                gallifrey.JiraTimerCollection.AddIdleTimer(runningTimerId.Value, idleTimer);
            }
            else if (radNew.Checked)
            {
                TopMost = false;
                var addForm = new AddTimerWindow(gallifrey);
                addForm.PreLoadTime(idleTimer.ExactCurrentTime);
                addForm.ShowDialog();
                if (!addForm.NewTimerId.HasValue)
                {
                    removeTimer = false;
                }
                TopMost = gallifrey.AppSettings.UiAlwaysOnTop;
            }
            else
            {
                MessageBox.Show("Please Choose One Of The Locked Timer Destinations", "No Action Selected", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                removeTimer = false;
            }

            if (removeTimer)
            {
                gallifrey.IdleTimerCollection.RemoveTimer(idleTimer.UniqueId);
                BindIdleTimers();
            }
        }

        private void lstIdleTimers_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetDayTimers();
        }
    }
}
