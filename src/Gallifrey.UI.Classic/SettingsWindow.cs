using System;
using System.Windows.Forms;

namespace Gallifrey.UI.Classic
{
    public partial class SettingsWindow : Form
    {
        private readonly Backend galifrey;

        public SettingsWindow(Backend galifrey)
        {
            this.galifrey = galifrey;
            InitializeComponent();
           
            if (galifrey.AppSettings.JiraUrl != null) txtJiraUrl.Text = galifrey.AppSettings.JiraUrl;
            if (galifrey.AppSettings.JiraUsername != null) txtJiraUsername.Text = galifrey.AppSettings.JiraUsername;
            if (galifrey.AppSettings.JiraPassword != null) txtJiraPassword.Text = galifrey.AppSettings.JiraPassword;
        }

        private void btnCancelEditSettings_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(galifrey.AppSettings.JiraUrl) ||
                    string.IsNullOrWhiteSpace(galifrey.AppSettings.JiraUsername) ||
                    string.IsNullOrWhiteSpace(galifrey.AppSettings.JiraPassword))
            {
                MessageBox.Show("You have to populate the Jira Credentials!", "Missing Config");
                return;
            }
            Close();
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            galifrey.AppSettings.JiraUrl = txtJiraUrl.Text;
            galifrey.AppSettings.JiraUsername = txtJiraUsername.Text;
            galifrey.AppSettings.JiraPassword = txtJiraPassword.Text;

            if (string.IsNullOrWhiteSpace(galifrey.AppSettings.JiraUrl) ||
                    string.IsNullOrWhiteSpace(galifrey.AppSettings.JiraUsername) ||
                    string.IsNullOrWhiteSpace(galifrey.AppSettings.JiraPassword))
            {
                MessageBox.Show("You have to populate the Jira Credentials!", "Missing Config");
                return;
            }

            galifrey.SaveAppSettings();
            Close();
        }
    }
}
