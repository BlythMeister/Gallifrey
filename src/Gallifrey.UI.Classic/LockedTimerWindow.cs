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
            var removeTimer = true;
            var closeWindow = true;
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
                    closeWindow = false;
                }
                TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
            }
            else if (radRemove.Checked)
            {
                if (MessageBox.Show(string.Format("Are You Sure You Want To Remove Idle Timer\nBetween {0} & {1}?", idleTimer.DateStarted.ToString("HH:mm:ss"), idleTimer.DateFinished.Value.ToString("HH:mm:ss")), "Are You Sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    removeTimer = false;
                    closeWindow = false;
                }
            }
            else
            {
                removeTimer = false;
            }

            if (removeTimer)
            {
                gallifrey.IdleTimerCollection.RemoveTimer(idleTimer.UniqueId);
            }

            if (closeWindow)
            {
                Close();
            }
        }

        private void lstIdleTimers_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetExportTimers();
        }
    }
}
