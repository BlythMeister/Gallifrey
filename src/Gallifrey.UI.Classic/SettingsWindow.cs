using System;
using System.Windows.Forms;
using Gallifrey.Exceptions.IntergrationPoints;

namespace Gallifrey.UI.Classic
{
    public partial class SettingsWindow : Form
    {
        private readonly Backend galifrey;

        public SettingsWindow(Backend galifrey)
        {
            this.galifrey = galifrey;
            InitializeComponent();

            if (galifrey.JiraConnnectionSettings.JiraUrl != null) txtJiraUrl.Text = galifrey.JiraConnnectionSettings.JiraUrl;
            if (galifrey.JiraConnnectionSettings.JiraUsername != null) txtJiraUsername.Text = galifrey.JiraConnnectionSettings.JiraUsername;
            if (galifrey.JiraConnnectionSettings.JiraPassword != null) txtJiraPassword.Text = galifrey.JiraConnnectionSettings.JiraPassword;
        }

        private void btnCancelEditSettings_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(galifrey.JiraConnnectionSettings.JiraUrl) ||
                    string.IsNullOrWhiteSpace(galifrey.JiraConnnectionSettings.JiraUsername) ||
                    string.IsNullOrWhiteSpace(galifrey.JiraConnnectionSettings.JiraPassword))
            {
                MessageBox.Show("You have to populate the Jira Credentials!", "Missing Config");
                return;
            }
            CloseWindow();
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            galifrey.JiraConnnectionSettings.JiraUrl = txtJiraUrl.Text;
            galifrey.JiraConnnectionSettings.JiraUsername = txtJiraUsername.Text;
            galifrey.JiraConnnectionSettings.JiraPassword = txtJiraPassword.Text;

            if (string.IsNullOrWhiteSpace(galifrey.JiraConnnectionSettings.JiraUrl) ||
                    string.IsNullOrWhiteSpace(galifrey.JiraConnnectionSettings.JiraUsername) ||
                    string.IsNullOrWhiteSpace(galifrey.JiraConnnectionSettings.JiraPassword))
            {
                MessageBox.Show("You have to populate the Jira Credentials!", "Missing Config");
                return;
            }
            
            CloseWindow();
        }

        private void CloseWindow()
        {
            try
            {
                galifrey.SaveJiraConnectionSettings();
            }
            catch (JiraConnectionException)
            {
                MessageBox.Show("Unable to connect to Jira with these settings!", "Unable to connect");
                return;
            }

            Close();
        }

    }
}
