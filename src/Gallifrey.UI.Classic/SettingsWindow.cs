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
            txtJiraUrl.Text = galifrey.AppSettings.JiraUrl;
            txtJiraUsername.Text = galifrey.AppSettings.JiraUsername;
            txtJiraPassword.Text = galifrey.AppSettings.JiraPassword;
        }

        private void btnCancelEditSettings_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            galifrey.AppSettings.JiraUrl = txtJiraUrl.Text;
            galifrey.AppSettings.JiraUsername = txtJiraUsername.Text;
            galifrey.AppSettings.JiraPassword = txtJiraPassword.Text;
            galifrey.AppSettings.SaveSettings();
        }
    }
}
