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

            SetDayTimers();
        }

        private void SetDayTimers()
        {
            var selectedTimer = (IdleTimer)lstLockedTimers.SelectedItem;
            var disableDayTimers = false;
            
            if (selectedTimer == null)
            {
                disableDayTimers = true;
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
            }

            if (disableDayTimers)
            {
                cmbDayTimers.DataSource = null;
                cmbDayTimers.Refresh();
                radSelected.Enabled = false;
                cmbDayTimers.Enabled = false;
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
                TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
            }
            else
            {
                MessageBox.Show("Please Choose One Of The Locked Timer Destinations", "No Action Selected", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                removeTimer = false;
            }

            if (removeTimer)
            {
                gallifrey.IdleTimerCollection.RemoveTimer(idleTimer.UniqueId);
                BindIdleTimers(false);
            }
        }

        private void lstIdleTimers_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetDayTimers();
        }
    }
}
