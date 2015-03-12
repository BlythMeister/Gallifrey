using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly bool multipleInstances;

        public MainWindow(Backend gallifrey)
        {
            InitializeComponent();

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ExceptionlessClient.Current.UnhandledExceptionReporting += OnUnhandledExceptionReporting;
                ExceptionlessClient.Current.Register();
            }

            var viewModel = new MainViewModel(gallifrey, this);

            try
            {
                gallifrey.Initialise();
            }
            catch (MissingJiraConfigException)
            {
                var flyout = OpenFlyout(new Flyouts.Settings(viewModel));
                flyout.Wait();
            }
            catch (JiraConnectionException)
            {
                var flyout = OpenFlyout(new Flyouts.Settings(viewModel));
                flyout.Wait();
            }
            catch (MultipleGallifreyRunningException)
            {
                multipleInstances = true;
            }

            viewModel.RefreshModel();
            viewModel.SelectRunningTimer();

            DataContext = viewModel;

            gallifrey.NoActivityEvent += GallifreyOnNoActivityEvent;
            gallifrey.ExportPromptEvent += GallifreyOnExportPromptEvent;
            SystemEvents.SessionSwitch += SessionSwitchHandler;

            Height = gallifrey.Settings.UiSettings.Height;
            Width = gallifrey.Settings.UiSettings.Width;
            Title = gallifrey.VersionControl.AppName;

            if (gallifrey.VersionControl.IsAutomatedDeploy && gallifrey.VersionControl.IsFirstRun)
            {
                var changeLog = gallifrey.GetChangeLog(XDocument.Parse(Properties.Resources.ChangeLog));

                if (changeLog.Any())
                {
                    ViewModel.MainWindow.OpenFlyout(new Flyouts.ChangeLog(ViewModel, changeLog));
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
                if (ViewModel.Gallifrey.Settings.AppSettings.ExportPromptAll || (new TimeSpan(exportTime.Ticks - (exportTime.Ticks % 600000000)) == new TimeSpan(timer.TimeToExport.Ticks - (timer.TimeToExport.Ticks % 600000000))))
                {
                    exportTime = timer.TimeToExport;
                    message += string.Format("You Have '{0}' To Export", exportTime.FormatAsString(false));
                }
                else
                {
                    message += string.Format("You Have '{0}' To Export For This Change", exportTime.FormatAsString(false));
                }

                var messageResult = await this.ShowMessageAsync("Do You Want To Export?", message, MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    if (ViewModel.Gallifrey.Settings.AppSettings.ExportPromptAll)
                    {
                        ViewModel.MainWindow.OpenFlyout(new Export(ViewModel, e.TimerId, e.ExportTime));
                    }
                    else
                    {
                        ViewModel.MainWindow.OpenFlyout(new Export(ViewModel, e.TimerId, null));
                    }
                }
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

        private void OnUnhandledExceptionReporting(object sender, UnhandledExceptionReportingEventArgs e)
        {
            e.Error.Tags.Add(ViewModel.Gallifrey.VersionControl.VersionName.Replace("\n", " - "));
            foreach (var module in e.Error.Modules.Where(module => module.Name.ToLower().Contains("gallifrey.ui")))
            {
                module.Version = ViewModel.Gallifrey.VersionControl.VersionName.Replace("\n", " - ");
            }
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
                            OpenFlyout(new LockedTimer(ViewModel));
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

        public Task<Flyout> OpenFlyout(Flyout flyout, bool holdExecution = false)
        {
            var a = new TaskCompletionSource<Flyout>();
            RoutedEventHandler closingFinishedHandler = null;
            closingFinishedHandler = (o, args) =>
            {
                flyout.ClosingFinished -= closingFinishedHandler;
                FlyoutsControl.Items.Remove(flyout);
                a.TrySetResult(flyout);
            };
            flyout.ClosingFinished += closingFinishedHandler;

            FlyoutsControl.Items.Add(flyout);

            flyout.IsOpen = true;

            return a.Task;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (multipleInstances)
            {
                await this.ShowMessageAsync("Multiple Instances", "You Can Only Have One Instance Of Gallifrey Running At A Time\nPlease Close The Other Instance");
                Application.Current.Shutdown();
            }

            PerformUpdate(false, true);
        }

        private async void PerformUpdate(bool manualUpdateCheck, bool promptReinstall)
        {
            var manualReinstall = false;

            try
            {
                UpdateResult updateResult;
                if (manualUpdateCheck)
                {
                    var controller = await this.ShowProgressAsync("Please Wait", "Checking For Updates");

                    updateResult = await ViewModel.Gallifrey.VersionControl.CheckForUpdates();
                    await controller.CloseAsync();
                }
                else
                {
                    updateResult = await ViewModel.Gallifrey.VersionControl.CheckForUpdates();
                }

                if (updateResult == UpdateResult.Updated)
                {
                    if (!manualUpdateCheck && ViewModel.Gallifrey.Settings.AppSettings.AutoUpdate)
                    {
                        Process.Start(Application.ResourceAssembly.Location);
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        var messageResult = await this.ShowMessageAsync("Update Found", "Restart Now To Install Update?", MessageDialogStyle.AffirmativeAndNegative,
                                                                    new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

                        if (messageResult == MessageDialogResult.Affirmative)
                        {
                            Process.Start(Application.ResourceAssembly.Location);
                            Application.Current.Shutdown();
                        }
                    }
                }
                else if (!manualUpdateCheck)
                {
                    //if it's not manual there is nothing more to do.
                    return;
                }
                else if (updateResult == UpdateResult.NoUpdate)
                {
                    await this.ShowMessageAsync("No Update Found", "There Are No Updates At This Time, Check Back Soon!");
                }
                else
                {
                    await this.ShowMessageAsync("Unable To Update", "You Cannot Auto Update This Version Of Gallifrey");
                }
            }
            catch (ManualReinstallRequiredException)
            {
                manualReinstall = true;
            }

            if (manualReinstall && promptReinstall)
            {
                var messageResult = await this.ShowMessageAsync("Update Error", "To Update An Uninstall/Reinstall Is Required.\nThis Can Happen Automatically\nNo Timers Will Be Lost\nDo You Want To Update Now?", MessageDialogStyle.AffirmativeAndNegative,
                                                                    new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    ViewModel.Gallifrey.VersionControl.ManualReinstall();
                }
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            ViewModel.Gallifrey.Settings.UiSettings.Height = (int)Height;
            ViewModel.Gallifrey.Settings.UiSettings.Width = (int)Width;
            ViewModel.Gallifrey.Close();
        }
    }
}
