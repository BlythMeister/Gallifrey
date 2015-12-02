using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Gallifrey.Contributors;
using Gallifrey.UI.Classic.Properties;

namespace Gallifrey.UI.Classic
{
    public partial class AboutWindow : Form
    {
        private readonly IBackend gallifrey;
        private readonly List<string> contributors;
        private int position;

        public AboutWindow(IBackend gallifrey)
        {
            this.gallifrey = gallifrey;
            InitializeComponent();
            lblCurrentVersion.Text = string.Format("Current Version: {0}", gallifrey.VersionControl.VersionName);
            lblInstallId.Text = string.Format("Install ID: {0}", gallifrey.Settings.InternalSettings.InstallationInstaceId);

            contributors = gallifrey.WithThanksDefinitions.Select(BuildThanksString).ToList();
            timerContributor_Tick(this, null);

            btnChangeLog.Visible = gallifrey.VersionControl.IsAutomatedDeploy;
        }

        private string BuildThanksString(WithThanksDefinition withThanksDefinition)
        {
            var returnString = new StringBuilder();

            returnString.AppendLine(withThanksDefinition.Name);

            if (!string.IsNullOrWhiteSpace(withThanksDefinition.ThanksReason)) returnString.AppendLine(string.Format("Reason: {0}", withThanksDefinition.ThanksReason));
            if (!string.IsNullOrWhiteSpace(withThanksDefinition.TwitterHandle)) returnString.AppendLine(string.Format("Twitter: {0}", withThanksDefinition.TwitterHandle));
            if (!string.IsNullOrWhiteSpace(withThanksDefinition.GitHubHandle)) returnString.AppendLine(string.Format("GitHub: {0}", withThanksDefinition.GitHubHandle));

            return returnString.ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void timerContributor_Tick(object sender, EventArgs e)
        {
            lblContributors.Text = contributors[position];
            position++;
            if (contributors.Count == position)
            {
                position = 0;
            }
        }

        private void btnPayPal_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=G3MWL8E6UG4RS");
        }

        private void btnGitHub_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/BlythMeister/Gallifrey");
        }

        private void btnEmail_Click(object sender, EventArgs e)
        {
            Process.Start("mailto:contact@gallifreyapp.co.uk?subject=Gallifrey App Contact");
        }

        private void btnTwitter_Click(object sender, EventArgs e)
        {
            Process.Start("https://twitter.com/GallifreyApp");
        }

        private void btnChangeLog_Click(object sender, EventArgs e)
        {
            var changeLog = gallifrey.GetChangeLog(XDocument.Parse(Resources.ChangeLog));

            if (changeLog.Any())
            {
                var changeLogWindow = new ChangeLogWindow(gallifrey, changeLog);
                changeLogWindow.ShowDialog();
            }
        }
    }
}
