using Gallifrey.AppTracking;
using Gallifrey.Exceptions;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.ExtensionMethods;
using Gallifrey.JiraTimers;
using Gallifrey.UI.Modern.Extensions;
using Gallifrey.UI.Modern.Flyouts;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using Gallifrey.Versions;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Gallifrey.UI.Modern.MainViews
{
    public partial class MainWindow
    {
        private readonly ModelHelpers modelHelpers;
        private readonly ExceptionlessHelper exceptionlessHelper;
        private readonly ProgressDialogHelper progressDialogHelper;
        private readonly Timer flyoutOpenCheck;
        private readonly Timer updateHeartbeat;
        private readonly Timer idleDetectionHeartbeat;
        private MainViewModel ViewModel => (MainViewModel)DataContext;
        private bool machineLocked;
        private bool machineIdle;

        public MainWindow(InstanceType instance)
        {
            InitializeComponent();

            var gallifrey = new Backend(instance);
            modelHelpers = new ModelHelpers(gallifrey, FlyoutsControl);

            exceptionlessHelper = new ExceptionlessHelper(modelHelpers);
            exceptionlessHelper.RegisterExceptionless();

            progressDialogHelper = new ProgressDialogHelper(modelHelpers.DialogContext);

            var viewModel = new MainViewModel(modelHelpers);
            modelHelpers.RefreshModel();
            modelHelpers.SelectRunningTimer();
            DataContext = viewModel;

            gallifrey.NoActivityEvent += GallifreyOnNoActivityEvent;
            gallifrey.ExportPromptEvent += GallifreyOnExportPromptEvent;
            SystemEvents.SessionSwitch += SessionSwitchHandler;

            Height = gallifrey.Settings.UiSettings.Height;
            Width = gallifrey.Settings.UiSettings.Width;
            ThemeHelper.ChangeTheme(gallifrey.Settings.UiSettings.Theme, gallifrey.Settings.UiSettings.Accent);

            updateHeartbeat = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            updateHeartbeat.Elapsed += AutoUpdateCheck;

            idleDetectionHeartbeat = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);
            idleDetectionHeartbeat.Elapsed += IdleDetectionCheck;

            flyoutOpenCheck = new Timer(100);
            flyoutOpenCheck.Elapsed += FlyoutOpenCheck;
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await PerformUpdate(UpdateType.StartUp);

            var debuggerMissing = false;
            var multipleInstances = false;
            var missingConfig = false;
            var connectionError = false;
            var noInternet = false;
            try
            {
                var progressDialogHelper = new ProgressDialogHelper(modelHelpers.DialogContext);
                var result = await progressDialogHelper.Do(modelHelpers.Gallifrey.Initialise, "Initialising Gallifrey", true, true);
                if (result.Status == ProgressResult.JiraHelperStatus.Cancelled)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Gallifrey Not Initialised", "Gallifrey Initialisation Was Cancelled, The App Will Now Close");
                    modelHelpers.CloseApp();
                }
            }
            catch (NoInternetConnectionException)
            {
                noInternet = true;
            }
            catch (MissingJiraConfigException)
            {
                missingConfig = true;
            }
            catch (JiraConnectionException)
            {
                connectionError = true;
            }
            catch (MultipleGallifreyRunningException)
            {
                multipleInstances = true;
            }
            catch (DebuggerException)
            {
                debuggerMissing = true;
            }

            if (debuggerMissing)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Debugger Not Running", "It Looks Like Your Running Without Auto-Update\nPlease Use The Installed Shortcut To Start Gallifrey Or Download Again From GallifreyApp.co.uk");
                modelHelpers.CloseApp();
            }
            else if (multipleInstances)
            {
                modelHelpers.Gallifrey.TrackEvent(TrackingType.MultipleInstancesRunning);
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Multiple Instances", "You Can Only Have One Instance Of Gallifrey Running At A Time\nPlease Close The Other Instance");
                modelHelpers.CloseApp();
            }
            else if (noInternet)
            {
                modelHelpers.Gallifrey.TrackEvent(TrackingType.NoInternet);
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "No Internet Connection ", "Gallifrey Requires An Active Internet Connection To Work.\nPlease Try Again When You Have Internet");
                modelHelpers.CloseApp();
            }
            else if (missingConfig)
            {
                modelHelpers.Gallifrey.TrackEvent(TrackingType.SettingsMissing);
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Welcome To Gallifrey", "You Current Have No Jira Settings In Gallifrey\nWe Therefore Think Your A New User, So Welcome!\n\nTo Get Started, We Need Your Jira Details");

                await NewUserOnBoarding();

                modelHelpers.RefreshModel();
            }
            else if (connectionError)
            {
                modelHelpers.Gallifrey.TrackEvent(TrackingType.ConnectionError);
                var userUpdate = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Login Failure", "We Were Unable To Authenticate To Jira, Please Confirm Login Details\nWould You Like To Update Your Details?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

                if (userUpdate == MessageDialogResult.Negative)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Come Back Soon", "Without A Correctly Configured Jira Connection Gallifrey Will Close, Please Come Back Soon!");
                    modelHelpers.CloseApp();
                }

                await UserLoginFailure();
            }

            if (modelHelpers.Gallifrey.VersionControl.IsAutomatedDeploy && modelHelpers.Gallifrey.VersionControl.IsFirstRun)
            {
                var changeLog = modelHelpers.Gallifrey.GetChangeLog(XDocument.Parse(Properties.Resources.ChangeLog)).Where(x => x.NewVersion).ToList();

                if (changeLog.Any())
                {
                    await modelHelpers.OpenFlyout(new Flyouts.ChangeLog(changeLog));
                }
            }

            exceptionlessHelper.RegisterExceptionless();
            updateHeartbeat.Enabled = true;
            idleDetectionHeartbeat.Enabled = true;
            flyoutOpenCheck.Enabled = true;
        }

        private async Task NewUserOnBoarding()
        {
            await UserLoginFailure();

            var viewSettings = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "More Settings", "Gallifrey Has A Vast Range Of Settings, Would You Like To See Them Now?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

            if (viewSettings == MessageDialogResult.Affirmative)
            {
                await modelHelpers.OpenFlyout(new Flyouts.Settings(modelHelpers));
                if (!modelHelpers.Gallifrey.JiraConnection.IsConnected)
                {
                    var userUpdate = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Lost Jira Connection", "We Seem To Have Lost Jira Connection\nWould You Like To Update Your Details?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

                    if (userUpdate == MessageDialogResult.Negative)
                    {
                        await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Come Back Soon", "Without A Correctly Configured Jira Connection Gallifrey Will Close, Please Come Back Soon!");
                        modelHelpers.CloseApp();
                    }

                    await UserLoginFailure();
                }
            }
        }

        private async Task UserLoginFailure()
        {
            var loggedIn = false;

            while (!loggedIn)
            {
                var jiraSettings = modelHelpers.Gallifrey.Settings.JiraConnectionSettings;
                var url = await DialogCoordinator.Instance.ShowInputAsync(modelHelpers.DialogContext, "Jira URL", "Please Enter Your Jira Instance URL\nThis Is The URL You Go To When You Login Using A Browser\ne.g. https://MyCompany.atlassian.net", new MetroDialogSettings { DefaultText = jiraSettings.JiraUrl });
                var details = await DialogCoordinator.Instance.ShowLoginAsync(modelHelpers.DialogContext, "UserName & Password", "Please Enter Your UserName/Email Address & Password You Use To Login To Jira", new LoginDialogSettings { EnablePasswordPreview = true, InitialUsername = jiraSettings.JiraUsername, InitialPassword = jiraSettings.JiraPassword });
                var useTempo = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Tempo", "Do You Want To Use Tempo To Record Timesheets?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

                jiraSettings.JiraUrl = url;
                jiraSettings.JiraUsername = details.Username;
                jiraSettings.JiraPassword = details.Password;
                jiraSettings.UseTempo = useTempo == MessageDialogResult.Affirmative;

                try
                {
                    await progressDialogHelper.Do(() => modelHelpers.Gallifrey.SaveSettings(true, false), "Checking Jira Credentials", false, true);

                    if (jiraSettings.UseTempo && !modelHelpers.Gallifrey.JiraConnection.HasTempo)
                    {
                        await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Missing Tempo", "We Were Unable To Locate A Tempo Endpoint\nGallifrey Will Fall Back To Standard Jira Endpoints");
                        jiraSettings.UseTempo = false;
                        //Even though we have changed jira settings, we are changing because tempo is not in use, so don't reconnect
                        modelHelpers.Gallifrey.SaveSettings(false, false);
                    }

                    loggedIn = true;
                }
                catch (MissingJiraConfigException)
                {
                    var result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Missing Information", "Some Of The Jira Information We Requested Was Missing, Try Again?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });
                    if (result == MessageDialogResult.Negative)
                    {
                        await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Come Back Soon", "Without A Correctly Configured Jira Connection Gallifrey Will Close, Please Come Back Soon!");
                        modelHelpers.CloseApp();
                    }
                }
                catch (JiraConnectionException)
                {
                    var result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Login Failure", "We Were Unable To Authenticate To Jira, Try Again?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });
                    if (result == MessageDialogResult.Negative)
                    {
                        await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Come Back Soon", "Without A Correctly Configured Jira Connection Gallifrey Will Close, Please Come Back Soon!");
                        modelHelpers.CloseApp();
                    }
                }
            }
        }

        private async void GallifreyOnExportPromptEvent(object sender, ExportPromptDetail e)
        {
            var timer = modelHelpers.Gallifrey.JiraTimerCollection.GetTimer(e.TimerId);
            if (timer != null)
            {
                var exportTime = e.ExportTime;
                var message = $"Do You Want To Export '{timer.JiraReference}'?\n";
                if (modelHelpers.Gallifrey.Settings.ExportSettings.ExportPromptAll || (new TimeSpan(exportTime.Ticks - exportTime.Ticks % 600000000) == new TimeSpan(timer.TimeToExport.Ticks - (timer.TimeToExport.Ticks % 600000000))))
                {
                    exportTime = timer.TimeToExport;
                    message += $"You Have '{exportTime.FormatAsString(false)}' To Export";
                }
                else
                {
                    message += $"You Have '{exportTime.FormatAsString(false)}' To Export For This Change";
                }

                var messageResult = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Do You Want To Export?", message, MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    if (modelHelpers.Gallifrey.Settings.ExportSettings.ExportPromptAll)
                    {
                        await modelHelpers.OpenFlyout(new Export(modelHelpers, e.TimerId, null));
                    }
                    else
                    {
                        await modelHelpers.OpenFlyout(new Export(modelHelpers, e.TimerId, e.ExportTime));
                    }
                }
            }
        }


        private void GallifreyOnNoActivityEvent(object sender, int millisecondsSinceActivity)
        {
            Dispatcher.Invoke(() =>
            {
                ViewModel.SetNoActivityMilliseconds(millisecondsSinceActivity);

                if (millisecondsSinceActivity == 0 || ViewModel.TimerRunning)
                {
                    this.StopFlashingWindow();
                }
                else
                {
                    this.FlashWindow();
                }
            });
        }

        private void SessionSwitchHandler(object sender, SessionSwitchEventArgs e)
        {
            if (!modelHelpers.Gallifrey.Settings.AppSettings.TrackLockTime)
            {
                return;
            }

            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.SessionLogoff:
                case SessionSwitchReason.RemoteDisconnect:
                case SessionSwitchReason.ConsoleDisconnect:

                    machineLocked = true;
                    if (machineIdle)
                    {
                        machineIdle = false;
                    }
                    else
                    {
                        modelHelpers.Gallifrey.StartLockTimer();
                    }
                    break;

                case SessionSwitchReason.SessionUnlock:
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.RemoteConnect:
                case SessionSwitchReason.ConsoleConnect:

                    machineLocked = false;
                    machineIdle = false;
                    StopLockTimer(modelHelpers.Gallifrey.Settings.AppSettings.LockTimeThresholdMilliseconds);
                    break;
            }
        }

        private void IdleDetectionCheck(object sender, ElapsedEventArgs e)
        {
            if (!modelHelpers.Gallifrey.Settings.AppSettings.TrackIdleTime)
            {
                return;
            }

            if (machineLocked)
            {
                machineIdle = false;
                return;
            }

            var idleTimeInfo = IdleTimeDetector.GetIdleTimeInfo();
            if (idleTimeInfo.IdleTime.TotalMilliseconds >= modelHelpers.Gallifrey.Settings.AppSettings.IdleTimeThresholdMilliseconds && !machineIdle)
            {
                machineIdle = true;
                modelHelpers.Gallifrey.StartLockTimer(idleTimeInfo.IdleTime);
            }
            else if (idleTimeInfo.IdleTime.TotalMilliseconds < modelHelpers.Gallifrey.Settings.AppSettings.IdleTimeThresholdMilliseconds && machineIdle)
            {
                machineIdle = false;
                Dispatcher.Invoke(() => StopLockTimer(modelHelpers.Gallifrey.Settings.AppSettings.IdleTimeThresholdMilliseconds));
            }
        }

        private void FlyoutOpenCheck(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (modelHelpers.Gallifrey.Settings.UiSettings.TopMostOnFlyoutOpen)
                    {
                        if (modelHelpers.FlyoutOpen)
                        {
                            if (!Topmost)
                            {
                                this.FlashWindow();
                                WindowState = WindowState.Normal;
                                Activate();
                            }

                            Topmost = true;
                        }
                        else
                        {
                            this.StopFlashingWindow();
                            Topmost = false;
                        }
                    }
                    else
                    {
                        Topmost = false;
                    }
                });
            }
            catch (TaskCanceledException)
            {
                //Surpress Exception
            }
        }

        private async void StopLockTimer(int threshold)
        {
            var idleTimerId = modelHelpers.Gallifrey.StopLockTimer();
            if (!idleTimerId.HasValue) return;

            var idleTimer = modelHelpers.Gallifrey.IdleTimerCollection.GetTimer(idleTimerId.Value);
            if (idleTimer == null) return;

            if (modelHelpers.Gallifrey.Settings.AppSettings.TargetLogPerDay == TimeSpan.Zero && idleTimer.IdleTimeValue > TimeSpan.FromHours(10))
            {
                modelHelpers.Gallifrey.IdleTimerCollection.RemoveTimer(idleTimerId.Value);
            }
            else if (idleTimer.IdleTimeValue > modelHelpers.Gallifrey.Settings.AppSettings.TargetLogPerDay)
            {
                modelHelpers.Gallifrey.IdleTimerCollection.RemoveTimer(idleTimerId.Value);
            }
            else if (idleTimer.IdleTimeValue.TotalMilliseconds < threshold)
            {
                modelHelpers.Gallifrey.IdleTimerCollection.RemoveTimer(idleTimerId.Value);
                var runningId = modelHelpers.Gallifrey.JiraTimerCollection.GetRunningTimerId();
                if (runningId.HasValue)
                {
                    modelHelpers.Gallifrey.JiraTimerCollection.GetTimer(runningId.Value).AddIdleTimer(idleTimer);
                }
            }
            else
            {
                if (modelHelpers.FlyoutOpen)
                {
                    modelHelpers.HideAllFlyouts();
                }

                await modelHelpers.OpenFlyout(new LockedTimer(modelHelpers));

                foreach (var hiddenFlyout in modelHelpers.GetHiddenFlyouts())
                {
                    await modelHelpers.OpenFlyout(hiddenFlyout);
                }
            }
        }

        private async void ManualUpdateCheck(object sender, RoutedEventArgs e)
        {
            await PerformUpdate(UpdateType.Manual);
        }

        private void GetPremium(object sender, RoutedEventArgs e)
        {
            modelHelpers.ShowGetPremiumMessage();
        }

        private void AutoUpdateCheck(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(async () =>
            {
                if (modelHelpers.Gallifrey.VersionControl.IsAutomatedDeploy)
                {
                    await PerformUpdate(UpdateType.Auto);
                }
            });
        }

        private enum UpdateType
        {
            Manual,
            Auto,
            StartUp
        }

        private async Task PerformUpdate(UpdateType updateType)
        {
            try
            {
                //Do Update
                UpdateResult updateResult;
                if (updateType == UpdateType.Manual || updateType == UpdateType.StartUp)
                {
                    var controller = await DialogCoordinator.Instance.ShowProgressAsync(modelHelpers.DialogContext, "Please Wait", "Checking For Updates");
                    controller.SetIndeterminate();
                    updateResult = await modelHelpers.Gallifrey.VersionControl.CheckForUpdates(true);
                    await controller.CloseAsync();
                }
                else
                {
                    updateResult = await modelHelpers.Gallifrey.VersionControl.CheckForUpdates(false);
                }

                //Handle Update Result
                if (updateResult == UpdateResult.Updated && (updateType == UpdateType.Manual || updateType == UpdateType.StartUp))
                {
                    var messageResult = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Update Found", "Restart Now To Install Update?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                    if (messageResult == MessageDialogResult.Affirmative)
                    {
                        modelHelpers.CloseApp(true);
                        modelHelpers.Gallifrey.TrackEvent(TrackingType.ManualUpdateRestart);
                    }
                }
                else if (updateResult == UpdateResult.Updated && modelHelpers.Gallifrey.Settings.AppSettings.AutoUpdate && !machineLocked && !modelHelpers.FlyoutOpen)
                {
                    modelHelpers.CloseApp(true);
                    modelHelpers.Gallifrey.TrackEvent(TrackingType.AutoUpdateInstalled);
                }
                else if (updateResult == UpdateResult.NotDeployable && updateType == UpdateType.Manual)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Unable To Update", "You Cannot Auto Update This Version Of Gallifrey");
                }
                else if ((updateResult == UpdateResult.NoUpdate || updateResult == UpdateResult.TooSoon) && updateType == UpdateType.Manual)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "No Update Found", "There Are No Updates At This Time, Check Back Soon!");
                }
                else if (updateResult == UpdateResult.Error && updateType == UpdateType.Manual)
                {
                    throw new Exception();//Trigger error condition
                }
                else if (updateResult == UpdateResult.ReinstallNeeded && (updateType == UpdateType.Manual || updateType == UpdateType.StartUp))
                {
                    var messageResult = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Update Error", "To Update An Uninstall/Reinstall Is Required.\nThis Can Happen Automatically & No Timers Will Be Lost\nAll You Need To Do Is Press The \"Install\" Button When Prompted\n\nDo You Want To Update Now?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                    if (messageResult == MessageDialogResult.Affirmative)
                    {
                        modelHelpers.Gallifrey.VersionControl.ManualReinstall();
                        modelHelpers.CloseApp();
                    }
                }
            }
            catch (Exception)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Update Error", "There Was An Error Trying To Update Gallifrey, If This Problem Persists Please Contact Support");
            }
        }

        private void GetBeta(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://releases.gallifreyapp.co.uk/download/download-beta.html"));
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            flyoutOpenCheck.Stop();
            modelHelpers.Gallifrey.Settings.UiSettings.Height = (int)Height;
            modelHelpers.Gallifrey.Settings.UiSettings.Width = (int)Width;
            modelHelpers.Gallifrey.Close();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (modelHelpers.FlyoutOpen) return;

            var key = e.Key;
            RemoteButtonTrigger trigger;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                switch (key)
                {
                    case Key.A: trigger = RemoteButtonTrigger.Add; break;
                    case Key.D: trigger = RemoteButtonTrigger.Delete; break;
                    case Key.F: trigger = RemoteButtonTrigger.Search; break;
                    case Key.E: trigger = RemoteButtonTrigger.Edit; break;
                    case Key.U: trigger = RemoteButtonTrigger.Export; break;
                    case Key.L: trigger = RemoteButtonTrigger.LockTimer; break;
                    case Key.S: trigger = RemoteButtonTrigger.Settings; break;
                    case Key.C: trigger = RemoteButtonTrigger.Copy; break;
                    case Key.V: trigger = RemoteButtonTrigger.Paste; break;
                    default: return;
                }
            }
            else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                switch (key)
                {
                    case Key.F1: trigger = RemoteButtonTrigger.Info; break;
                    case Key.F2: trigger = RemoteButtonTrigger.Twitter; break;
                    case Key.F3: trigger = RemoteButtonTrigger.Email; break;
                    case Key.F4: trigger = RemoteButtonTrigger.Gitter; break;
                    case Key.F5: trigger = RemoteButtonTrigger.GitHub; break;
                    case Key.F6: trigger = RemoteButtonTrigger.PayPal; break;
                    default: return;
                }
            }
            else
            {
                switch (key)
                {
                    case Key.F1: trigger = RemoteButtonTrigger.Add; break;
                    case Key.F2: trigger = RemoteButtonTrigger.Delete; break;
                    case Key.F3: trigger = RemoteButtonTrigger.Search; break;
                    case Key.F4: trigger = RemoteButtonTrigger.Edit; break;
                    case Key.F5: trigger = RemoteButtonTrigger.Export; break;
                    case Key.F6: trigger = RemoteButtonTrigger.LockTimer; break;
                    case Key.F7: trigger = RemoteButtonTrigger.Settings; break;
                    default: return;
                }
            }

            modelHelpers.TriggerRemoteButtonPress(trigger);
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            this.StopFlashingWindow();
        }

        private void MainWindow_StateChange(object sender, EventArgs e)
        {
            if (modelHelpers.Gallifrey.Settings.UiSettings.TopMostOnFlyoutOpen && modelHelpers.FlyoutOpen && WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
        }
    }
}
