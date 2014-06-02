using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Atlassian.Jira;
using Gallifrey.Exceptions.IntergrationPoints;
using Gallifrey.Exceptions.JiraTimers;

namespace Gallifrey.UI.Classic
{
    public partial class SearchWindow : Form
    {
        private readonly IBackend gallifrey;
        public Guid? NewTimerId { get; private set; }

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

        private void btnAddTimer_Click(object sender, EventArgs e)
        {
            var selectedIssue = (JiraSearchResult) lstResults.SelectedItem;

            TopMost = false;
            var addForm = new AddTimerWindow(gallifrey);
            addForm.PreLoadJira(selectedIssue.JiraRef);
            addForm.ShowDialog();
            NewTimerId = addForm.NewTimerId;
            Close();
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

                lstResults.DataSource = searchResults.Select(issue => new JiraSearchResult(issue)).ToList();
                lstResults.Enabled = true;
            }
            catch (NoResultsFoundException)
            {
                lstResults.Enabled = false;
            }
        }

        internal class JiraSearchResult
        {
            internal string JiraRef { get; private set; }
            internal string JiraDesc { get; private set; }

            internal JiraSearchResult(Issue issue)
            {
                JiraRef = issue.Key.Value;
                JiraDesc = issue.Summary;
            }

            public override string ToString()
            {
                return string.Format("[ {0} ] - {1}", JiraRef, JiraDesc);
            }
        }
    }
}
