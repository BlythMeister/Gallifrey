using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Xml.Linq;
using Exceptionless;
using Gallifrey.AppTracking;
using Gallifrey.Exceptions;
using Gallifrey.Exceptions.IdleTimers;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Exceptions.Versions;
using Gallifrey.ExtensionMethods;
using Gallifrey.JiraTimers;
using Gallifrey.UI.Modern.Extensions;
using Gallifrey.UI.Modern.Flyouts;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using Gallifrey.Versions;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace Gallifrey.UI.Modern.MainViews
{
    public partial class MainWindow
    {
        private readonly ModelHelpers modelHelpers;
        private MainViewModel ViewModel => (MainViewModel)DataContext;
        private bool machineLocked;

        public MainWindow(InstanceType instance, AppType appType)
        {
            InitializeComponent();

            var gallifrey = new Backend(instance, appType);

            if (gallifrey.VersionControl.IsAutomatedDeploy)
            {
                ExceptionlessClient.Default.Configuration.ApiKey = "e7ac6366507547639ce69fea261d6545";
                ExceptionlessClient.Default.Configuration.DefaultTags.Add(gallifrey.VersionControl.VersionName.Replace("\n", " - "));
                ExceptionlessClient.Default.Configuration.Enabled = true;
                ExceptionlessClient.Default.SubmittingEvent += ExceptionlessSubmittingEvent;
                ExceptionlessClient.Default.Register();
            }

            modelHelpers = new ModelHelpers(gallifrey, FlyoutsControl);
            var viewModel = new MainViewModel(modelHelpers);
            modelHelpers.RefreshModel();
            modelHelpers.SelectRunningTimer();
            DataContext = viewModel;

            gallifrey.NoActivityEvent += GallifreyOnNoActivityEvent;
            gallifrey.ExportPromptEvent += GallifreyOnExportPromptEvent;
            gallifrey.DailyTrackingEvent += GallifreyOnDailyTrackingEvent;
            SystemEvents.SessionSwitch += SessionSwitchHandler;

            Height = gallifrey.Settings.UiSettings.Height;
            Width = gallifrey.Settings.UiSettings.Width;
            Title = gallifrey.VersionControl.AppName;
            ThemeHelper.ChangeTheme(gallifrey.Settings.UiSettings.Theme, gallifrey.Settings.UiSettings.Accent);

            if (gallifrey.VersionControl.IsAutomatedDeploy)
            {
                PerformUpdate(false, true);
                var updateHeartbeat = new Timer(60000);
                updateHeartbeat.Elapsed += AutoUpdateCheck;
                updateHeartbeat.Enabled = true;
            }
        }

        private async void ExceptionlessSubmittingEvent(object sender, EventSubmittingEventArgs e)
        {
            if (e.IsUnhandledError)
            {
                e.Cancel = true;

                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    modelHelpers.CloseAllFlyouts();
                    await modelHelpers.OpenFlyout(new Error(modelHelpers, e.Event));
                    modelHelpers.CloseApp(true);
                });
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var multipleInstances = false;
            var showSettings = false;
            try
            {
                modelHelpers.Gallifrey.Initialise();
            }
            catch (MissingJiraConfigException)
            {
                showSettings = true;
            }
            catch (JiraConnectionException)
            {
                showSettings = true;
            }
            catch (MultipleGallifreyRunningException)
            {
                multipleInstances = true;
            }

            if (multipleInstances)
            {
                modelHelpers.Gallifrey.TrackEvent(TrackingType.MultipleInstancesRunning);
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Multiple Instances", "You Can Only Have One Instance Of Gallifrey Running At A Time\nPlease Close The Other Instance");
                modelHelpers.CloseApp();
            }
            else if (showSettings)
            {
                modelHelpers.Gallifrey.TrackEvent(TrackingType.SettingsMissing);
                await modelHelpers.OpenFlyout(new Flyouts.Settings(modelHelpers));
                if (!modelHelpers.Gallifrey.JiraConnection.IsConnected)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Connection Required", "You Must Have A Working Jira Connection To Use Gallifrey");
                    modelHelpers.CloseApp();
                }
            }

            if (modelHelpers.Gallifrey.VersionControl.IsAutomatedDeploy && modelHelpers.Gallifrey.VersionControl.IsFirstRun)
            {
                var changeLog = modelHelpers.Gallifrey.GetChangeLog(XDocument.Parse(Properties.Resources.ChangeLog)).Where(x => x.NewVersion).ToList();

                if (changeLog.Any())
                {
                    await modelHelpers.OpenFlyout(new Flyouts.ChangeLog(changeLog));
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
                if (modelHelpers.Gallifrey.Settings.ExportSettings.ExportPromptAll || (new TimeSpan(exportTime.Ticks - (exportTime.Ticks % 600000000)) == new TimeSpan(timer.TimeToExport.Ticks - (timer.TimeToExport.Ticks % 600000000))))
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
                        await modelHelpers.OpenFlyout(new Export(modelHelpers, e.TimerId, e.ExportTime));
                    }
                    else
                    {
                        await modelHelpers.OpenFlyout(new Export(modelHelpers, e.TimerId, null));
                    }
                }
            }
        }

        private void GallifreyOnDailyTrackingEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ExceptionlessClient.Default.SubmitFeatureUsage(modelHelpers.Gallifrey.VersionControl.VersionName);
                });
            }
            catch (Exception)
            {
                //suppress errors if tracking fails
            }
        }

        private void GallifreyOnNoActivityEvent(object sender, int millisecondsSinceActivity)
        {
            Application.Current.Dispatcher.Invoke(() =>
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

        private async void SessionSwitchHandler(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.SessionLogoff:
                case SessionSwitchReason.RemoteDisconnect:
                case SessionSwitchReason.ConsoleDisconnect:

                    modelHelpers.Gallifrey.StartIdleTimer();
                    machineLocked = true;
                    break;

                case SessionSwitchReason.SessionUnlock:
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.RemoteConnect:
                case SessionSwitchReason.ConsoleConnect:

                    try
                    {
                        var idleTimerId = modelHelpers.Gallifrey.StopIdleTimer();
                        var idleTimer = modelHelpers.Gallifrey.IdleTimerCollection.GetTimer(idleTimerId);
                        if (idleTimer.IdleTimeValue.TotalSeconds < 60 || idleTimer.IdleTimeValue.TotalHours > 10)
                        {
                            modelHelpers.Gallifrey.IdleTimerCollection.RemoveTimer(idleTimerId);
                        }
                        else
                        {
                            this.FlashWindow();
                            Activate();
                            Topmost = true;
                            modelHelpers.CloseAllFlyouts();
                            await modelHelpers.OpenFlyout(new LockedTimer(modelHelpers));
                            this.StopFlashingWindow();
                            Topmost = false;
                        }
                    }
                    catch (NoIdleTimerRunningException) { }
                    machineLocked = false;
                    break;
            }
        }

        private void ManualUpdateCheck(object sender, RoutedEventArgs e)
        {
            PerformUpdate(true, true);
        }

        private void AutoUpdateCheck(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!machineLocked)
                {
                    PerformUpdate(false, false);
                }
            });
        }

        private async void PerformUpdate(bool manualUpdateCheck, bool promptReinstall)
        {
            var manualReinstall = false;

            try
            {
                UpdateResult updateResult;
                if (manualUpdateCheck)
                {
                    modelHelpers.Gallifrey.TrackEvent(TrackingType.ManualUpdateCheck);
                    var controller = await DialogCoordinator.Instance.ShowProgressAsync(modelHelpers.DialogContext, "Please Wait", "Checking For Updates");

                    updateResult = await modelHelpers.Gallifrey.VersionControl.CheckForUpdates(true);
                    await controller.CloseAsync();
                }
                else
                {
                    updateResult = await modelHelpers.Gallifrey.VersionControl.CheckForUpdates();
                }

                if (updateResult == UpdateResult.Updated)
                {
                    if (manualUpdateCheck)
                    {
                        var messageResult = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Update Found", "Restart Now To Install Update?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                        if (messageResult == MessageDialogResult.Affirmative)
                        {
                            modelHelpers.CloseApp(true);
                            modelHelpers.Gallifrey.TrackEvent(TrackingType.ManualUpdateRestart);
                        }
                    }
                    else
                    {
                        if (modelHelpers.Gallifrey.Settings.AppSettings.AutoUpdate)
                        {
                            modelHelpers.CloseApp(true);
                            modelHelpers.Gallifrey.TrackEvent(TrackingType.AutoUpdateInstalled);
                        }
                    }
                }
                else if (manualUpdateCheck && updateResult == UpdateResult.NoUpdate)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "No Update Found", "There Are No Updates At This Time, Check Back Soon!");
                }
                else if (manualUpdateCheck && updateResult == UpdateResult.NotDeployable)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Unable To Update", "You Cannot Auto Update This Version Of Gallifrey");
                }
            }
            catch (ManualReinstallRequiredException)
            {
                manualReinstall = true;
            }

            if (manualReinstall && promptReinstall)
            {
                var messageResult = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Update Error", "To Update An Uninstall/Reinstall Is Required.\nThis Can Happen Automatically\nNo Timers Will Be Lost\nDo You Want To Update Now?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    modelHelpers.Gallifrey.VersionControl.ManualReinstall();
                }
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            modelHelpers.Gallifrey.Settings.UiSettings.Height = (int)Height;
            modelHelpers.Gallifrey.Settings.UiSettings.Width = (int)Width;
            modelHelpers.Gallifrey.Close();
        }
    }
}
