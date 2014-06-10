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
        private Guid? runningTimerId;
        internal bool DisplayForm { get; private set; }

        public LockedTimerWindow(IBackend gallifrey)
        {
            DisplayForm = true;
            this.gallifrey = gallifrey;
            InitializeComponent();

            BindIdleTimers();
            SetExportTimers();

            TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
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

        private void SetExportTimers()
        {
            runningTimerId = gallifrey.JiraTimerCollection.GetRunningTimerId();

            var selectedTimer = (IdleTimer)lstLockedTimers.SelectedItem;
            var disableDayTimers = false;
            var disableRunning = false;

            if (selectedTimer == null)
            {
                disableDayTimers = true;
                disableRunning = true;
            }
            else
            {
                var dayTimers = gallifrey.JiraTimerCollection.GetTimersForADate(selectedTimer.DateStarted.Date).ToList();
                if (!dayTimers.Any())
                {
                    disableDayTimers = true;
                }
                else
                {
                    radSelected.Enabled = true;
                    cmbDayTimers.Enabled = true;
                    cmbDayTimers.DataSource = dayTimers;
                    cmbDayTimers.Refresh();
                }

                disableRunning = selectedTimer.DateStarted.Date != DateTime.Now.Date;
            }

            if (disableDayTimers)
            {
                cmbDayTimers.DataSource = null;
                cmbDayTimers.Refresh();
                radSelected.Checked = false;
                radSelected.Enabled = false;
                cmbDayTimers.Enabled = false;
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
                MessageBox.Show("Cannot Apply Action Using Multiple Timers!", "Invalid Opperation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            var removeTimer = true;
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
                TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
            }
            else
            {
                removeTimer = false;
                MessageBox.Show("You Must Select An Opperation When Pressing This Button", "Invalid Opperation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (removeTimer)
            {
                gallifrey.IdleTimerCollection.RemoveTimer(idleTimer.UniqueId);
                Close();
            }
        }

        private void lstIdleTimers_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetExportTimers();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            foreach (IdleTimer idleTimer in lstLockedTimers.SelectedItems)
            {
                if (MessageBox.Show(string.Format("Are You Sure You Want To Remove Idle Timer\nBetween {0} & {1}?", idleTimer.DateStarted.ToString("HH:mm:ss"), idleTimer.DateFinished.Value.ToString("HH:mm:ss")), "Are You Sure", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    gallifrey.IdleTimerCollection.RemoveTimer(idleTimer.UniqueId);
                }
            }
        }
    }
}
