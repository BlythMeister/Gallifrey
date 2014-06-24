using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
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
        private readonly Dictionary<DateTime, ThreadedBindingList<JiraTimer>> internalTimerList;

        private DateTime lastUpdateCheck;
        private string myVersion;
        private PrivateFontCollection privateFontCollection;

        public MainWindow(bool isBeta)
        {
            InitializeComponent();
            this.isBeta = isBeta;
            lastUpdateCheck = DateTime.MinValue;
            internalTimerList = new Dictionary<DateTime, ThreadedBindingList<JiraTimer>>();

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

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ExceptionlessClient.Current.UnhandledExceptionReporting += OnUnhandledExceptionReporting;
                ExceptionlessClient.Current.Register();
            }
        }

        private void OnUnhandledExceptionReporting(object sender, UnhandledExceptionReportingEventArgs e)
        {
            foreach (var form in Application.OpenForms.Cast<Form>())
            {
                form.TopMost = false;
            }

            e.Error.Tags.Add(myVersion.Replace("\n", " - "));
            foreach (var module in e.Error.Modules.Where(module => module.Name.ToLower().Contains("gallifrey.ui")))
            {
                module.Version = myVersion.Replace("\n", " - ");
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            Height = gallifrey.Settings.UiSettings.Height;
            Width = gallifrey.Settings.UiSettings.Width;
            Text = isBeta ? "Gallifrey (Beta)" : "Gallifrey";

            SetVersionNumber();
            RefreshInternalTimerList();
            SetupDisplayFont();
            formTimer.Enabled = true;

            if (gallifrey.JiraTimerCollection.GetRunningTimerId().HasValue)
            {
                SelectTimer(gallifrey.JiraTimerCollection.GetRunningTimerId().Value);
            }

            if (ApplicationDeployment.IsNetworkDeployed && ApplicationDeployment.CurrentDeployment.IsFirstRun)
            {
                var version = ApplicationDeployment.CurrentDeployment.CurrentVersion;

                var changeLog = gallifrey.GetChangeLog(version, XDocument.Parse(Properties.Resources.ChangeLog));

                if (changeLog.Any())
                {
                    var changeLogWindow = new ChangeLogWindow(isBeta, changeLog);
                    changeLogWindow.ShowDialog();
                }
            }
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                var selectedTab = tabTimerDays.SelectedTab;
                var tabList = (ListBox)selectedTab.Controls[string.Format("lst_{0}", selectedTab.Name)];

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
                    case Keys.X:
                        MultiExport();
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
                    case Keys.Down:
                        tabList.SelectedIndex = 0;
                        tabList.Focus();
                        break;
                    case Keys.Right:
                        if (selectedTab.TabIndex < tabTimerDays.TabPages.Count - 1)
                        {
                            tabTimerDays.SelectedIndex++;
                            tabList = (ListBox)tabTimerDays.SelectedTab.Controls[string.Format("lst_{0}", tabTimerDays.SelectedTab.Name)];
                            tabList.SelectedIndex = 0;
                            tabList.Focus();
                        }
                        break;
                    case Keys.Left:
                        if (selectedTab.TabIndex > 0)
                        {
                            tabTimerDays.SelectedIndex--;
                            tabList = (ListBox)tabTimerDays.SelectedTab.Controls[string.Format("lst_{0}", tabTimerDays.SelectedTab.Name)];
                            tabList.SelectedIndex = 0;
                            tabList.Focus();
                        }
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
            RefreshInternalTimerList();
            SetDisplayClock();
            SetExportStats();
            SetExportTargetStats();
            CheckIfUpdateCallNeeded();
        }

        #region "Non Button Handlers"

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
                    MessageBox.Show("Use The Version Of This Timer For Today!", "Wrong Day!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            RefreshInternalTimerList();
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

        #endregion

        #region "Button Handlers"

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
                RefreshInternalTimerList();
                if (addForm.NewTimerId.HasValue) SelectTimer(addForm.NewTimerId.Value);
            }
        }

        private void btnRemoveTimer_Click(object sender, EventArgs e)
        {
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab == null) return;
            var selectedTimer = (JiraTimer)((ListBox)selectedTab.Controls[string.Format("lst_{0}", selectedTab.Name)]).SelectedItem;

            if (MessageBox.Show(string.Format("Are You Sure You Want To Remove Timer For '{0}'?", selectedTimer.JiraReference), "Are You Sure", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                gallifrey.JiraTimerCollection.RemoveTimer(selectedTimer.UniqueId);
                RefreshInternalTimerList();
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
            var renameWindow = new EditTimerWindow(gallifrey, selectedTimer.UniqueId);
            renameWindow.ShowDialog();
            RefreshInternalTimerList();
        }

        private void btnTimeEdit_Click(object sender, EventArgs e)
        {
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab == null) return;
            var selectedTimer = (JiraTimer)((ListBox)selectedTab.Controls[string.Format("lst_{0}", selectedTab.Name)]).SelectedItem;
            var adjustTimerWindow = new AdjustTimerWindow(gallifrey, selectedTimer.UniqueId);
            adjustTimerWindow.ShowDialog();
            RefreshInternalTimerList();
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
            RefreshInternalTimerList();
        }

        private void btnIdle_Click(object sender, EventArgs e)
        {
            var lockedTimerWindow = new LockedTimerWindow(gallifrey);
            if (lockedTimerWindow.DisplayForm)
            {
                lockedTimerWindow.ShowDialog();
                RefreshInternalTimerList();
                if (lockedTimerWindow.NewTimerId.HasValue)
                {
                    SelectTimer(lockedTimerWindow.NewTimerId.Value);
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var searchForm = new SearchWindow(gallifrey);
            searchForm.ShowDialog();
            RefreshInternalTimerList();
            if (searchForm.NewTimerId.HasValue) SelectTimer(searchForm.NewTimerId.Value);
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            var aboutForm = new AboutWindow(isBeta, gallifrey);
            aboutForm.ShowDialog();
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
                    if (idleTimer.IdleTimeValue.TotalSeconds < 60 || idleTimer.IdleTimeValue.TotalHours > 10)
                    {
                        gallifrey.IdleTimerCollection.RemoveTimer(idleTimerId);
                    }
                    else
                    {
                        var lockedTimerWindow = new LockedTimerWindow(gallifrey);
                        lockedTimerWindow.Closed += LockedTimerWindowClosed;
                        if (lockedTimerWindow.DisplayForm)
                        {
                            lockedTimerWindow.BringToFront();
                            lockedTimerWindow.Show();
                        }
                    }
                    break;
            }
        }

        private void LockedTimerWindowClosed(object sender, EventArgs e)
        {
            RefreshInternalTimerList();
            var lockedTimerWindow = (LockedTimerWindow)sender;
            if (lockedTimerWindow.NewTimerId.HasValue)
            {
                SelectTimer(lockedTimerWindow.NewTimerId.Value);
            }
        }

        #endregion

        #region "UI Hlpers"

        private void SetupDisplayFont()
        {
            try
            {
                privateFontCollection = new PrivateFontCollection();

                var fontPath = Path.Combine(Environment.CurrentDirectory, "digital7.ttf");
                if (!File.Exists(fontPath))
                {
                    File.WriteAllBytes(fontPath, Properties.Resources.digital7);
                }

                privateFontCollection.AddFontFile(fontPath);

                if (privateFontCollection.Families.Any())
                {
                    lblCurrentTime.Font = new Font(privateFontCollection.Families[0], 50);
                }

            }
            catch (Exception) {/*Intentional - use default font*/}
        }

        private void SetVersionNumber(bool checkingUpdate = false, bool noUpdate = false)
        {
            var networkDeploy = ApplicationDeployment.IsNetworkDeployed;
            myVersion = networkDeploy ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : Application.ProductVersion;

            if (networkDeploy && !isBeta)
            {
                myVersion = myVersion.Substring(0, myVersion.LastIndexOf("."));
            }

            var betaText = isBeta ? " (beta)" : "";
            var upToDateText = "Invalid Deployment";

            if (networkDeploy) upToDateText = "Up To Date!";
            if (checkingUpdate) upToDateText = "Checking Updates!";
            if (noUpdate) upToDateText = "No New Updates!";

            myVersion = string.Format("v{0}{1}\n{2}", myVersion, betaText, upToDateText);
            if (!networkDeploy)
            {
                lblUpdate.BackColor = Color.Red;
                lblUpdate.BorderStyle = BorderStyle.FixedSingle;
            }
            lblUpdate.Text = string.Format("Currently Running {0}", myVersion);
        }

        private void RefreshInternalTimerList()
        {
            var validDates = gallifrey.JiraTimerCollection.GetValidTimerDates().OrderByDescending(x => x.Date);

            //Build Correct Set Of Data Internally
            foreach (var validDate in validDates)
            {
                //Add missing dates
                if (!internalTimerList.ContainsKey(validDate))
                {
                    var list = new ThreadedBindingList<JiraTimer> { RaiseListChangedEvents = true };
                    list.ListChanged += ListOnListChanged;
                    internalTimerList.Add(validDate, list);
                }

                //Add missing dates
                foreach (var jiraTimer in gallifrey.JiraTimerCollection.GetTimersForADate(validDate))
                {
                    if (internalTimerList[validDate].All(x => x.UniqueId != jiraTimer.UniqueId))
                    {
                        internalTimerList[validDate].Add(jiraTimer);
                    }
                }

                //remove timers that have been deleted
                var removeList = internalTimerList[validDate].Where(timer => gallifrey.JiraTimerCollection.GetTimer(timer.UniqueId) == null).ToList();
                foreach (var jiraTimer in removeList)
                {
                    internalTimerList[validDate].Remove(jiraTimer);
                }
            }

            //remove dates that don't have any timers or date is not present
            var removeDates = internalTimerList.Where(x => !x.Value.Any()).Select(x => x.Key).ToList();
            removeDates.AddRange(internalTimerList.Where(x => validDates.All(date => date != x.Key)).Select(x => x.Key));
            foreach (var removeDate in removeDates)
            {
                internalTimerList.Remove(removeDate);
            }

            UpdateTabPages();
        }

        private void UpdateTabPages()
        {
            //Add missing tab pages
            var itteration = 0;
            foreach (var timerlistValue in internalTimerList.OrderByDescending(x=>x.Key))
            {
                var tabName = timerlistValue.Key.Date.ToString("yyyyMMdd");
                var tabListName = string.Format("lst_{0}", tabName);
                var tabDisplay = string.Format("{0} [ {1} ]", timerlistValue.Key.Date.ToString("ddd, dd MMM"), gallifrey.JiraTimerCollection.GetTotalTimeForDate(timerlistValue.Key).FormatAsString());
                var page = tabTimerDays.TabPages[tabName];

                if (page == null)
                {
                    page = new TabPage(tabName) { Name = tabName, Text = tabDisplay };
                    tabTimerDays.TabPages.Insert(itteration, page);
                }

                if (!page.Controls.ContainsKey(tabListName))
                {
                    var timerList = new ListBox { Dock = DockStyle.Fill, Name = tabListName };
                    timerList.DoubleClick += DoubleClickListBox;
                    page.Controls.Add(timerList);
                    timerList.DataSource = timerlistValue.Value;
                }

                itteration++;
            }

            //remove tab pages which are empty
            foreach (TabPage tabPage in tabTimerDays.TabPages)
            {
                var tabDate = DateTime.ParseExact(tabPage.Name, "yyyyMMdd", CultureInfo.InvariantCulture);
                if (internalTimerList.Keys.All(date => date.Date != tabDate.Date))
                {
                    tabTimerDays.TabPages.Remove(tabPage);
                }
            }
        }

        private void ListOnListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            var jiraTimerList = sender as IEnumerable<JiraTimer>;
            if (jiraTimerList != null && jiraTimerList.Any())
            {
                var jiraTimer = jiraTimerList.First();
                var tabName = jiraTimer.DateStarted.Date.ToString("yyyyMMdd");
                var tabDisplay = string.Format("{0} [ {1} ]", jiraTimer.DateStarted.Date.ToString("ddd, dd MMM"), gallifrey.JiraTimerCollection.GetTotalTimeForDate(jiraTimer.DateStarted).FormatAsString());
                var page = tabTimerDays.TabPages[tabName];

                if (page != null && page.Text != tabDisplay) page.Text = tabDisplay;
            }

            SetDisplayClock();
        }

        private void SelectTimer(Guid selectedTimerId)
        {
            foreach (TabPage tabPage in tabTimerDays.TabPages)
            {
                var foundMatch = false;
                var tabList = (ListBox)tabPage.Controls[string.Format("lst_{0}", tabPage.Name)];
                foreach (var item in tabList.Items.Cast<JiraTimer>().Where(item => item.UniqueId == selectedTimerId))
                {
                    try
                    {
                        tabList.SelectedItem = item;
                    }
                    catch (Exception) { }
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
            var selectedList = ((ListBox) selectedTab.Controls[string.Format("lst_{0}", selectedTab.Name)]);
            if (selectedList == null) return;
            
            var selectedTimer = (JiraTimer)selectedList.SelectedItem;
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
            var exportedTime = gallifrey.JiraTimerCollection.GetTotalExportedTimeThisWeek(gallifrey.Settings.AppSettings.StartOfWeek);
            var target = gallifrey.Settings.AppSettings.GetTargetThisWeek();

            lblExportedWeek.Text = string.Format("Exported: {0}", exportedTime.FormatAsString(false));
            lblExportTargetWeek.Text = string.Format("Target: {0}", target.FormatAsString(false));
            progExportTarget.Maximum = (int)target.TotalMinutes;

            if (progExportTarget.Maximum == 0)
            {
                progExportTarget.Maximum = 1;
                progExportTarget.Value = 1;
            }
            else
            {
                var exportedMinutes = (int)exportedTime.TotalMinutes;
                progExportTarget.Value = exportedMinutes > progExportTarget.Maximum ? progExportTarget.Maximum : exportedMinutes;
            }
        }

        private void MultiExport()
        {
            var selectedTimer = (JiraTimer)((ListBox)tabTimerDays.SelectedTab.Controls[string.Format("lst_{0}", tabTimerDays.SelectedTab.Name)]).SelectedItem;

            if (selectedTimer != null)
            {
                foreach (var jiraTimer in gallifrey.JiraTimerCollection.GetUnexportedTimers(selectedTimer.DateStarted))
                {
                    var exportTimerWindow = new ExportTimerWindow(gallifrey, jiraTimer.UniqueId);
                    if (exportTimerWindow.DisplayForm)
                    {
                        exportTimerWindow.ShowDialog();
                    }
                }
            }

            RefreshInternalTimerList();
        }

        #endregion

        #region "Updates

        private void lblUpdate_DoubleClick(object sender, EventArgs e)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                if (ApplicationDeployment.CurrentDeployment.UpdatedVersion != ApplicationDeployment.CurrentDeployment.CurrentVersion)
                {
                    try
                    {
                        Application.Restart();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("An Error Occured When Trying To Restart, Please Restart Manually", "Restart Failure", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    SetVersionNumber(true);
                    CheckForUpdates(true);
                }
            }
            else
            {
                MessageBox.Show("The Version You Are Running Cannot Be Updated!!", "Invalid Version", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void CheckIfUpdateCallNeeded()
        {
            if (ApplicationDeployment.IsNetworkDeployed && lastUpdateCheck < DateTime.UtcNow.AddMinutes(-15))
            {
                SetVersionNumber();
                CheckForUpdates(false);
            }
        }

        private async void CheckForUpdates(bool manualCheck)
        {
            try
            {
                var updateInfo = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate();
                lastUpdateCheck = DateTime.UtcNow;

                if (updateInfo.UpdateAvailable && updateInfo.AvailableVersion > ApplicationDeployment.CurrentDeployment.CurrentVersion)
                {
                    ApplicationDeployment.CurrentDeployment.UpdateCompleted += UpdateComplete;
                    ApplicationDeployment.CurrentDeployment.UpdateAsync();
                }
                else if (manualCheck)
                {
                    await Task.Delay(1500);
                    SetVersionNumber(noUpdate: true);
                }
            }
            catch (Exception)
            {
            }
        }

        private void UpdateComplete(object sender, AsyncCompletedEventArgs e)
        {
            grpUpdates.Text = "Update Avaliable";
            lblUpdate.BackColor = Color.OrangeRed;
            lblUpdate.BorderStyle = BorderStyle.FixedSingle;
            lblUpdate.Text = string.Format("     v{0}\nDouble Click Here To Restart.", ApplicationDeployment.CurrentDeployment.UpdatedVersion);
            lblUpdate.Image = Properties.Resources.Download_16x16;

            notifyAlert.ShowBalloonTip(10000, "Update Avaliable", string.Format("An Update To v{0} Has Been Downloaded!", ApplicationDeployment.CurrentDeployment.UpdatedVersion), ToolTipIcon.Info);
        }

        #endregion
    }
}
