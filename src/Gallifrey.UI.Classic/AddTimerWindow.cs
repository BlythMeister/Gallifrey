using System;
using System.Windows.Forms;
using Atlassian.Jira;
using Gallifrey.Exceptions.IntergrationPoints;
using Gallifrey.Exceptions.JiraTimers;

namespace Gallifrey.UI.Classic
{
    public partial class AddTimerWindow : Form
    {
        private readonly IBackend gallifrey;
        public bool TimerAdded { get; private set; }

        public AddTimerWindow(IBackend gallifrey)
        {
            this.gallifrey = gallifrey;
            InitializeComponent();
            calStartDate.MinDate = DateTime.Now.AddDays(gallifrey.AppSettings.KeepTimersForDays*-1);
            calStartDate.MaxDate = DateTime.Now.AddDays(gallifrey.AppSettings.KeepTimersForDays);

            TopMost = gallifrey.AppSettings.UiAlwaysOnTop;
        }
        
        public void PreLoadJira(string jiraRef)
        {
            txtJiraRef.Text = jiraRef;
        }
        
        public void PreLoadDate(DateTime startDate)
        {
            calStartDate.Value = startDate;
        }

        public void PreLoadTime(TimeSpan time)
        {
            txtStartHours.Text = time.Hours.ToString();
            txtStartMins.Text = time.Minutes.ToString();
        }

        private void btnAddTimer_Click(object sender, EventArgs e)
        {
            if (AddJira())
            {
                TimerAdded = true;
                Close();
            }
        }

        private bool AddJira()
        {
            var jiraReference = txtJiraRef.Text;
            var startDate = calStartDate.Value.Date;
            int hours, minutes;
            int.TryParse(txtStartHours.Text, out hours);
            int.TryParse(txtStartMins.Text, out minutes);

            var seedTime = new TimeSpan(hours, minutes, 0);

            Issue jiraIssue;
            try
            {
                jiraIssue = gallifrey.JiraConnection.GetJiraIssue(jiraReference);
            }
            catch (NoResultsFoundException)
            {
                MessageBox.Show("Unable to locate the Jira", "Invalid Jira", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (MessageBox.Show(string.Format("Jira found!\n\nRef: {0}\nName: {1}\n\nIs that correct?", jiraIssue.Key, jiraIssue.Summary), "Correct Jira?", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return false;
            }

            Guid newTimerId;
            try
            {
                newTimerId = gallifrey.JiraTimerCollection.AddTimer(jiraIssue, startDate, seedTime, chkStartNow.Checked);
            }
            catch (DuplicateTimerException)
            {
                MessageBox.Show("This timer already exists!", "Duplicate Timer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!chkStartNow.Checked && seedTime.TotalMinutes > 0)
            {
                if (MessageBox.Show("Do You Want To Log This Time To Jira?", "Log Time?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var exportTimerWindow = new ExportTimerWindow(gallifrey, newTimerId);
                    if (exportTimerWindow.DisplayForm)
                    {
                        exportTimerWindow.ShowDialog();
                    }
                }
            }

            return true;
        }

        private void btnCancelAddTimer_Click(object sender, EventArgs e)
        {
            TimerAdded = false;
            Close();
        }

        private void calStartDate_ValueChanged(object sender, EventArgs e)
        {
            if (calStartDate.Value.Date != DateTime.Now.Date)
            {
                chkStartNow.Checked = false;
                chkStartNow.Enabled = false;
            }
            else
            {
                chkStartNow.Enabled = true;
            }
        }
    }
}
