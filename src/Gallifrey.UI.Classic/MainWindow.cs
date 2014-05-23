using System;
using System.Deployment.Application;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gallifrey.Exceptions.IntergrationPoints;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.ExtensionMethods;
using Gallifrey.JiraTimers;

namespace Gallifrey.UI.Classic
{
    public partial class MainWindow : Form
    {
        private readonly IBackend gallifrey;

        public MainWindow()
        {
            InitializeComponent();
            gallifrey = new Backend();
            try
            {
                gallifrey.Initialise();
            }
            catch (MissingJiraConfigException)
            {
                btnSettings_Click(null, null);
            }
            catch (JiraConnectionException)
            {
                btnSettings_Click(null, null);
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            SetVersionNumber();
            RefreshTimerPages();
            SetupDisplayFont();
            formTimer.Enabled = true;
        }

        private void SetupDisplayFont()
        {
            var privateFonts = new PrivateFontCollection();
            
            var resource = string.Empty;
            foreach (var name in GetType().Assembly.GetManifestResourceNames().Where(name => name.Contains("digital7.ttf")))
            {
                resource = name;
                break;
            }

            var fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            var data = Marshal.AllocCoTaskMem((int)fontStream.Length);
            var fontdata = new byte[fontStream.Length];
            fontStream.Read(fontdata, 0, (int)fontStream.Length);
            Marshal.Copy(fontdata, 0, data, (int)fontStream.Length);
            privateFonts.AddMemoryFont(data, (int)fontStream.Length);
            fontStream.Close();
            Marshal.FreeCoTaskMem(data);
            
            lblCurrentTime.Font = new Font(privateFonts.Families[0], 50);
        }

        private void SetVersionNumber()
        {
            var networkDeploy = ApplicationDeployment.IsNetworkDeployed;
            var myVersion = networkDeploy ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : Application.ProductVersion;
            myVersion = string.Format("v{0}", myVersion);
            if (!networkDeploy) myVersion = string.Format("{0} (manual)", myVersion);
            lblVersion.Text = myVersion;
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            gallifrey.Close();
        }

        private void RefreshTimerPages()
        {
            Guid? selectedTimerId = null;
            if (tabTimerDays.SelectedTab != null)
            {
                var selectedTimer = (JiraTimer)((ListBox)tabTimerDays.SelectedTab.Controls[string.Format("lst_{0}", tabTimerDays.SelectedTab.Name)]).SelectedItem;
                selectedTimerId = selectedTimer.UniqueId;
            }

            var timers = gallifrey.JiraTimerCollection;
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
                foreach (var item in tabList.Items.Cast<JiraTimer>().Where(item => item.UniqueId == selectedTimerId))
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
            var runningTimer = gallifrey.JiraTimerCollection.GetRunningTimerId();

            if (runningTimer.HasValue && runningTimer.Value == timerClicked.UniqueId)
            {
                gallifrey.JiraTimerCollection.StopTimer(timerClicked.UniqueId);
            }
            else
            {
                try
                {
                    gallifrey.JiraTimerCollection.StartTimer(timerClicked.UniqueId);
                }
                catch (DuplicateTimerException)
                {
                    MessageBox.Show("Use the version of this timer for today!", "Wrong Day!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            RefreshTimerPages();
        }

        private void btnAddTimer_Click(object sender, EventArgs e)
        {
            var addForm = new AddTimerWindow(gallifrey);
            addForm.ShowDialog();
            RefreshTimerPages();
        }

        private void btnRemoveTimer_Click(object sender, EventArgs e)
        {
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab == null) return;
            var selectedTimer = (JiraTimer)((ListBox)selectedTab.Controls[string.Format("lst_{0}", selectedTab.Name)]).SelectedItem;
            gallifrey.JiraTimerCollection.RemoveTimer(selectedTimer.UniqueId);
            RefreshTimerPages();
        }

        private void formTimer_Tick(object sender, EventArgs e)
        {
            if (gallifrey.JiraTimerCollection.GetRunningTimerId().HasValue)
            {
                RefreshTimerPages();
            }
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab == null) return;
            var selectedTimer = (JiraTimer)((ListBox)selectedTab.Controls[string.Format("lst_{0}", selectedTab.Name)]).SelectedItem;
            lblCurrentTime.Text = selectedTimer.ExactCurrentTime.FormatAsString();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            var settingsWindow = new SettingsWindow(gallifrey);
            settingsWindow.ShowDialog();
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab == null) return;
            var selectedTimer = (JiraTimer)((ListBox)selectedTab.Controls[string.Format("lst_{0}", selectedTab.Name)]).SelectedItem;
            var renameWindow = new RenameTimerWindow(gallifrey, selectedTimer.UniqueId);
            renameWindow.ShowDialog();
            RefreshTimerPages();
        }
    }
}
