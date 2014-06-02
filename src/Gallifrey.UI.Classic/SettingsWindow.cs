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

            if (gallifrey.Settings.JiraConnectionSettings.JiraUrl != null) txtJiraUrl.Text = gallifrey.Settings.JiraConnectionSettings.JiraUrl;
            if (gallifrey.Settings.JiraConnectionSettings.JiraUsername != null) txtJiraUsername.Text = gallifrey.Settings.JiraConnectionSettings.JiraUsername;
            if (gallifrey.Settings.JiraConnectionSettings.JiraPassword != null) txtJiraPassword.Text = gallifrey.Settings.JiraConnectionSettings.JiraPassword;

            chkAlert.Checked = gallifrey.Settings.AppSettings.AlertWhenNotRunning;
            txtAlertMins.Text = ((gallifrey.Settings.AppSettings.AlertTimeMilliseconds / 1000) / 60).ToString();
            txtTimerDays.Text = gallifrey.Settings.AppSettings.KeepTimersForDays.ToString();

            txtTargetHours.Text = gallifrey.Settings.AppSettings.TargetLogPerDay.Hours.ToString();
            txtTargetMins.Text = gallifrey.Settings.AppSettings.TargetLogPerDay.Minutes.ToString();

            chkAlwaysTop.Checked = gallifrey.Settings.UiSettings.AlwaysOnTop;

            TopMost = gallifrey.Settings.UiSettings.AlwaysOnTop;
        }

        private void btnCancelEditSettings_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(gallifrey.Settings.JiraConnectionSettings.JiraUrl) ||
                    string.IsNullOrWhiteSpace(gallifrey.Settings.JiraConnectionSettings.JiraUsername) ||
                    string.IsNullOrWhiteSpace(gallifrey.Settings.JiraConnectionSettings.JiraPassword))
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

            gallifrey.Settings.AppSettings.AlertWhenNotRunning = chkAlert.Checked;
            gallifrey.Settings.AppSettings.AlertTimeMilliseconds = (alertTime * 60) * 1000;
            gallifrey.Settings.AppSettings.KeepTimersForDays = keepTimerDays;
            gallifrey.Settings.UiSettings.AlwaysOnTop = chkAlwaysTop.Checked;

            int hours, minutes;

            if (!int.TryParse(txtTargetHours.Text, out hours)) { hours = 7; }
            if (!int.TryParse(txtTargetMins.Text, out minutes)) { minutes = 30; }

            gallifrey.Settings.AppSettings.TargetLogPerDay = new TimeSpan(hours, minutes, 0);


            gallifrey.Settings.JiraConnectionSettings.JiraUrl = txtJiraUrl.Text;
            gallifrey.Settings.JiraConnectionSettings.JiraUsername = txtJiraUsername.Text;
            gallifrey.Settings.JiraConnectionSettings.JiraPassword = txtJiraPassword.Text;

            if (string.IsNullOrWhiteSpace(gallifrey.Settings.JiraConnectionSettings.JiraUrl) ||
                    string.IsNullOrWhiteSpace(gallifrey.Settings.JiraConnectionSettings.JiraUsername) ||
                    string.IsNullOrWhiteSpace(gallifrey.Settings.JiraConnectionSettings.JiraPassword))
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
                gallifrey.SaveSettings();
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
