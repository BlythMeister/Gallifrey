using System;
using System.ComponentModel;
using System.Deployment.Application;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Exceptionless;
using Gallifrey.Exceptions.IntergrationPoints;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.ExtensionMethods;
using Gallifrey.JiraTimers;
using Microsoft.Win32;

namespace Gallifrey.UI.Classic
{
    public partial class MainWindow : Form
    {
        private readonly bool isBeta;
        private readonly IBackend gallifrey;

        public MainWindow(bool isBeta)
        {
            this.isBeta = isBeta;
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

            Text = isBeta ? "Gallifrey (Beta)" : "Gallifrey";

            ExceptionlessClient.Current.Register();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            Height = gallifrey.Settings.UiSettings.Height;
            Width = gallifrey.Settings.UiSettings.Width;
            
            SetVersionNumber();
            RefreshTimerPages();
            SetupDisplayFont();
            SetToolTips();
            formTimer.Enabled = true;

            if (gallifrey.JiraTimerCollection.GetRunningTimerId().HasValue)
            {
                SelectTimer(gallifrey.JiraTimerCollection.GetRunningTimerId().Value);
            }
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        btnAddTimer_Click(sender, null);
                        break;
                    case Keys.D:
                        btnRemoveTimer_Click(sender, null);
                        break;
                    case Keys.F:
                        btnSearch_Click(sender, null);
                        break;
                    case Keys.C:
                        btnTimeEdit_Click(sender, null);
                        break;
                    case Keys.R:
                        btnRename_Click(sender, null);
                        break;
                    case Keys.E:
                        btnExport_Click(sender, null);
                        break;
                    case Keys.L:
                        btnIdle_Click(sender, null);
                        break;
                    case Keys.I:
                        btnAbout_Click(sender, null);
                        break;
                    case Keys.S:
                        btnSettings_Click(sender, null);
                        break;
                    case Keys.J:
                        lblCurrentTime_DoubleClick(sender, null);
                        break;
                }
            }
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            gallifrey.Settings.UiSettings.Height = Height;
            gallifrey.Settings.UiSettings.Width = Width;
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
            CheckForUpdate();
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
            if (addForm.DisplayForm)
            {
                addForm.ShowDialog();
                RefreshTimerPages();
                if (addForm.NewTimerId.HasValue) SelectTimer(addForm.NewTimerId.Value);
            }
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
            var lockedTimerWindow = new LockedTimerWindow(gallifrey);
            if (lockedTimerWindow.DisplayForm)
            {
                lockedTimerWindow.ShowDialog();
                RefreshTimerPages();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var searchForm = new SearchWindow(gallifrey);
            searchForm.ShowDialog();
            RefreshTimerPages();
            if (searchForm.NewTimerId.HasValue) SelectTimer(searchForm.NewTimerId.Value);
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            var aboutForm = new AboutWindow(isBeta);
            aboutForm.ShowDialog();
        }
        
        private void lblUpdate_Click(object sender, EventArgs e)
        {
            Application.Restart();
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
                    var openForms = Application.OpenForms.Cast<Form>().ToList();
                    foreach (var form in openForms.Where(form => form.Name != "MainWindow"))
                    {
                        form.Close();
                    }

                    gallifrey.StartIdleTimer();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    BringToFront();
                    var idleTimerId = gallifrey.StopIdleTimer();
                    var idleTimer = gallifrey.IdleTimerCollection.GetTimer(idleTimerId);
                    if (idleTimer.ExactCurrentTime.TotalSeconds < 60 || idleTimer.ExactCurrentTime.TotalHours > 10)
                    {
                        gallifrey.IdleTimerCollection.RemoveTimer(idleTimerId);
                    }
                    else
                    {
                        var lockedTimerWindow = new LockedTimerWindow(gallifrey);
                        lockedTimerWindow.Closed += (o, args) => RefreshTimerPages();
                        if (lockedTimerWindow.DisplayForm)
                        {
                            lockedTimerWindow.BringToFront();
                            lockedTimerWindow.Show();
                        }
                    }
                    break;
            }
        }

        #endregion

        #region "UI Hlpers"

        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

        private void SetupDisplayFont()
        {
            var fontArray = Properties.Resources.digital7;
            var dataLenth = fontArray.Length;

            var ptrData = Marshal.AllocCoTaskMem(dataLenth);
            Marshal.Copy(fontArray, 0, ptrData, dataLenth);
            uint cfonts = 0;
            AddFontMemResourceEx(ptrData, (uint) dataLenth, IntPtr.Zero, ref cfonts);
            
            var privateFonts = new PrivateFontCollection();

            privateFonts.AddMemoryFont(ptrData, dataLenth);
            Marshal.FreeCoTaskMem(ptrData);

            lblCurrentTime.Font = new Font(privateFonts.Families[0], 50);
        }

        private void SetVersionNumber()
        {
            var networkDeploy = ApplicationDeployment.IsNetworkDeployed;
            var myVersion = networkDeploy ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : Application.ProductVersion;
            myVersion = string.Format("v{0}", myVersion);
            if (!networkDeploy) myVersion = string.Format("{0} (manual)", myVersion);
            if (isBeta) myVersion = string.Format("{0} (beta)", myVersion);
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
                    break;
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
                target = target.Add(gallifrey.Settings.AppSettings.TargetLogPerDay);
            }

            lblExportedWeek.Text = string.Format("Exported: {0}", exportedTime.FormatAsString(false));
            lblExportTargetWeek.Text = string.Format("Target: {0}", target.FormatAsString(false));
            progExportTarget.Maximum = (int)target.TotalMinutes;

            var exportedMinutes = (int)exportedTime.TotalMinutes;
            progExportTarget.Value = exportedMinutes > progExportTarget.Maximum ? progExportTarget.Maximum : exportedMinutes;
        }

        private void CheckForUpdate()
        {
            if (ApplicationDeployment.IsNetworkDeployed &&
                ApplicationDeployment.CurrentDeployment.TimeOfLastUpdateCheck < DateTime.UtcNow.AddMinutes(-30))
            {
                try
                {
                    var updateInfo = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate();

                    if (updateInfo.UpdateAvailable && updateInfo.AvailableVersion > ApplicationDeployment.CurrentDeployment.CurrentVersion)
                    {
                        ApplicationDeployment.CurrentDeployment.UpdateCompleted += (sender, args) => lblUpdate.Visible = true;
                        ApplicationDeployment.CurrentDeployment.UpdateAsync();
                    }
                }
                catch (Exception) { }
                
            }
        }

        private void SetToolTips()
        {
            toolTip.SetToolTip(btnAddTimer, "Add New Timer (CTRL+A)");
            toolTip.SetToolTip(btnRemoveTimer, "Remove Selected Timer (CTRL+D)");
            toolTip.SetToolTip(btnSearch, "Search Jira (CTRL+F)");
            toolTip.SetToolTip(btnTimeEdit, "Edit Current Time (CTRL+C)");
            toolTip.SetToolTip(btnRename, "Change Jira For Timer (CTRL+R)");
            toolTip.SetToolTip(btnExport, "Export Time To Jira (CTRL+E)");
            toolTip.SetToolTip(btnIdle, "View Machine Locked Timers (CTRL+L)");
            toolTip.SetToolTip(btnAbout, "About (CTRL+I)");
            toolTip.SetToolTip(btnSettings, "Settings (CTRL+S)");
            toolTip.SetToolTip(lblCurrentTime, "Double Click Jump To Running (CTRL+J)");
        }

        #endregion

    }
}
