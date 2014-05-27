using System;
using System.Windows.Forms;
using Gallifrey.Exceptions.IntergrationPoints;

namespace Gallifrey.UI.Classic
{
    public partial class SettingsWindow : Form
    {
        private readonly IBackend gallifrey;

        public SettingsWindow(IBackend gallifrey)
        {
            this.gallifrey = gallifrey;
            InitializeComponent();

            if (gallifrey.JiraConnectionSettings.JiraUrl != null) txtJiraUrl.Text = gallifrey.JiraConnectionSettings.JiraUrl;
            if (gallifrey.JiraConnectionSettings.JiraUsername != null) txtJiraUsername.Text = gallifrey.JiraConnectionSettings.JiraUsername;
            if (gallifrey.JiraConnectionSettings.JiraPassword != null) txtJiraPassword.Text = gallifrey.JiraConnectionSettings.JiraPassword;

            chkAlert.Checked = gallifrey.AppSettings.AlertWhenNotRunning;
            txtAlertMins.Text = ((gallifrey.AppSettings.AlertTimeMilliseconds / 1000) / 60).ToString();
            txtTimerDays.Text = gallifrey.AppSettings.KeepTimersForDays.ToString();

            txtTargetHours.Text = gallifrey.AppSettings.TargetLogPerDay.Hours.ToString();
            txtTargetMins.Text = gallifrey.AppSettings.TargetLogPerDay.Minutes.ToString();

            chkAlwaysTop.Checked = gallifrey.AppSettings.UiAlwaysOnTop;

            TopMost = gallifrey.AppSettings.UiAlwaysOnTop;
        }

        private void btnCancelEditSettings_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(gallifrey.JiraConnectionSettings.JiraUrl) ||
                    string.IsNullOrWhiteSpace(gallifrey.JiraConnectionSettings.JiraUsername) ||
                    string.IsNullOrWhiteSpace(gallifrey.JiraConnectionSettings.JiraPassword))
            {
                MessageBox.Show("You have to populate the Jira Credentials!", "Missing Config");
                return;
            }
            CloseWindow();
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            int keepTimerDays, alertTime;
            if (!int.TryParse(txtAlertMins.Text, out alertTime)) alertTime = 0;
            if (!int.TryParse(txtTimerDays.Text, out keepTimerDays)) keepTimerDays = 28;

            gallifrey.AppSettings.AlertWhenNotRunning = chkAlert.Checked;
            gallifrey.AppSettings.AlertTimeMilliseconds = (alertTime * 60) * 1000;
            gallifrey.AppSettings.KeepTimersForDays = keepTimerDays;
            gallifrey.AppSettings.UiAlwaysOnTop = chkAlwaysTop.Checked;

            int hours, minutes;

            if (!int.TryParse(txtTargetHours.Text, out hours)) { hours = 7; }
            if (!int.TryParse(txtTargetMins.Text, out minutes)) { minutes = 30; }

            gallifrey.AppSettings.TargetLogPerDay = new TimeSpan(hours, minutes, 0);


            gallifrey.JiraConnectionSettings.JiraUrl = txtJiraUrl.Text;
            gallifrey.JiraConnectionSettings.JiraUsername = txtJiraUsername.Text;
            gallifrey.JiraConnectionSettings.JiraPassword = txtJiraPassword.Text;
            
            if (string.IsNullOrWhiteSpace(gallifrey.JiraConnectionSettings.JiraUrl) ||
                    string.IsNullOrWhiteSpace(gallifrey.JiraConnectionSettings.JiraUsername) ||
                    string.IsNullOrWhiteSpace(gallifrey.JiraConnectionSettings.JiraPassword))
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
                gallifrey.SaveJiraConnectionSettings();
                gallifrey.SaveAppSettings();
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
