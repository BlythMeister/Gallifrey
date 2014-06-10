using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Windows.Forms;

namespace Gallifrey.UI.Classic
{
    public partial class AboutWindow : Form
    {
        private readonly bool isBeta;
        private readonly IBackend gallifrey;
        private readonly List<string> contributors;
        private int position;

        public AboutWindow(bool isBeta, IBackend gallifrey)
        {
            this.isBeta = isBeta;
            this.gallifrey = gallifrey;
            InitializeComponent();
            var networkDeploy = ApplicationDeployment.IsNetworkDeployed;
            var myVersion = networkDeploy ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : Application.ProductVersion;
            myVersion = string.Format("Current Version: {0}", myVersion);
            if (!networkDeploy) myVersion = string.Format("{0} (manual)", myVersion);
            if (isBeta) myVersion = string.Format("{0} (beta)", myVersion);
            lblCurrentVersion.Text = myVersion;

            contributors = new List<string>()
                {
                    "Mark Harrison\nTwitter: @HarrisonMeister\nGitHub: @HarrisonMeister",
                };

            btnChangeLog.Visible = networkDeploy;
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
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=G3MWL8E6UG4RS");
        }

        private void btnGitHub_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/BlythMeister/Gallifrey");
        }

        private void btnEmail_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:GallifreyApp@gmail.com?subject=Gallifrey App Contact");
        }

        private void btnTwitter_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://twitter.com/GallifreyApp");
        }

        private void btnChangeLog_Click(object sender, EventArgs e)
        {
            var version = ApplicationDeployment.CurrentDeployment.CurrentVersion;
            if (isBeta) version = new Version(version.Major, version.Minor, version.Build, 0);

            var changeLog = gallifrey.GetChangeLog(version);

            if (changeLog.Any())
            {
                var changeLogWindow = new ChangeLogWindow(isBeta, changeLog);
                changeLogWindow.ShowDialog();
            }
        }
    }
}
