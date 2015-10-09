using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Xml.Linq;
using Exceptionless;
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
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace Gallifrey.UI.Modern.MainViews
{
    public partial class MainWindow : MetroWindow
    {
        private MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }

        public MainWindow(InstanceType instance, AppType appType)
        {
            InitializeComponent();

            ExceptionlessClient.Default.Configuration.ApiKey = "e7ac6366507547639ce69fea261d6545";
            ExceptionlessClient.Default.Configuration.DefaultTags.Add("Unknown_Version_Pre_Startup");
            ExceptionlessClient.Default.Configuration.Enabled = true;
            ExceptionlessClient.Default.SubmittingEvent += ExceptionlessSubmittingEvent;
            ExceptionlessClient.Default.Register();

            var gallifrey = new Backend(instance, appType);

            ExceptionlessClient.Default.Configuration.DefaultTags.Clear();
            ExceptionlessClient.Default.Configuration.DefaultTags.Add(gallifrey.VersionControl.VersionName.Replace("\n", " - "));

            var viewModel = new MainViewModel(gallifrey, FlyoutsControl);
            viewModel.RefreshModel();
            viewModel.SelectRunningTimer();
            DataContext = viewModel;

            gallifrey.NoActivityEvent += GallifreyOnNoActivityEvent;
            gallifrey.ExportPromptEvent += GallifreyOnExportPromptEvent;
            gallifrey.DailyTrackingEvent += GallifreyOnDailyTrackingEvent;
            SystemEvents.SessionSwitch += SessionSwitchHandler;

            Height = gallifrey.Settings.UiSettings.Height;
            Width = gallifrey.Settings.UiSettings.Width;
            Title = gallifrey.VersionControl.AppName;
            ThemeHelper.ChangeTheme(gallifrey.Settings.UiSettings.Theme);

            if (gallifrey.VersionControl.IsAutomatedDeploy)
            {
                PerformUpdate(false, true);
                var updateHeartbeat = new Timer(60000);
                updateHeartbeat.Elapsed += delegate { PerformUpdate(false, false); };
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
                    ViewModel.CloseAllFlyouts();
                    await ViewModel.OpenFlyout(new Error(ViewModel, e.Event));
                    CloseApp(true);
                });
            }
            else
            {
                return;
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var missingConfig = false;
            var showSettings = false;
            try
            {
                ViewModel.Gallifrey.Initialise();
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
                missingConfig = true;
            }

            if (missingConfig)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(ViewModel, "Multiple Instances", "You Can Only Have One Instance Of Gallifrey Running At A Time\nPlease Close The Other Instance");
                CloseApp();
            }
            else if (showSettings)
            {
                await ViewModel.OpenFlyout(new Flyouts.Settings(ViewModel));
            }

            if (ViewModel.Gallifrey.VersionControl.IsAutomatedDeploy && ViewModel.Gallifrey.VersionControl.IsFirstRun)
            {
                var changeLog = ViewModel.Gallifrey.GetChangeLog(XDocument.Parse(Properties.Resources.ChangeLog));

                if (changeLog.Any())
                {
                    ViewModel.OpenFlyout(new Flyouts.ChangeLog(ViewModel, changeLog));
                }
            }
        }

        private async void GallifreyOnExportPromptEvent(object sender, ExportPromptDetail e)
        {
            var timer = ViewModel.Gallifrey.JiraTimerCollection.GetTimer(e.TimerId);
            if (timer != null)
            {
                var exportTime = e.ExportTime;
                var message = string.Format("Do You Want To Export '{0}'?\n", timer.JiraReference);
                if (ViewModel.Gallifrey.Settings.ExportSettings.ExportPromptAll || (new TimeSpan(exportTime.Ticks - (exportTime.Ticks % 600000000)) == new TimeSpan(timer.TimeToExport.Ticks - (timer.TimeToExport.Ticks % 600000000))))
                {
                    exportTime = timer.TimeToExport;
                    message += string.Format("You Have '{0}' To Export", exportTime.FormatAsString(false));
                }
                else
                {
                    message += string.Format("You Have '{0}' To Export For This Change", exportTime.FormatAsString(false));
                }

                var messageResult = await DialogCoordinator.Instance.ShowMessageAsync(ViewModel, "Do You Want To Export?", message, MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    if (ViewModel.Gallifrey.Settings.ExportSettings.ExportPromptAll)
                    {
                        ViewModel.OpenFlyout(new Export(ViewModel, e.TimerId, e.ExportTime));
                    }
                    else
                    {
                        ViewModel.OpenFlyout(new Export(ViewModel, e.TimerId, null));
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
                    ExceptionlessClient.Default.SubmitFeatureUsage(ViewModel.Gallifrey.VersionControl.VersionName);
                });
            }
            catch (Exception)
            {
                //supress errors if tracking fails
            }
        }

        private void GallifreyOnNoActivityEvent(object sender, int millisecondsSinceActivity)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ViewModel.SetNoActivityMilliseconds(millisecondsSinceActivity);

                if (millisecondsSinceActivity == 0)
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
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.SessionLogoff:
                case SessionSwitchReason.RemoteDisconnect:
                case SessionSwitchReason.ConsoleDisconnect:

                    ViewModel.Gallifrey.StartIdleTimer();
                    break;

                case SessionSwitchReason.SessionUnlock:
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.RemoteConnect:
                case SessionSwitchReason.ConsoleConnect:

                    try
                    {
                        var idleTimerId = ViewModel.Gallifrey.StopIdleTimer();
                        var idleTimer = ViewModel.Gallifrey.IdleTimerCollection.GetTimer(idleTimerId);
                        if (idleTimer.IdleTimeValue.TotalSeconds < 60 || idleTimer.IdleTimeValue.TotalHours > 10)
                        {
                            ViewModel.Gallifrey.IdleTimerCollection.RemoveTimer(idleTimerId);
                        }
                        else
                        {
                            ViewModel.OpenFlyout(new LockedTimer(ViewModel));
                        }
                    }
                    catch (NoIdleTimerRunningException) { }

                    break;
            }
        }

        private void ManualUpdateCheck(object sender, RoutedEventArgs e)
        {
            PerformUpdate(true, true);
        }

        private async void PerformUpdate(bool manualUpdateCheck, bool promptReinstall)
        {
            var manualReinstall = false;

            try
            {
                UpdateResult updateResult;
                if (manualUpdateCheck)
                {
                    var controller = await DialogCoordinator.Instance.ShowProgressAsync(ViewModel, "Please Wait", "Checking For Updates");

                    updateResult = await ViewModel.Gallifrey.VersionControl.CheckForUpdates(true);
                    await controller.CloseAsync();
                }
                else
                {
                    updateResult = await ViewModel.Gallifrey.VersionControl.CheckForUpdates();
                }

                if (updateResult == UpdateResult.Updated)
                {
                    if (manualUpdateCheck)
                    {
                        var messageResult = await DialogCoordinator.Instance.ShowMessageAsync(ViewModel, "Update Found", "Restart Now To Install Update?", MessageDialogStyle.AffirmativeAndNegative,
                                                                    new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

                        if (messageResult == MessageDialogResult.Affirmative)
                        {
                            CloseApp(true);
                        }
                    }
                    else
                    {
                        if (ViewModel.Gallifrey.Settings.AppSettings.AutoUpdate)
                        {
                            CloseApp(true);
                        }
                    }
                }
                else if (manualUpdateCheck && updateResult == UpdateResult.NoUpdate)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(ViewModel, "No Update Found", "There Are No Updates At This Time, Check Back Soon!");
                }
                else if (manualUpdateCheck && updateResult == UpdateResult.NotDeployable)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(ViewModel, "Unable To Update", "You Cannot Auto Update This Version Of Gallifrey");
                }
            }
            catch (ManualReinstallRequiredException)
            {
                manualReinstall = true;
            }

            if (manualReinstall && promptReinstall)
            {
                var messageResult = await DialogCoordinator.Instance.ShowMessageAsync(ViewModel, "Update Error", "To Update An Uninstall/Reinstall Is Required.\nThis Can Happen Automatically\nNo Timers Will Be Lost\nDo You Want To Update Now?", MessageDialogStyle.AffirmativeAndNegative,
                                                                    new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    ViewModel.Gallifrey.VersionControl.ManualReinstall();
                }
            }
        }

        private void CloseApp(bool restart = false)
        {
            if (restart && ViewModel.Gallifrey.VersionControl.IsAutomatedDeploy)
            {
                Process.Start(ViewModel.Gallifrey.VersionControl.ActivationUrl);
            }

            Application.Current.Shutdown();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            ViewModel.Gallifrey.Settings.UiSettings.Height = (int)Height;
            ViewModel.Gallifrey.Settings.UiSettings.Width = (int)Width;
            ViewModel.Gallifrey.Close();
        }
    }
}
