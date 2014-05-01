using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Gallifrey.JiraTimers;

namespace Gallifrey.UI
{
    public partial class MainWindow : Form
    {
        private readonly Backend galifrey;

        public MainWindow()
        {
            InitializeComponent();
            galifrey = new Backend();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            RefreshTimerPages();
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            galifrey.JiraTimerCollection.SaveTimers();
        }

        private void RefreshTimerPages()
        {
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
        }

        private void DoubleClickListBox(object sender, EventArgs e)
        {
            var timerClicked = (JiraTimer) ((ListBox) sender).SelectedItem;
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
                galifrey.JiraTimerCollection.StartTimer(timerClicked.UniqueId);
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
            var selectedTimer = (JiraTimer)((ListBox)selectedTab.Controls[string.Format("lst_{0}", selectedTab.Name)]).SelectedItem;
            galifrey.JiraTimerCollection.RemoveTimer(selectedTimer.UniqueId);
            RefreshTimerPages();
        }
    }
}
