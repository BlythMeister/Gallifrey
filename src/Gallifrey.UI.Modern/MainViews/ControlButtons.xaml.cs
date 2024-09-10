using Gallifrey.AppTracking;
using Gallifrey.ExtensionMethods;
using Gallifrey.IdleTimers;
using Gallifrey.UI.Modern.Flyouts;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace Gallifrey.UI.Modern.MainViews
{
    public partial class ControlButtons
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;
        private ModelHelpers ModelHelpers => ((MainViewModel)DataContext).ModelHelpers;

        public ControlButtons()
        {
            InitializeComponent();
        }

        private void ControlButtons_OnLoaded(object sender, RoutedEventArgs e)
        {
            ModelHelpers.RemoteButtonTrigger += ModelHelpersOnRemoteButtonTrigger;
        }

        private async void AddButton(object sender, RoutedEventArgs e)
        {
            var startDate = ViewModel.TimerDates.FirstOrDefault(x => x.DateIsSelected)?.TimerDate ?? DateTime.Today;

            var addTimer = new AddTimer(ModelHelpers, startDate: startDate);
            await ModelHelpers.OpenFlyout(addTimer);
            if (addTimer.AddedTimer)
            {
                ModelHelpers.SetSelectedTimer(addTimer.NewTimerId);
            }
        }

        private async void AddToFillDay()
        {
            var startDate = ViewModel.TimerDates.FirstOrDefault(x => x.DateIsSelected)?.TimerDate ?? DateTime.Today;
            var recordedToDate = ModelHelpers.Gallifrey.JiraTimerCollection.GetTotalTimeForDateNoSeconds(startDate);
            var target = ModelHelpers.Gallifrey.Settings.AppSettings.TargetLogPerDay;

            if (recordedToDate < target)
            {
                var dummyIdleTimer = new IdleTimer(DateTime.Now, DateTime.Now, target.Subtract(recordedToDate), Guid.NewGuid());
                var addTimer = new AddTimer(ModelHelpers, startDate: startDate, idleTimers: new List<IdleTimer> { dummyIdleTimer }, enableDateChange: false);
                await ModelHelpers.OpenFlyout(addTimer);
                if (addTimer.AddedTimer)
                {
                    ModelHelpers.SetSelectedTimer(addTimer.NewTimerId);
                }
            }
            else
            {
                await ModelHelpers.ShowMessageAsync("No Extra Time", "You Have Already Hit The Target For This Date!");
            }
        }

        private async void CopyButton()
        {
            var selectedTimers = ViewModel.GetSelectedTimerIds().ToList();

            if (selectedTimers.Count > 1)
            {
                await ModelHelpers.ShowMessageAsync("Too Many Timers Selected", "Please Select Only One Timer When Copying Reference");
            }
            else if (selectedTimers.Count == 1)
            {
                var selectedTimer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(selectedTimers.First());
                if (selectedTimer.LocalTimer)
                {
                    await ModelHelpers.ShowMessageAsync("Not Available", "A Local Timer Does Not Have A Copyable Reference");
                }
                else
                {
                    await ClipboardHelper.SetClipboard(selectedTimer.JiraReference);
                }
            }
        }

        private async void PasteButton()
        {
            var pastedData = await ClipboardHelper.GetClipboard();
            string jiraRef;

            if (Uri.TryCreate(pastedData, UriKind.Absolute, out var pastedUri) && Uri.TryCreate(ModelHelpers.Gallifrey.Settings.JiraConnectionSettings.JiraUrl, UriKind.Absolute, out var jiraUri) && pastedUri.Host == jiraUri.Host)
            {
                var uriDrag = pastedUri.AbsolutePath;
                jiraRef = uriDrag.Substring(uriDrag.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase) + 1);
            }
            else
            {
                jiraRef = pastedData;
            }

            if (ModelHelpers.Gallifrey.JiraConnection.DoesJiraExist(jiraRef))
            {
                var startDate = ViewModel.TimerDates.FirstOrDefault(x => x.DateIsSelected)?.TimerDate ?? DateTime.Today;
                var addTimer = new AddTimer(ModelHelpers, startDate: startDate, jiraRef: jiraRef);
                await ModelHelpers.OpenFlyout(addTimer);
                if (addTimer.AddedTimer)
                {
                    ModelHelpers.SetSelectedTimer(addTimer.NewTimerId);
                }
            }
            else
            {
                await ModelHelpers.ShowMessageAsync("Invalid Jira", $"Unable To Locate That Jira.\n\nJira Ref Pasted: '{jiraRef}'");
            }
        }

        private async void ViewButton()
        {
            var selectedTimers = ViewModel.GetSelectedTimerIds().ToList();

            if (selectedTimers.Count > 1)
            {
                await ModelHelpers.ShowMessageAsync("Too Many Timers Selected", "Please Select Only One Timer When Showing In Jira");
            }
            else if (selectedTimers.Count == 1)
            {
                var selectedTimer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(selectedTimers.First());
                if (selectedTimer.LocalTimer)
                {
                    await ModelHelpers.ShowMessageAsync("Not Available", "A Local Timer Does Not Exist In Jira");
                }
                else
                {
                    var jiraRef = selectedTimer.JiraReference;
                    Uri.TryCreate(ModelHelpers.Gallifrey.Settings.JiraConnectionSettings.JiraUrl, UriKind.Absolute, out var jiraUri);
                    Process.Start(new ProcessStartInfo($"{jiraUri.AbsoluteUri}browse/{jiraRef}"));
                }
            }
        }

        private async void DeleteButton(object sender, RoutedEventArgs e)
        {
            var selectedTimerIds = ViewModel.GetSelectedTimerIds().ToList();

            foreach (var selectedTimerId in selectedTimerIds)
            {
                var timer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(selectedTimerId);

                if (timer != null)
                {
                    if (ModelHelpers.Gallifrey.Settings.AppSettings.DefaultTimers != null && ModelHelpers.Gallifrey.Settings.AppSettings.DefaultTimers.Any(x => x == timer.JiraReference) && timer.DateStarted.Date <= DateTime.Now.Date)
                    {
                        await ModelHelpers.ShowMessageAsync("Default Timer", $"The Timer {timer.JiraReference} Is A Default Time And Cannot Be Deleted.");
                    }
                    else
                    {
                        var result = await ModelHelpers.ShowMessageAsync("Are You Sure?", $"Are You Sure You Want To Delete {timer.JiraReference}\n\n{timer.JiraName}\nFor: {timer.DateStarted.Date:ddd, dd MMM}", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                        if (result == MessageDialogResult.Affirmative)
                        {
                            ModelHelpers.Gallifrey.JiraTimerCollection.RemoveTimer(selectedTimerId);
                        }
                    }
                }
            }

            ModelHelpers.RefreshModel();
        }

        private void SearchButton(object sender, RoutedEventArgs e)
        {
            var startDate = ViewModel.TimerDates.FirstOrDefault(x => x.DateIsSelected)?.TimerDate ?? DateTime.Today;
            ModelHelpers.OpenFlyout(new Search(ModelHelpers, selectedDateTab: startDate));
        }

        private async void EditButton(object sender, RoutedEventArgs e)
        {
            var selectedTimerIds = ViewModel.GetSelectedTimerIds().ToList();

            foreach (var selectedTimerId in selectedTimerIds)
            {
                var stoppedTimer = false;
                var runningTimerId = ModelHelpers.Gallifrey.JiraTimerCollection.GetRunningTimerId();

                if (runningTimerId.HasValue && runningTimerId.Value == selectedTimerId)
                {
                    ModelHelpers.Gallifrey.JiraTimerCollection.StopTimer(selectedTimerId, true);
                    stoppedTimer = true;
                }

                var editTimerFlyout = new EditTimer(ModelHelpers, selectedTimerId);
                await ModelHelpers.OpenFlyout(editTimerFlyout);

                if (stoppedTimer)
                {
                    var timer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(editTimerFlyout.EditedTimerId);
                    if (timer.DateStarted.Date == DateTime.Now.Date)
                    {
                        ModelHelpers.Gallifrey.JiraTimerCollection.StartTimer(editTimerFlyout.EditedTimerId);
                    }
                }
            }

            ModelHelpers.RefreshModel();
        }

        private async void ExportButton(object sender, RoutedEventArgs e)
        {
            var selectedTimerIds = ViewModel.GetSelectedTimerIds().ToList();
            if (selectedTimerIds.Any())
            {
                if (selectedTimerIds.Count > 1)
                {
                    foreach (var selectedTimer in selectedTimerIds.Select(x => ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(x)).ToList())
                    {
                        if (selectedTimer.IsRunning)
                        {
                            selectedTimerIds.Remove(selectedTimer.UniqueId);
                        }
                        else if (selectedTimer.LocalTimer)
                        {
                            selectedTimerIds.Remove(selectedTimer.UniqueId);
                        }
                    }
                }

                if (selectedTimerIds.Count == 1)
                {
                    await ModelHelpers.OpenFlyout(new Export(ModelHelpers, selectedTimerIds.First(), null));
                }
                else
                {
                    var timers = selectedTimerIds.Select(x => ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(x)).ToList();
                    await ModelHelpers.OpenFlyout(new BulkExport(ModelHelpers, timers));
                }
            }
        }

        private void LockTimerButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.OpenFlyout(new LockedTimer(ModelHelpers));
        }

        private async void SettingsButton(object sender, RoutedEventArgs e)
        {
            await ModelHelpers.OpenFlyout(new Flyouts.Settings(ModelHelpers));
            if (!ModelHelpers.Gallifrey.JiraConnection.IsConnected)
            {
                await ModelHelpers.ShowMessageAsync("Connection Required", "You Must Have A Working Jira Connection To Use Gallifrey");
                ModelHelpers.CloseApp();
            }
            ModelHelpers.RefreshModel();
        }

        private void SaveButton(object sender, RoutedEventArgs e)
        {
            var saveFile = new SaveFileDialog
            {
                DefaultExt = "csv",
                Filter = @"Comma Separated File|*.csv"
            };
            saveFile.ShowDialog();

            if (!string.IsNullOrWhiteSpace(saveFile.FileName))
            {
                var fileOutput = new StringBuilder();

                fileOutput.AppendLine("Timer ID,LocalTimer?,Jira Reference,Jira Description,Work Date,Time Spent");

                foreach (var timer in ModelHelpers.Gallifrey.JiraTimerCollection.GetAllTimersWithTime())
                {
                    fileOutput.AppendLine($"{timer.UniqueId},{timer.LocalTimer},{timer.JiraReference},\"{timer.JiraName}\",{timer.DateStarted.Date},{timer.ExactCurrentTime.FormatAsString()}");
                }

                File.WriteAllText(saveFile.FileName, fileOutput.ToString());
            }
        }

        private void InfoButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.OpenFlyout(new Information(ModelHelpers));
        }

        private void EmailButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.Gallifrey.TrackEvent(TrackingType.ContactClick);
            Process.Start(new ProcessStartInfo("mailto:gallifrey@blyth.me.uk?subject=Gallifrey App Contact"));
        }

        private void DonateButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.Gallifrey.TrackEvent(TrackingType.DonateClick);
            Process.Start(new ProcessStartInfo("https://www.blyth.me.uk/donations"));
        }

        private void GitHubButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.Gallifrey.TrackEvent(TrackingType.GitHubClick);
            Process.Start(new ProcessStartInfo("https://github.com/BlythMeister/Gallifrey"));
        }

        private void ModelHelpersOnRemoteButtonTrigger(object sender, RemoteButtonTrigger remoteButtonTrigger)
        {
            switch (remoteButtonTrigger)
            {
                case RemoteButtonTrigger.Add: AddButton(this, null); break;
                case RemoteButtonTrigger.AddToFill: AddToFillDay(); break;
                case RemoteButtonTrigger.Copy: CopyButton(); break;
                case RemoteButtonTrigger.Paste: PasteButton(); break;
                case RemoteButtonTrigger.View: ViewButton(); break;
                case RemoteButtonTrigger.Delete: DeleteButton(this, null); break;
                case RemoteButtonTrigger.Search: SearchButton(this, null); break;
                case RemoteButtonTrigger.Edit: EditButton(this, null); break;
                case RemoteButtonTrigger.Export: ExportButton(this, null); break;
                case RemoteButtonTrigger.LockTimer: LockTimerButton(this, null); break;
                case RemoteButtonTrigger.Settings: SettingsButton(this, null); break;
                case RemoteButtonTrigger.Save: SaveButton(this, null); break;
                case RemoteButtonTrigger.Info: InfoButton(this, null); break;
                case RemoteButtonTrigger.Email: EmailButton(this, null); break;
                case RemoteButtonTrigger.GitHub: GitHubButton(this, null); break;
                case RemoteButtonTrigger.Donate: DonateButton(this, null); break;

                default: return;
            }
        }
    }
}
