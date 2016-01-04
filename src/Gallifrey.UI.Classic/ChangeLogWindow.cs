using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gallifrey.ChangeLog;

namespace Gallifrey.UI.Classic
{
    public partial class ChangeLogWindow : Form
    {
        public ChangeLogWindow(IBackend gallifrey, IEnumerable<ChangeLogVersion> changeLog)
        {
            InitializeComponent();
            
            lblCurrentVersion.Text = $"Current Version: {gallifrey.VersionControl.VersionName}";

            foreach (var changeLogDetail in changeLog)
            {
                txtChangeLog.SelectionFont = new Font(txtChangeLog.SelectionFont.FontFamily, 14, FontStyle.Bold);
                if (string.IsNullOrWhiteSpace(changeLogDetail.Name))
                {
                    txtChangeLog.AppendText($"Version: {changeLogDetail.Version}\n");    
                }
                else
                {
                    txtChangeLog.AppendText($"Version: {changeLogDetail.Version} ({changeLogDetail.Name})\n");
                }
                
                if (changeLogDetail.Features.Any())
                {
                    txtChangeLog.SelectionFont = new Font(txtChangeLog.SelectionFont.FontFamily, 12, FontStyle.Underline);
                    txtChangeLog.AppendText("New Features\n");
                    txtChangeLog.SelectionFont = new Font(txtChangeLog.SelectionFont.FontFamily, 10, FontStyle.Regular);
                    foreach (var feature in changeLogDetail.Features)
                    {
                        txtChangeLog.AppendText($"  => {feature}\n");
                    }
                }

                if (changeLogDetail.Bugs.Any())
                {
                    txtChangeLog.SelectionFont = new Font(txtChangeLog.SelectionFont.FontFamily, 12, FontStyle.Underline);
                    txtChangeLog.AppendText("Bug Fixes\n");
                    txtChangeLog.SelectionFont = new Font(txtChangeLog.SelectionFont.FontFamily, 10, FontStyle.Regular);
                    foreach (var feature in changeLogDetail.Bugs)
                    {
                        txtChangeLog.AppendText($"  => {feature}\n");
                    }
                }

                if (changeLogDetail.Others.Any())
                {
                    txtChangeLog.SelectionFont = new Font(txtChangeLog.SelectionFont.FontFamily, 12, FontStyle.Underline);
                    txtChangeLog.AppendText("Other Items\n");
                    txtChangeLog.SelectionFont = new Font(txtChangeLog.SelectionFont.FontFamily, 10, FontStyle.Regular);
                    foreach (var feature in changeLogDetail.Others)
                    {
                        txtChangeLog.AppendText($"  => {feature}\n");
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
