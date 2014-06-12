using System;
using System.Linq;
using System.Windows.Forms;
using Atlassian.Jira;
using Gallifrey.Exceptions.IntergrationPoints;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.JiraTimers;

namespace Gallifrey.UI.Classic
{
    public partial class RenameTimerWindow : Form
    {
        private readonly IBackend gallifrey;
        private readonly JiraTimer timerToShow;
        private bool showingJiras;

        public RenameTimerWindow(IBackend gallifrey, Guid timerGuid)
        {
            this.gallifrey = gallifrey;
            timerToShow = gallifrey.JiraTimerCollection.GetTimer(timerGuid);
            InitializeComponent();

            txtJiraRef.AutoCompleteCustomSource.AddRange(gallifrey.JiraConnection.GetJiraProjects().Select(x => x.ToString()).ToArray());
            showingJiras = false;
            txtJiraRef.Text = timerToShow.JiraReference;

            calStartDate.Value = timerToShow.DateStarted.Date;
            
            txtJiraRef.Enabled = timerToShow.HasExportedTime();
            calStartDate.Enabled = timerToShow.HasExportedTime();

            TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
        }

        private bool RenameTimer()
        {
            var jiraReference = txtJiraRef.Text;

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

            try
            {
                gallifrey.JiraTimerCollection.RenameTimer(timerToShow.UniqueId, jiraIssue);
            }
            catch (DuplicateTimerException)
            {
                MessageBox.Show("This Timer Already Exists!", "Duplicate Timer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }



        private bool ChangeTimerDate()
        {
            try
            {
                gallifrey.JiraTimerCollection.ChangeTimerDate(timerToShow.UniqueId, calStartDate.Value);
            }
            catch (DuplicateTimerException)
            {
                MessageBox.Show("This Timer Already Exists!", "Duplicate Timer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if ((txtJiraRef.Text != timerToShow.JiraReference && RenameTimer()) || (calStartDate.Value.Date != timerToShow.DateStarted.Date && ChangeTimerDate()))
            {
                Close();
            }
            else
            {
                DialogResult = DialogResult.None;
            }

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
