using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Gallifrey.AppTracking;
using Gallifrey.UI.Modern.Flyouts;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

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

        private void AddButton(object sender, RoutedEventArgs e)
        {
            DateTime? startDate = null;

            if (ViewModel.TimerDates.Any(x => x.TimerDate.Date == DateTime.Now.Date))
            {
                var selectedDate = ViewModel.TimerDates.FirstOrDefault(x => x.IsSelected);
                startDate = selectedDate?.TimerDate;
            }

            ModelHelpers.OpenFlyout(new AddTimer(ModelHelpers, startDate: startDate));
        }

        private async void DeleteButton(object sender, RoutedEventArgs e)
        {
            var selectedTimer = ViewModel.GetSelectedTimerId();

            if (selectedTimer.HasValue)
            {
                var timer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(selectedTimer.Value);

                var result = await DialogCoordinator.Instance.ShowMessageAsync(ModelHelpers.DialogContext, "Are You Sure?", $"Are You Sure You Want To Delete {timer.JiraReference}\n\n{timer.JiraName}\nFor: {timer.DateStarted.Date.ToString("ddd, dd MMM")}", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                if (result == MessageDialogResult.Affirmative)
                {
                    ModelHelpers.Gallifrey.JiraTimerCollection.RemoveTimer(selectedTimer.Value);
                    ModelHelpers.RefreshModel();
                }
            }
        }

        private void SearchButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.OpenFlyout(new Search(ModelHelpers, false));
        }

        private async void EditButton(object sender, RoutedEventArgs e)
        {
            var selectedTimerId = ViewModel.GetSelectedTimerId();
            if (selectedTimerId.HasValue)
            {
                var stoppedTimer = false;
                var runningTimerId = ModelHelpers.Gallifrey.JiraTimerCollection.GetRunningTimerId();
                if (runningTimerId.HasValue && runningTimerId.Value == selectedTimerId.Value)
                {
                    ModelHelpers.Gallifrey.JiraTimerCollection.StopTimer(selectedTimerId.Value, true);
                    stoppedTimer = true;
                }

                var editTimerFlyout = new EditTimer(ModelHelpers, selectedTimerId.Value);
                await ModelHelpers.OpenFlyout(editTimerFlyout);

                if (stoppedTimer)
                {
                    ModelHelpers.Gallifrey.JiraTimerCollection.StartTimer(editTimerFlyout.EditedTimerId);
                }
            }
        }

        private void ExportButton(object sender, RoutedEventArgs e)
        {
            var selectedTimerId = ViewModel.GetSelectedTimerId();
            if (selectedTimerId != null)
            {
                ModelHelpers.OpenFlyout(new Export(ModelHelpers, selectedTimerId.Value, null));
            }
        }

        private void LockTimerButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.OpenFlyout(new LockedTimer(ModelHelpers));
        }

        private void InfoButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.OpenFlyout(new Information(ModelHelpers));
        }

        private async void SettingsButton(object sender, RoutedEventArgs e)
        {
            await ModelHelpers.OpenFlyout(new Flyouts.Settings(ModelHelpers));
            if (!ModelHelpers.Gallifrey.JiraConnection.IsConnected)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(ModelHelpers.DialogContext, "Connection Required", "You Must Have A Working Jira Connection To Use Gallifrey");
                ModelHelpers.CloseApp();
            }
        }

        private void EmailButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.Gallifrey.TrackEvent(TrackingType.ContactClick);
            Process.Start(new ProcessStartInfo("mailto:contact@gallifreyapp.co.uk?subject=Gallifrey App Contact"));
            e.Handled = true;
        }

        private void TwitterButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.Gallifrey.TrackEvent(TrackingType.ContactClick);
            Process.Start(new ProcessStartInfo("https://twitter.com/GallifreyApp"));
            e.Handled = true;
        }

        private void PayPalButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.Gallifrey.TrackEvent(TrackingType.PayPalClick);
            Process.Start(new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=G3MWL8E6UG4RS"));
            e.Handled = true;
        }

        private void GitHubButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.Gallifrey.TrackEvent(TrackingType.GitHubClick);
            Process.Start(new ProcessStartInfo("https://github.com/BlythMeister/Gallifrey"));
            e.Handled = true;
        }
    }
}