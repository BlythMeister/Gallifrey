using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Exceptionless;
using Gallifrey.Comparers;
using Gallifrey.Exceptions.IdleTimers;
using Gallifrey.Exceptions.JiraIntegration;
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
        private bool updateReady;

        #region "Main Window & Error"

        public MainWindow(bool isBeta)
        {
            InitializeComponent();

            this.isBeta = isBeta;
            updateReady = false;
            lastUpdateCheck = DateTime.UtcNow;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ExceptionlessClient.Current.UnhandledExceptionReporting += OnUnhandledExceptionReporting;
                ExceptionlessClient.Current.Register();
            }

            internalTimerList = new Dictionary<DateTime, ThreadedBindingList<JiraTimer>>();

            gallifrey = new Backend();
            try
            {
                gallifrey.Initialise();
            }
            catch (MissingJiraConfigException)
            {
                ShowSettings(false);
            }
            catch (JiraConnectionException)
            {
                ShowSettings(false);
            }

            gallifrey.NoActivityEvent += GallifreyOnNoActivityEvent;
            gallifrey.ExportPromptEvent += GallifreyOnExportPromptEvent;
            SystemEvents.SessionSwitch += SessionSwitchHandler;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                var updateResult = CheckForUpdates();
                updateResult.Wait();

                if (updateResult.Result)
                {
                    UpdateComplete();
                }
                else
                {
                    SetVersionNumber(noUpdate: true);
                }
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
                if (selectedTab == null) return;

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

        #endregion

        #region "Non Button Handlers"

        private void ListBoxDoubleClick(object sender, EventArgs e)
        {
            var timerClicked = GetSelectedTimer();
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

        private void ListBoxMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var senderList = (ListBox)sender;
                var item = senderList.IndexFromPoint(e.Location);
                if (item >= 0)
                {
                    senderList.SelectedIndex = item;
                    senderList.ContextMenu = BuildTimerListContextMenu((JiraTimer)senderList.SelectedItem);
                }
                else
                {
                    senderList.ContextMenu = BuildTimerListContextMenu(null);
                }
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

        private void formTimer_Tick(object sender, EventArgs e)
        {
            RefreshInternalTimerList();
            SetDisplayClock();
            SetExportStats();
            SetExportTargetStats();
            CheckIfUpdateCallNeeded();
        }

        private void GallifreyOnExportPromptEvent(object sender, ExportPromptDetail e)
        {
            var timer = gallifrey.JiraTimerCollection.GetTimer(e.TimerId);
            if (timer != null)
            {
                var exportTime = e.ExportTime;
                var message = string.Format("Do You Want To Export '{0}'?\n", timer.JiraReference);
                if (gallifrey.Settings.AppSettings.ExportPromptAll || (new TimeSpan(exportTime.Ticks - (exportTime.Ticks % 600000000)) == new TimeSpan(timer.TimeToExport.Ticks - (timer.TimeToExport.Ticks % 600000000))))
                {
                    exportTime = timer.TimeToExport;
                    message += string.Format("You Have '{0}' To Export", exportTime.FormatAsString(false));
                }
                else
                {
                    message += string.Format("You Have '{0}' To Export For This Change", exportTime.FormatAsString(false));
                }

                if (MessageBox.Show(message, "Do You Want To Export?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var exportTimerWindow = new ExportTimerWindow(gallifrey, e.TimerId);
                    if (exportTimerWindow.DisplayForm)
                    {
                        if (!gallifrey.Settings.AppSettings.ExportPromptAll)
                        {
                            exportTimerWindow.PreLoadExportTime(e.ExportTime);
                        }
                        exportTimerWindow.ShowDialog();
                    }
                }
            }
        }

        private void lblUnexportedTime_Click(object sender, EventArgs e)
        {
            var timer = gallifrey.JiraTimerCollection.GetOldestUnexportedTimer();
            if (timer != null)
            {
                SelectTimer(timer.UniqueId);
            }
            else
            {
                MessageBox.Show("No Un-Exported Timers To Show", "Nothing To Export");
            }
        }

        private void lblTwitter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://twitter.com/GallifreyApp");
        }

        private void lblEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:GallifreyApp@gmail.com?subject=Gallifrey App Contact");
        }

        private void lblGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/BlythMeister/Gallifrey");
        }

        private void lblDonate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=G3MWL8E6UG4RS");
        }

        #endregion

        #region "Button Handlers"

        private void btnAddTimer_Click(object sender, EventArgs e)
        {
            var addForm = new AddTimerWindow(gallifrey);

            var selectedTabDate = GetSelectedTabDate();
            if (selectedTabDate.HasValue)
            {
                addForm.PreLoadDate(selectedTabDate.Value, true);
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
            var selectedTimer = GetSelectedTimer();

            if (MessageBox.Show(string.Format("Are You Sure You Want To Remove Timer For '{0}'?", selectedTimer.JiraReference), "Are You Sure", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                gallifrey.JiraTimerCollection.RemoveTimer(selectedTimer.UniqueId);
                RefreshInternalTimerList();
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            ShowSettings(true);
        }

        private void ShowSettings(bool showInTaskbar)
        {
            var settingsWindow = new SettingsWindow(gallifrey) { ShowInTaskbar = showInTaskbar };
            settingsWindow.ShowDialog();
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab == null) return;
            var selectedTimer = GetSelectedTimer();
            var renameWindow = new EditTimerWindow(gallifrey, selectedTimer.UniqueId);
            renameWindow.ShowDialog();
            RefreshInternalTimerList();
        }

        private void btnTimeEdit_Click(object sender, EventArgs e)
        {
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab == null) return;
            var selectedTimer = GetSelectedTimer();
            var adjustTimerWindow = new AdjustTimerWindow(gallifrey, selectedTimer.UniqueId);
            adjustTimerWindow.ShowDialog();
            RefreshInternalTimerList();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab == null) return;
            var selectedTimer = GetSelectedTimer();
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
            if (updateReady)
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
                switch (WindowState)
                {
                    case FormWindowState.Minimized:
                        WindowState = FormWindowState.Normal;
                        break;
                    default:
                        this.BringToFront();
                        break;
                }
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
                case SessionSwitchReason.SessionLogoff:
                case SessionSwitchReason.RemoteDisconnect:
                case SessionSwitchReason.ConsoleDisconnect:

                    if (gallifrey.StartIdleTimer())
                    {
                        var openForms = Application.OpenForms.Cast<Form>().ToList();
                        foreach (var form in openForms.Where(form => form.Name != "MainWindow"))
                        {
                            form.Close();
                        }
                    }

                    break;

                case SessionSwitchReason.SessionUnlock:
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.RemoteConnect:
                case SessionSwitchReason.ConsoleConnect:

                    try
                    {
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
                    }
                    catch (NoIdleTimerRunningException) { }

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
                    list.ListChanged += OnListChanged;
                    internalTimerList.Add(validDate, list);
                }

                //Add missing timers
                var addedTimer = false;
                var listStartedEmpty = !internalTimerList[validDate].Any();
                foreach (var jiraTimer in gallifrey.JiraTimerCollection.GetTimersForADate(validDate))
                {
                    if (internalTimerList[validDate].All(x => x.UniqueId != jiraTimer.UniqueId))
                    {
                        if (!listStartedEmpty) addedTimer = true;
                        internalTimerList[validDate].Add(jiraTimer);
                    }
                }

                //re-order list
                if (addedTimer)
                {
                    var orderedList = internalTimerList[validDate].OrderBy(x => x.JiraReference, new JiraReferenceComparer()).ToList();
                    var list = new ThreadedBindingList<JiraTimer>(orderedList) { RaiseListChangedEvents = true };
                    list.ListChanged += OnListChanged;
                    internalTimerList[validDate] = list;

                    var timerList = (ListBox)tabTimerDays.TabPages[validDate.ToString("yyyyMMdd")].Controls[string.Format("lst_{0}", validDate.ToString("yyyyMMdd"))];
                    timerList.DataSource = internalTimerList[validDate];
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
            foreach (var timerlistValue in internalTimerList.OrderByDescending(x => x.Key))
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
                    timerList.DoubleClick += ListBoxDoubleClick;
                    timerList.MouseDown += ListBoxMouseDown;
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

        private ContextMenu BuildTimerListContextMenu(JiraTimer jiraTimerSelected)
        {
            if (jiraTimerSelected != null)
            {
                return BuildSelectedTimerContext(jiraTimerSelected);
            }
            else
            {
                return BuildNoTimerContext();
            }
        }

        private ContextMenu BuildNoTimerContext()
        {
            var menuItems = new List<MenuItem>
            {
                new MenuItem("Add New Timer", btnAddTimer_Click)
            };

            return new ContextMenu(menuItems.ToArray());
        }

        private ContextMenu BuildSelectedTimerContext(JiraTimer jiraTimer)
        {
            var menuItems = new List<MenuItem>();

            var dateMenuItems = new List<MenuItem>();

            var dateList = gallifrey.JiraTimerCollection.GetValidTimerDates().OrderByDescending(x => x.Date);

            if (dateList.All(x => x.Date != DateTime.Now.Date))
            {
                dateMenuItems.Add(new MenuItem(DateTime.Now.ToString("ddd, dd MMM"), ListContextDateClicked));
            }

            foreach (var timerlistValue in dateList)
            {
                if (timerlistValue.Date != jiraTimer.DateStarted.Date)
                {
                    dateMenuItems.Add(new MenuItem(timerlistValue.ToString("ddd, dd MMM"), ListContextDateClicked));
                }
            }

            menuItems.Add(new MenuItem("Add To Date", dateMenuItems.ToArray()));
            menuItems.Add(new MenuItem("Move Time To New Timer", ListContextSplitClicked));
            menuItems.Add(new MenuItem("Delete Timer", btnRemoveTimer_Click));
            menuItems.Add(new MenuItem("Adjust Timer Time", btnTimeEdit_Click));
            menuItems.Add(new MenuItem("Change Jira Ref/Date", btnRename_Click));
            menuItems.Add(new MenuItem("Export Timer", btnExport_Click));
            if (jiraTimer.IsRunning)
            {
                menuItems.Add(new MenuItem("Stop Timer", ListBoxDoubleClick));    
            }
            else
            {
                menuItems.Add(new MenuItem("Start Timer", ListBoxDoubleClick));    
            }
            
            return new ContextMenu(menuItems.ToArray());
        }

        private void ListContextSplitClicked(object sender, EventArgs e)
        {
            var selectedTimer = GetSelectedTimer();

            var addForm = new AddTimerWindow(gallifrey);
            addForm.PreLoadDate(selectedTimer.DateStarted.Date, false);

            if (addForm.DisplayForm)
            {
                addForm.ShowDialog();
                RefreshInternalTimerList();
                if (addForm.NewTimerId.HasValue)
                {
                    var addedTimer = gallifrey.JiraTimerCollection.GetTimer(addForm.NewTimerId.Value);
                    selectedTimer.ManualAdjustment(addedTimer.CurrentTime, false);
                    SelectTimer(addForm.NewTimerId.Value);
                }
            }
        }

        private void ListContextDateClicked(object sender, EventArgs e)
        {
            var menuItemSender = (MenuItem)sender;
            var selectedTimer = GetSelectedTimer();

            var addForm = new AddTimerWindow(gallifrey);
            addForm.PreLoadJira(selectedTimer.JiraReference);

            DateTime selecteDateTime;
            if (DateTime.TryParse(menuItemSender.Text, out selecteDateTime))
            {
                addForm.PreLoadDate(selecteDateTime, true);
                if (selecteDateTime.Date == DateTime.Now.Date)
                {
                    addForm.PreLoadStartNow();
                }
            }
            else
            {
                addForm.PreLoadStartNow();
            }

            if (addForm.DisplayForm)
            {
                addForm.ShowDialog();
                RefreshInternalTimerList();
                if (addForm.NewTimerId.HasValue) SelectTimer(addForm.NewTimerId.Value);
            }
        }

        private void OnListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
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
            var selectedTimer = GetSelectedTimer();

            if (selectedTimer == null)
            {
                lblCurrentTime.Text = "00:00:00";
                lblCurrentTime.ForeColor = Color.Red;
            }
            else
            {
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
            var selectedTimer = GetSelectedTimer();

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

        private DateTime? GetSelectedTabDate()
        {
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab != null)
            {
                if (gallifrey.JiraTimerCollection.GetTimersForADate(DateTime.Now.Date).Any())
                {
                    return DateTime.ParseExact(selectedTab.Name, "yyyyMMdd", CultureInfo.InvariantCulture);
                }
            }
            return null;
        }

        private JiraTimer GetSelectedTimer()
        {
            JiraTimer selectedTimer = null;
            var selectedTab = tabTimerDays.SelectedTab;
            if (selectedTab != null)
            {
                var selectedList = ((ListBox)selectedTab.Controls[string.Format("lst_{0}", selectedTab.Name)]);
                if (selectedList != null)
                {
                    try
                    {
                        selectedTimer = (JiraTimer)selectedList.SelectedItem;
                    }
                    catch (IndexOutOfRangeException) { /* There Seems to be some situations this throws, for no good reason */}
                }
            }

            if (selectedTimer != null)
            {
                return selectedTimer;
            }

            return null;
        }

        #endregion

        #region "Updates

        private async void lblUpdate_Click(object sender, EventArgs e)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                var restart = false;
                try
                {
                    if (ApplicationDeployment.CurrentDeployment != null && ApplicationDeployment.CurrentDeployment.UpdatedVersion != ApplicationDeployment.CurrentDeployment.CurrentVersion)
                    {
                        restart = true;
                    }
                    else
                    {
                        SetVersionNumber(true);
                        var updateResult = CheckForUpdates(true);
                        await updateResult;

                        if (updateResult.Result)
                        {
                            UpdateComplete();
                        }
                        else
                        {
                            SetVersionNumber(noUpdate: true);
                        }
                    }
                }
                catch (Exception)
                {
                    restart = true;
                }

                if (restart)
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
            }
            else
            {
                MessageBox.Show("The Version You Are Running Cannot Be Updated!!", "Invalid Version", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private async void CheckIfUpdateCallNeeded()
        {
            if (ApplicationDeployment.IsNetworkDeployed && lastUpdateCheck < DateTime.UtcNow.AddMinutes(-10))
            {
                SetVersionNumber();
                var updateResult = CheckForUpdates();
                await updateResult;

                if (updateResult.Result)
                {
                    UpdateComplete();
                }
                else
                {
                    SetVersionNumber(noUpdate: true);
                }
            }
        }

        private Task<bool> CheckForUpdates(bool manualCheck = false)
        {
            try
            {
                var updateInfo = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate(false);
                lastUpdateCheck = DateTime.UtcNow;

                if (updateInfo.UpdateAvailable && updateInfo.AvailableVersion > ApplicationDeployment.CurrentDeployment.CurrentVersion)
                {
                    return Task.Factory.StartNew(() => ApplicationDeployment.CurrentDeployment.Update());
                }

                if (manualCheck)
                {
                    return Task.Factory.StartNew(() =>
                    {
                        Task.Delay(1500);
                        return false;
                    });
                }
            }
            catch (Exception)
            {
            }

            return Task.Factory.StartNew(() => false);
        }

        private void UpdateComplete()
        {
            if ((gallifrey.Settings.AppSettings.AutoUpdate && Application.OpenForms.Count <= 1))
            {
                try
                {
                    Application.Restart();
                }
                catch (Exception)
                {
                    MessageBox.Show("An Error Occured When Trying To Restart Following An Update, Please Restart Manually", "Restart Failure", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                grpUpdates.Text = "Update Avaliable";
                lblUpdate.BackColor = Color.OrangeRed;
                lblUpdate.BorderStyle = BorderStyle.FixedSingle;
                lblUpdate.Image = Properties.Resources.Download_16x16;
                updateReady = true;

                try
                {
                    lblUpdate.Text = string.Format("     v{0}\nClick Here To Restart.", ApplicationDeployment.CurrentDeployment.UpdatedVersion);

                    notifyAlert.ShowBalloonTip(10000, "Update Avaliable", string.Format("An Update To v{0} Has Been Downloaded!", ApplicationDeployment.CurrentDeployment.UpdatedVersion), ToolTipIcon.Info);
                }
                catch (Exception)
                {
                    lblUpdate.Text = string.Format("\nClick Here To Restart.");
                }
            }
        }

        #endregion

        #region "Drag & Drop"

        private void tabTimerDays_DragOver(object sender, DragEventArgs e)
        {
            var validDrop = false;
            var url = GetUrl(e);
            if (!string.IsNullOrWhiteSpace(url))
            {
                var uriDrag = new Uri(url);
                var jiraUri = new Uri(gallifrey.Settings.JiraConnectionSettings.JiraUrl);
                if (uriDrag.Host == jiraUri.Host)
                {
                    validDrop = true;
                }
            }

            if (validDrop)
            {
                e.Effect = DragDropEffects.Copy;

                var pos = tabTimerDays.PointToClient(MousePosition);
                for (var ix = 0; ix < tabTimerDays.TabCount; ++ix)
                {
                    if (tabTimerDays.GetTabRect(ix).Contains(pos))
                    {
                        tabTimerDays.SelectedIndex = ix;
                        break;
                    }
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void tabTimerDays_DragDrop(object sender, DragEventArgs e)
        {
            var url = GetUrl(e);
            if (!string.IsNullOrWhiteSpace(url))
            {
                var uriDrag = new Uri(url).AbsolutePath;
                var jiraRef = uriDrag.Substring(uriDrag.LastIndexOf("/") + 1);

                var selectedTabDate = GetSelectedTabDate();
                //Check if already added & if so start timer.
                if (selectedTabDate.HasValue)
                {
                    var dayTimers = gallifrey.JiraTimerCollection.GetTimersForADate(selectedTabDate.Value);
                    if (dayTimers.Any(x => x.JiraReference == jiraRef))
                    {
                        gallifrey.JiraTimerCollection.StartTimer(dayTimers.First(x => x.JiraReference == jiraRef).UniqueId);
                        RefreshInternalTimerList();
                        if (gallifrey.JiraTimerCollection.GetRunningTimerId().HasValue)
                        {
                            SelectTimer(gallifrey.JiraTimerCollection.GetRunningTimerId().Value);
                        }
                        return;
                    }
                }

                //Validate jira is real
                try
                {
                    gallifrey.JiraConnection.GetJiraIssue(jiraRef);
                }
                catch (Exception)
                {
                    MessageBox.Show(string.Format("Unable To Locate That Jira.\n\nJira Ref Dropped: '{0}'", jiraRef), "Cannot Find Jira", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //show add form, we know it's a real jira & valid

                var addForm = new AddTimerWindow(gallifrey);
                addForm.PreLoadJira(jiraRef);

                if (selectedTabDate.HasValue)
                {
                    addForm.PreLoadDate(selectedTabDate.Value, true);
                    if (selectedTabDate.Value.Date == DateTime.Now.Date)
                    {
                        addForm.PreLoadStartNow();
                    }
                }
                else
                {
                    addForm.PreLoadStartNow();
                }

                if (addForm.DisplayForm)
                {
                    addForm.ShowDialog();
                    RefreshInternalTimerList();
                    if (addForm.NewTimerId.HasValue) SelectTimer(addForm.NewTimerId.Value);
                }
            }
        }

        private string GetUrl(DragEventArgs e)
        {
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                if (e.Data.GetDataPresent("Text"))
                {
                    return (string)e.Data.GetData("Text");
                }
            }

            return string.Empty;
        }

        #endregion

        


    }
}
