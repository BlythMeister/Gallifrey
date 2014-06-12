using System;
using System.Linq;
using System.Windows.Forms;
using Atlassian.Jira;
using Gallifrey.Exceptions.IntergrationPoints;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.ExtensionMethods;

namespace Gallifrey.UI.Classic
{
    public partial class AddTimerWindow : Form
    {
        private readonly IBackend gallifrey;
        private bool showingJiras;
        public Guid? NewTimerId { get; private set; }
        internal bool DisplayForm { get; private set; }

        public AddTimerWindow(IBackend gallifrey)
        {
            DisplayForm = true;
            this.gallifrey = gallifrey;
            InitializeComponent();
            calStartDate.MinDate = DateTime.Now.AddDays(gallifrey.Settings.AppSettings.KeepTimersForDays * -1);
            calStartDate.MaxDate = DateTime.Now.AddDays(gallifrey.Settings.AppSettings.KeepTimersForDays);

            txtJiraRef.AutoCompleteCustomSource.AddRange(gallifrey.JiraConnection.GetJiraProjects().Select(x => x.ToString()).ToArray());
            showingJiras = false;

            TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
        }

        public void PreLoadJira(string jiraRef)
        {
            txtJiraRef.Text = jiraRef;
            txtJiraRef.Enabled = false;
        }

        public void PreLoadDate(DateTime startDate)
        {
            if (!startDate.Between(calStartDate.MinDate, calStartDate.MaxDate))
            {
                MessageBox.Show("New Timer Start Date Is Not Valid For The Minimum And Maximum Duration Of Timers To Keep.\nHave You Updated The Days To Keep In Settings?", "New Timer Date Invalid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DisplayForm = false;
            }
            else
            {
                calStartDate.Value = startDate;
            }
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
                Close();
            }
            else
            {
                DialogResult = DialogResult.None;
            }
        }

        private bool AddJira()
        {
            var jiraReference = txtJiraRef.Text;
            var startDate = calStartDate.Value.Date;
            int hours, minutes;
            int.TryParse(txtStartHours.Text, out hours);
            int.TryParse(txtStartMins.Text, out minutes);

            if (minutes > 60)
            {
                MessageBox.Show("You Cannot Start A Timer With More Than 60 Minutes!", "Invalid Start Time", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (hours > 10)
            {
                MessageBox.Show("You Cannot Start A Timer With More Than 10 Hours!", "Invalid  Start Time", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            var seedTime = new TimeSpan(hours, minutes, 0);

            Issue jiraIssue;
            try
            {
                jiraIssue = gallifrey.JiraConnection.GetJiraIssue(jiraReference);
            }
            catch (NoResultsFoundException)
            {
                MessageBox.Show("Unable To Locate The Jira", "Invalid Jira", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (txtJiraRef.Enabled)
            {
                if (MessageBox.Show(string.Format("Jira found!\n\nRef: {0}\nName: {1}\n\nIs that correct?", jiraIssue.Key, jiraIssue.Summary), "Correct Jira?", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return false;
                }
            }

            try
            {
                NewTimerId = gallifrey.JiraTimerCollection.AddTimer(jiraIssue, startDate, seedTime, chkStartNow.Checked);
            }
            catch (DuplicateTimerException)
            {
                MessageBox.Show("This Timer Already Exists!", "Duplicate Timer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void btnCancelAddTimer_Click(object sender, EventArgs e)
        {
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

        private void btnSearch_Click(object sender, EventArgs e)
        {
            TopMost = false;
            var searchForm = new SearchWindow(gallifrey);
            searchForm.SetFromAddWindow();
            searchForm.ShowDialog();

            if (!string.IsNullOrWhiteSpace(searchForm.JiraReference))
            {
                txtJiraRef.Text = searchForm.JiraReference;
                txtJiraRef.Enabled = false;
            }

            TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
        }

        private void txtJiraRef_TextChanged(object sender, EventArgs e)
        {
            var enteredJiraRef = txtJiraRef.Text;

            if (enteredJiraRef.Contains(" ("))
            {
                enteredJiraRef = enteredJiraRef.Substring(0, enteredJiraRef.IndexOf(" ("));
                txtJiraRef.Text = enteredJiraRef;
            }

            if (enteredJiraRef.Contains("-"))
            {
                if (!showingJiras)
                {
                    txtJiraRef.AutoCompleteCustomSource.Clear();
                    txtJiraRef.AutoCompleteCustomSource.AddRange(gallifrey.JiraConnection.GetRecentJirasFound().Select(x => x.ToString()).ToArray());
                    showingJiras = true;
                    txtJiraRef.SelectionLength = 0;
                    txtJiraRef.SelectionStart = txtJiraRef.TextLength;
                }
            }
            else
            {
                if (showingJiras)
                {
                    txtJiraRef.AutoCompleteCustomSource.Clear();
                    txtJiraRef.AutoCompleteCustomSource.AddRange(gallifrey.JiraConnection.GetJiraProjects().Select(x => x.ToString()).ToArray());
                    showingJiras = false;
                    txtJiraRef.SelectionLength = 0;
                    txtJiraRef.SelectionStart = txtJiraRef.TextLength;
                }
            }
        }
    }
}
