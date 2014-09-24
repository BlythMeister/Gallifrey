using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Gallifrey.Exceptions.IntergrationPoints;
using Gallifrey.Jira;

namespace Gallifrey.UI.Classic
{
    public partial class SearchWindow : Form
    {
        private readonly IBackend gallifrey;
        private bool fromAddWindow = false;
        public Guid? NewTimerId { get; private set; }
        public string JiraReference { get; private set;  }

        public SearchWindow(IBackend gallifrey)
        {
            this.gallifrey = gallifrey;
            InitializeComponent();

            try
            {
                cmbUserFilters.Items.Add(string.Empty);
                foreach (var jiraFilter in gallifrey.JiraConnection.GetJiraFilters())
                {
                    cmbUserFilters.Items.Add(jiraFilter);
                }
                cmbUserFilters.Enabled = true;
            }
            catch (NoResultsFoundException)
            {
                cmbUserFilters.Enabled = false;
            }
            
            try
            {
                var currentUserIssues = gallifrey.JiraConnection.GetJiraCurrentUserOpenIssues();
                lstResults.DataSource = currentUserIssues.Select(issue => new JiraSearchResult(issue)).ToList();
                lstResults.Enabled = true;
            }
            catch (NoResultsFoundException)
            {
                lstResults.Enabled = false;
            }

            TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
        }

        public void SetFromAddWindow()
        {
            fromAddWindow = true;
        }

        private void btnAddTimer_Click(object sender, EventArgs e)
        {
            var selectedIssue = (JiraSearchResult) lstResults.SelectedItem;

            if (selectedIssue == null)
            {
                MessageBox.Show("No Issue Selected, Cannot Add Timer", "No Issue Selected", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                DialogResult = DialogResult.None;
            }

            if (fromAddWindow)
            {
                JiraReference = selectedIssue.JiraRef;
                Close();
            }
            else
            {
                LoadAddTimerWindow(selectedIssue);    
            }
            
        }

        private void LoadAddTimerWindow(JiraSearchResult selectedIssue)
        {
            TopMost = false;
            var addForm = new AddTimerWindow(gallifrey);
            addForm.PreLoadJira(selectedIssue.JiraRef);
            addForm.ShowDialog();
            NewTimerId = addForm.NewTimerId;
            if (NewTimerId.HasValue)
            {
                Close();
            }
            else
            {
                DialogResult = DialogResult.None;
                TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
            }
        }

        private void btnCancelAddTimer_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            var freeSearch = txtJiraRef.Text;
            var filterSearch = (string)cmbUserFilters.SelectedItem;

            if (string.IsNullOrWhiteSpace(freeSearch) && string.IsNullOrWhiteSpace(filterSearch))
            {
                MessageBox.Show("You Must Enter Search Criteria", "Invalid Search Criteria", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!string.IsNullOrWhiteSpace(freeSearch) && !string.IsNullOrWhiteSpace(filterSearch))
            {
                MessageBox.Show("You Cannot Use Filter & Search Text At The Same Time", "Invalid Search Criteria", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                IEnumerable<Issue> searchResults;
                if (!string.IsNullOrWhiteSpace(freeSearch))
                {
                    searchResults = gallifrey.JiraConnection.GetJiraIssuesFromSearchText(freeSearch);
                }
                else
                {
                    searchResults = gallifrey.JiraConnection.GetJiraIssuesFromFilter(filterSearch);
                }

                if (searchResults.Any())
                {
                    lstResults.DataSource = searchResults.Select(issue => new JiraSearchResult(issue)).ToList();
                    lstResults.Enabled = true;    
                }
                else
                {
                    throw new NoResultsFoundException("No Results In Collection");
                }
            }
            catch (NoResultsFoundException)
            {
                lstResults.Enabled = false;
                MessageBox.Show("No Results Found, Try A Different Search", "No Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        internal class JiraSearchResult
        {
            internal string JiraRef { get; private set; }
            internal string JiraDesc { get; private set; }

            internal JiraSearchResult(Issue issue)
            {
                JiraRef = issue.key;
                JiraDesc = issue.fields.summary;
            }

            public override string ToString()
            {
                return string.Format("[ {0} ] - {1}", JiraRef, JiraDesc);
            }
        }

        private void SearchWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.Enter)
            {
                btnAddTimer_Click(sender, null);
            }
        }

        private void lstResults_DoubleClick(object sender, EventArgs e)
        {
            btnAddTimer_Click(sender, e);
        }
    }
}
