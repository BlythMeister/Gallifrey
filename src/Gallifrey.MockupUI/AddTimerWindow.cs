using System;
using System.Windows.Forms;

namespace Gallifrey.MockupUI
{
    public partial class AddTimerWindow : Form
    {
        private readonly Backend galifrey;
        
        public AddTimerWindow(Backend galifrey)
        {
            this.galifrey = galifrey;
            InitializeComponent();
        }

        private void btnAddTimer_Click(object sender, EventArgs e)
        {
            var jiraReference = txtJiraRef.Text;
            var startDate = calStartDate.Value.Date;
            int hours, minutes;
            int.TryParse(txtStartHours.Text, out hours);
            int.TryParse(txtStartMins.Text, out minutes);

            var seedTime = new TimeSpan(hours, minutes, 0);

            var jiraIssue = galifrey.JiraConnection.GetJiraIssue(jiraReference);

            galifrey.JiraTimerCollection.AddTimer(jiraIssue, startDate, seedTime);
            Close();
        }

        private void btnRemoveTimer_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
