using System;
using System.Deployment.Application;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Gallifrey.Exceptions.IntergrationPoints;
using Gallifrey.JiraTimers;

namespace Gallifrey.UI.Classic
{
    public partial class MainWindow : Form
    {
        private Backend galifrey;

        public MainWindow()
        {
            InitializeComponent();
            SetupGallifrey();
        }

        private void SetupGallifrey(bool retry = false)
        {
            try
            {
                galifrey = new Backend();
            }
            catch (MissingJiraConfigException)
            {
                if (!retry)
                {
                    btnSettings_Click(null, null);
                    SetupGallifrey(true);
                }
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            SetVersionNumber();
            RefreshTimerPages();
            formTimer.Enabled = true;
        }

        private void SetVersionNumber()
        {
            var myVersion = string.Empty;
            var networkDeploy = ApplicationDeployment.IsNetworkDeployed;
            myVersion = networkDeploy ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : Application.ProductVersion;
            myVersion = string.Format("v{0}", myVersion);
            if (!networkDeploy) myVersion = string.Format("{0} (manual)", myVersion);
            lblVersion.Text = myVersion;
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            galifrey.JiraTimerCollection.SaveTimers();
        }

        private void RefreshTimerPages()
        {
            Guid? selectedTimerId = null;
            if (tabTimerDays.SelectedTab != null)
            {
                var selectedTimer = (JiraTimer)((ListBox)tabTimerDays.SelectedTab.Controls[string.Format("lst_{0}", tabTimerDays.SelectedTab.Name)]).SelectedItem;
                selectedTimerId = selectedTimer.UniqueId;
            }

            var timers = galifrey.JiraTimerCollection;
            var validDates = timers.GetValidTimerDates().OrderByDescending(x => x.Date).ToList();

            foreach (TabPage tabPage in tabTimerDays.TabPages)
            {
                var tabDate = DateTime.ParseExact(tabPage.Name, "yyyyMMdd", CultureInfo.InvariantCulture);
                if (validDates.All(date => date.Date != tabDate.Date))
                {
                    tabTimerDays.TabPages.Remove(tabPage);
                }
            }

            var itteration = 0;
            foreach (var timerDate in validDates)
            {
                var timersForDate = timers.GetTimersForADate(timerDate).ToArray();

                var tabName = timerDate.Date.ToString("yyyyMMdd");
                var tabListName = string.Format("lst_{0}", tabName);
                var tabDisplay = timerDate.Date.ToString("ddd, dd MMM");
                var page = tabTimerDays.TabPages[tabName];

                if (page == null)
                {
                    page = new TabPage(tabName) { Name = tabName, Text = tabDisplay };
                    tabTimerDays.TabPages.Insert(itteration, page);
                }

                ListBox timerList;
                if (page.Controls.ContainsKey(tabListName))
                {
                    timerList = (ListBox)page.Controls[tabListName];
                }
                else
                {
                    timerList = new ListBox { Dock = DockStyle.Fill, Name = tabListName };
                    timerList.DoubleClick += DoubleClickListBox;
                    page.Controls.Add(timerList);
                }

                timerList.DataSource = timersForDate;
                itteration++;
            }

            if (selectedTimerId.HasValue)
            { 
                SelectTimer(selectedTimerId.Value); 
            }
        }

        private void SelectTimer(Guid selectedTimerId)
        {
            foreach (TabPage tabPage in tabTimerDays.TabPages)
            {
                var foundMatch = false;
                var tabList = (ListBox)tabPage.Controls[string.Format("lst_{0}", tabPage.Name)];
                foreach (JiraTimer item in tabList.Items.Cast<JiraTimer>().Where(item => item.UniqueId == selectedTimerId))
                {
                    tabList.SelectedItem = item;
                    foundMatch = true;
                    break;
                }

                if (foundMatch)
                {
                    tabTimerDays.SelectedTab = tabPage;
                }
            }
        }

        private void DoubleClickListBox(object sender, EventArgs e)
        {
            var timerClicked = (JiraTimer)((ListBox)sender).SelectedItem;
            var runningTimers = galifrey.JiraTimerCollection.GetRunningTimers();

            var wasRunning = runningTimers.Any(timer => timer.UniqueId == timerClicked.UniqueId);

            if (wasRunning)
            {
                galifrey.JiraTimerCollection.StopTimer(timerClicked.UniqueId);
            }
            else
            {
                foreach (var runningTimer in runningTimers)
                {
                    galifrey.JiraTimerCollection.StopTimer(runningTimer.UniqueId);
                }
                if (galifrey.JiraTimerCollection.IsTimerForToday(timerClicked.UniqueId))
                {
                    galifrey.JiraTimerCollection.StartTimer(timerClicked.UniqueId);    
                }
                else
                {
                    MessageBox.Show("Cannot Start as not valid for today!", "Wrong Day!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            RefreshTimerPages();
        }

        private void btnAddTimer_Click(object sender, EventArgs e)
        {
            var addForm = new AddTimerWindow(galifrey);
            addForm.ShowDialog();
            RefreshTimerPages();
        }

        private void btnRemoveTimer_Click(object sender, EventArgs e)
        {
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab == null) return;
            var selectedTimer = (JiraTimer)((ListBox)selectedTab.Controls[string.Format("lst_{0}", selectedTab.Name)]).SelectedItem;
            galifrey.JiraTimerCollection.RemoveTimer(selectedTimer.UniqueId);
            RefreshTimerPages();
        }

        private void formTimer_Tick(object sender, EventArgs e)
        {
            RefreshTimerPages();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            var settingsWindow = new SettingsWindow(galifrey);
            settingsWindow.ShowDialog();
        }
    }
}
