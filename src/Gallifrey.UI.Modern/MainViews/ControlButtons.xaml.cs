using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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

        private void ControlButtons_OnLoaded(object sender, RoutedEventArgs e)
        {
            ModelHelpers.KeyupEvent += ModelHelpersOnKeyupEvent;
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
            var selectedTimerIds = ViewModel.GetSelectedTimerIds().ToList();

            foreach (var selectedTimerId in selectedTimerIds)
            {
                var timer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(selectedTimerId);

                var result = await DialogCoordinator.Instance.ShowMessageAsync(ModelHelpers.DialogContext, "Are You Sure?", $"Are You Sure You Want To Delete {timer.JiraReference}\n\n{timer.JiraName}\nFor: {timer.DateStarted.Date.ToString("ddd, dd MMM")}", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                if (result == MessageDialogResult.Affirmative)
                {
                    ModelHelpers.Gallifrey.JiraTimerCollection.RemoveTimer(selectedTimerId);
                }
            }

            ModelHelpers.RefreshModel();
        }

        private void SearchButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.OpenFlyout(new Search(ModelHelpers, false));
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
                    ModelHelpers.Gallifrey.JiraTimerCollection.StartTimer(editTimerFlyout.EditedTimerId);
                }
            }
        }

        private async void ExportButton(object sender, RoutedEventArgs e)
        {
            var selectedTimerIds = ViewModel.GetSelectedTimerIds().ToList();

            foreach (var selectedTimerId in selectedTimerIds)
            {
                await ModelHelpers.OpenFlyout(new Export(ModelHelpers, selectedTimerId, null));
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
        }

        private void TwitterButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.Gallifrey.TrackEvent(TrackingType.ContactClick);
            Process.Start(new ProcessStartInfo("https://twitter.com/GallifreyApp"));
        }

        private void PayPalButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.Gallifrey.TrackEvent(TrackingType.PayPalClick);
            Process.Start(new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=G3MWL8E6UG4RS"));
        }

        private void GitHubButton(object sender, RoutedEventArgs e)
        {
            ModelHelpers.Gallifrey.TrackEvent(TrackingType.GitHubClick);
            Process.Start(new ProcessStartInfo("https://github.com/BlythMeister/Gallifrey"));
        }

        private void ModelHelpersOnKeyupEvent(object sender, Key key)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                switch (key)
                {
                    case Key.A: AddButton(this, null); break;
                    case Key.D: DeleteButton(this, null); break;
                    case Key.F: SearchButton(this, null); break;
                    case Key.E: EditButton(this, null); break;
                    case Key.S: ExportButton(this, null); break;
                    case Key.L: LockTimerButton(this, null); break;
                    case Key.P: SettingsButton(this, null); break;
                    case Key.I: InfoButton(this, null); break;
                    case Key.T: TwitterButton(this, null); break;
                    case Key.M: EmailButton(this, null); break;
                    case Key.G: GitHubButton(this, null); break;
                    case Key.C: PayPalButton(this, null); break;
                    default: return;
                }
            }
            else
            {
                switch (key)
                {
                    case Key.F1: AddButton(this, null); break;
                    case Key.F2: DeleteButton(this, null); break;
                    case Key.F3: SearchButton(this, null); break;
                    case Key.F4: EditButton(this, null); break;
                    case Key.F5: ExportButton(this, null); break;
                    case Key.F6: LockTimerButton(this, null); break;
                    case Key.F7: SettingsButton(this, null); break;
                    case Key.F8: InfoButton(this, null); break;
                    case Key.F9: TwitterButton(this, null); break;
                    case Key.F10: EmailButton(this, null); break;
                    case Key.F11: GitHubButton(this, null); break;
                    case Key.F12: PayPalButton(this, null); break;
                    default: return;
                }
            }
        }
    }
}