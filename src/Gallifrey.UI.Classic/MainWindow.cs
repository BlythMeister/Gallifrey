using System;
using System.Deployment.Application;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Gallifrey.Exceptions.IntergrationPoints;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.ExtensionMethods;
using Gallifrey.JiraTimers;
using Microsoft.Win32;

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

            gallifrey.NoActivityEvent += GallifreyOnNoActivityEvent;
            SystemEvents.SessionSwitch += SessionSwitchHandler;
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            Height = gallifrey.AppSettings.UiHeight;
            Width = gallifrey.AppSettings.UiWidth;

            SetVersionNumber();
            RefreshTimerPages();
            SetupDisplayFont();
            SetToolTips();
            HandleUnexpectedErrors();
            formTimer.Enabled = true;
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            gallifrey.AppSettings.UiHeight = Height;
            gallifrey.AppSettings.UiWidth = Width;
            gallifrey.Close();
            notifyAlert.Visible = false;
        }

        private void formTimer_Tick(object sender, EventArgs e)
        {
            if (gallifrey.JiraTimerCollection.GetRunningTimerId().HasValue)
            {
                RefreshTimerPages();
            }

            SetDisplayClock();
            SetExportStats();
            SetExportTargetStats();
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
            var runningId = gallifrey.JiraTimerCollection.GetRunningTimerId();
            if (runningId.HasValue)
            {
                SelectTimer(runningId.Value);
            }
        }

        private void lblCurrentTime_DoubleClick(object sender, EventArgs e)
        {
            var runningId = gallifrey.JiraTimerCollection.GetRunningTimerId();
            if (runningId.HasValue)
            {
                SelectTimer(runningId.Value);
            }
        }

        #region "Button Handlers

        private void btnAddTimer_Click(object sender, EventArgs e)
        {
            var addForm = new AddTimerWindow(gallifrey);
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab != null)
            {
                if (gallifrey.JiraTimerCollection.GetTimersForADate(DateTime.Now.Date).Any())
                {
                    var tabDate = DateTime.ParseExact(selectedTab.Name, "yyyyMMdd", CultureInfo.InvariantCulture);
                    addForm.PreLoadDate(tabDate);
                }
            }

            addForm.ShowDialog();
            RefreshTimerPages();
            if (addForm.NewTimerId.HasValue) SelectTimer(addForm.NewTimerId.Value);
        }

        private void btnRemoveTimer_Click(object sender, EventArgs e)
        {
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab == null) return;
            var selectedTimer = (JiraTimer)((ListBox)selectedTab.Controls[string.Format("lst_{0}", selectedTab.Name)]).SelectedItem;

            if (MessageBox.Show(string.Format("Are you sure you want to remove timer for '{0}'?", selectedTimer.JiraReference), "Are You Sure", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                gallifrey.JiraTimerCollection.RemoveTimer(selectedTimer.UniqueId);
                RefreshTimerPages();
            }
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

        private void btnTimeEdit_Click(object sender, EventArgs e)
        {
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab == null) return;
            var selectedTimer = (JiraTimer)((ListBox)selectedTab.Controls[string.Format("lst_{0}", selectedTab.Name)]).SelectedItem;
            var adjustTimerWindow = new AdjustTimerWindow(gallifrey, selectedTimer.UniqueId);
            adjustTimerWindow.ShowDialog();
            RefreshTimerPages();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab == null) return;
            var selectedTimer = (JiraTimer)((ListBox)selectedTab.Controls[string.Format("lst_{0}", selectedTab.Name)]).SelectedItem;
            var exportTimerWindow = new ExportTimerWindow(gallifrey, selectedTimer.UniqueId);
            if (exportTimerWindow.DisplayForm)
            {
                exportTimerWindow.ShowDialog();
            }
            RefreshTimerPages();
        }

        private void btnIdle_Click(object sender, EventArgs e)
        {
            var idleTimerWindow = new LockedTimerWindow(gallifrey);
            if (idleTimerWindow.DisplayForm)
            {
                idleTimerWindow.ShowDialog();
                RefreshTimerPages();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var searchForm = new SearchWindow(gallifrey);
            searchForm.ShowDialog();
            RefreshTimerPages();
        }

        #endregion

        #region "Tray Icon"

        private void GallifreyOnNoActivityEvent(object sender, int millisecondsSinceActivity)
        {
            var minutesSinceActivity = (millisecondsSinceActivity / 1000) / 60;
            var minutesPlural = string.Empty;
            if (minutesSinceActivity > 1)
            {
                minutesPlural = "s";
            }

            notifyAlert.BalloonTipText = string.Format("No Timer Running For {0} Minute{1}", minutesSinceActivity, minutesPlural);
            notifyAlert.ShowBalloonTip(3000);
        }

        private void notifyAlert_BalloonTipClicked(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case FormWindowState.Minimized:
                    WindowState = FormWindowState.Normal;
                    break;
                default:
                    BringToFront();
                    break;
            }
        }

        private void notifyAlert_DoubleClick(object sender, EventArgs e)
        {
            notifyAlert.ShowBalloonTip(5000);
        }

        #endregion

        #region "Session Management"

        private void SessionSwitchHandler(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    gallifrey.StartIdleTimer();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    var idleTimerId = gallifrey.StopIdleTimer();
                    var idleTimer = gallifrey.IdleTimerCollection.GetTimer(idleTimerId);
                    if (idleTimer.ExactCurrentTime.TotalSeconds < 60)
                    {
                        MessageBox.Show("Machine Locked For Less Than 1 Minute.\nLocked Time Was Not Captured", "Short Lock Time", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        gallifrey.IdleTimerCollection.RemoveTimer(idleTimerId);
                    }
                    else
                    {
                        var idleTimerWindow = new LockedTimerWindow(gallifrey);
                        if (idleTimerWindow.DisplayForm)
                        {
                            idleTimerWindow.BringToFront();
                            idleTimerWindow.ShowDialog();
                            RefreshTimerPages();
                        }
                    }
                    break;
            }
        }

        #endregion

        #region "UI Hlpers"

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
                var tabDisplay = string.Format("{0} [ {1} ]", timerDate.Date.ToString("ddd, dd MMM"), timers.GetTotalTimeForDate(timerDate).FormatAsString());
                var page = tabTimerDays.TabPages[tabName];

                if (page == null)
                {
                    page = new TabPage(tabName) { Name = tabName, Text = tabDisplay };
                    tabTimerDays.TabPages.Insert(itteration, page);
                }
                else
                {
                    page.Text = tabDisplay;
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

        private void SetDisplayClock()
        {
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab == null) return;
            var selectedTimer = (JiraTimer)((ListBox)selectedTab.Controls[string.Format("lst_{0}", selectedTab.Name)]).SelectedItem;
            lblCurrentTime.Text = selectedTimer.ExactCurrentTime.FormatAsString();

            if (gallifrey.JiraTimerCollection.GetRunningTimerId().HasValue)
            {
                if (selectedTimer.IsRunning)
                {
                    lblCurrentTime.ForeColor = Color.LimeGreen;
                }
                else
                {
                    lblCurrentTime.ForeColor = Color.Orange;
                }
            }
            else
            {
                lblCurrentTime.ForeColor = Color.Red;
            }
        }

        private void SetExportStats()
        {
            var numbersExported = gallifrey.JiraTimerCollection.GetNumberExported();
            lblExportStat.Text = string.Format("Exported: {0}/{1}", numbersExported.Item1, numbersExported.Item2);

            lblUnexportedTime.Text = string.Format("Un-Exported Time: {0}", gallifrey.JiraTimerCollection.GetTotalUnexportedTime().FormatAsString(false));
        }

        private void SetExportTargetStats()
        {
            var exportedTime = gallifrey.JiraTimerCollection.GetTotalExportedTimeThisWeek();
            var target = new TimeSpan();
            var n = 0;
            while (n++ < (int)DateTime.Today.DayOfWeek)
            {
                target = target.Add(gallifrey.AppSettings.TargetLogPerDay);
            }

            lblExportedWeek.Text = string.Format("Exported: {0}", exportedTime.FormatAsString(false));
            lblExportTargetWeek.Text = string.Format("Target: {0}", target.FormatAsString(false));
            progExportTarget.Maximum = (int)target.TotalMinutes;

            var exportedMinutes = (int)exportedTime.TotalMinutes;
            progExportTarget.Value = exportedMinutes > progExportTarget.Maximum ? progExportTarget.Maximum : exportedMinutes;
        }

        private void SetToolTips()
        {
            toolTip.SetToolTip(btnAddTimer, "Add New Timer");
            toolTip.SetToolTip(btnRemoveTimer, "Remove Selected Timer");
            toolTip.SetToolTip(btnSearch, "Search Jira");
            toolTip.SetToolTip(btnTimeEdit, "Edit Current Time");
            toolTip.SetToolTip(btnRename, "Change Jira For Timer");
            toolTip.SetToolTip(btnExport, "Export Time To Jira");
            toolTip.SetToolTip(btnIdle, "View Machine Locked Timers");
            toolTip.SetToolTip(btnSettings, "Settings");
        }

        #endregion

        #region "Unhandled Errors"

        private void HandleUnexpectedErrors()
        {
            Application.ThreadException += ThreadExceptionHandler;
            AppDomain.CurrentDomain.UnhandledException += UnhanhdledExceptionHandler;
        }

        private void UnhanhdledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            HandleUnexpectedError();
        }

        private void ThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
        {
            HandleUnexpectedError();
        }

        private void HandleUnexpectedError()
        {
            if (MessageBox.Show("Sorry An Unexpected Error Has Occured!\n\nDo You Want To Restart The App?", "Unexpected Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(Application.ExecutablePath);
            }

            MainWindow_FormClosed(null, null);
            Application.ExitThread();
        }

        #endregion
    }
}
