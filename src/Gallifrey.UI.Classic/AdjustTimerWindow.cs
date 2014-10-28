using System;
using System.Windows.Forms;
using Gallifrey.JiraTimers;

namespace Gallifrey.UI.Classic
{
    public partial class AdjustTimerWindow : Form
    {
        private readonly IBackend gallifrey;
        private readonly JiraTimer timerToShow;

        public AdjustTimerWindow(IBackend gallifrey, Guid timerGuid)
        {
            this.gallifrey = gallifrey;
            timerToShow = gallifrey.JiraTimerCollection.GetTimer(timerGuid);
            InitializeComponent();

            txtJiraRef.Text = timerToShow.JiraReference;

            TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
        }

        private bool AdjustTime(bool addTime)
        {
            int hours, minutes;

            if (!int.TryParse(txtHours.Text, out hours) || !int.TryParse(txtMinutes.Text, out minutes))
            {
                MessageBox.Show("Invalid Hours/Minutes Entered!", "Invalid Adjustment", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (minutes > 60)
            {
                MessageBox.Show("You Cannot Change By More Than 60 Minutes!", "Invalid Adjustment", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (hours > 10)
            {
                MessageBox.Show("You Cannot Change By More Than 10 Hours!", "Invalid Adjustment", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            var adjustmentSuccess = gallifrey.JiraTimerCollection.AdjustTime(timerToShow.UniqueId, hours, minutes, addTime);

            if (!adjustmentSuccess)
            {
                MessageBox.Show("You Cannot Subtract More Time Than You Have Already Exported\nHave Subtracted All Un-Exported Time", "Adjustment Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (AdjustTime(true))
            {
                Close();
            }
            else
            {
                DialogResult = DialogResult.None;
            }
        }

        private void btnSubtract_Click(object sender, EventArgs e)
        {
            if (AdjustTime(false))
            {
                Close();
            }
            else
            {
                DialogResult = DialogResult.None;
            }
        }
    }
}
