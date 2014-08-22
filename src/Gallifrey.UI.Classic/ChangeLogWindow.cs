using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gallifrey.ChangeLog;

namespace Gallifrey.UI.Classic
{
    public partial class ChangeLogWindow : Form
    {
        public ChangeLogWindow(bool isBeta, IDictionary<Version, ChangeLogVersionDetails> changeLog)
        {
            InitializeComponent();
            var networkDeploy = ApplicationDeployment.IsNetworkDeployed;
            var myVersion = networkDeploy ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : Application.ProductVersion;
            if (networkDeploy && !isBeta)
            {
                myVersion = myVersion.Substring(0, myVersion.LastIndexOf("."));
            }
            myVersion = string.Format("Current Version: {0}", myVersion);
            if (!networkDeploy) myVersion = string.Format("{0} (manual)", myVersion);
            if (isBeta) myVersion = string.Format("{0} (beta)", myVersion);
            lblCurrentVersion.Text = myVersion;

            foreach (var changeLogDetail in changeLog)
            {
                txtChangeLog.SelectionFont = new Font(txtChangeLog.SelectionFont.FontFamily, 14, FontStyle.Bold);
                if (string.IsNullOrWhiteSpace(changeLogDetail.Value.Name))
                {
                    txtChangeLog.AppendText(string.Format("Version: {0}\n", changeLogDetail.Key));    
                }
                else
                {
                    txtChangeLog.AppendText(string.Format("Version: {0} ({1})\n", changeLogDetail.Key, changeLogDetail.Value.Name));
                }
                
                if (changeLogDetail.Value.Features.Any())
                {
                    txtChangeLog.SelectionFont = new Font(txtChangeLog.SelectionFont.FontFamily, 12, FontStyle.Underline);
                    txtChangeLog.AppendText("New Features\n");
                    txtChangeLog.SelectionFont = new Font(txtChangeLog.SelectionFont.FontFamily, 10, FontStyle.Regular);
                    foreach (var feature in changeLogDetail.Value.Features)
                    {
                        txtChangeLog.AppendText(string.Format("  => {0}\n", feature));
                    }
                }

                if (changeLogDetail.Value.Bugs.Any())
                {
                    txtChangeLog.SelectionFont = new Font(txtChangeLog.SelectionFont.FontFamily, 12, FontStyle.Underline);
                    txtChangeLog.AppendText("Bug Fixes\n");
                    txtChangeLog.SelectionFont = new Font(txtChangeLog.SelectionFont.FontFamily, 10, FontStyle.Regular);
                    foreach (var feature in changeLogDetail.Value.Bugs)
                    {
                        txtChangeLog.AppendText(string.Format("  => {0}\n", feature));
                    }
                }

                if (changeLogDetail.Value.Others.Any())
                {
                    txtChangeLog.SelectionFont = new Font(txtChangeLog.SelectionFont.FontFamily, 12, FontStyle.Underline);
                    txtChangeLog.AppendText("Other Items\n");
                    txtChangeLog.SelectionFont = new Font(txtChangeLog.SelectionFont.FontFamily, 10, FontStyle.Regular);
                    foreach (var feature in changeLogDetail.Value.Others)
                    {
                        txtChangeLog.AppendText(string.Format("  => {0}\n", feature));
                    }
                }

                txtChangeLog.AppendText("\n");
            }

            txtChangeLog.Select(0,0);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
