using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Gallifrey.UI.Modern.Flyouts;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.MainViews
{
    /// <summary>
    /// Interaction logic for ControlButtons.xaml
    /// </summary>
    public partial class ControlButtons : UserControl
    {
        private MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }

        public ControlButtons()
        {
            InitializeComponent();
        }

        private void AddButton(object sender, RoutedEventArgs e)
        {
            DateTime? startDate = null;

            if (ViewModel.HaveTimerToday())
            {
                startDate = ViewModel.GetSelectedDateTab();
            }

            ViewModel.OpenFlyout(new AddTimer(ViewModel, startDate: startDate));
        }

        private async void DeleteButton(object sender, RoutedEventArgs e)
        {
            var selectedTimer = ViewModel.GetSelectedTimerId();

            if (selectedTimer.HasValue)
            {
                var timer = ViewModel.Gallifrey.JiraTimerCollection.GetTimer(selectedTimer.Value);

                var result = DialogCoordinator.Instance.ShowMessageAsync(ViewModel.DialogContext, "Are You Sure?", string.Format("Are You Sure You Want To Delete {0}\n\n{1}\nFor: {2}", timer.JiraReference, timer.JiraName, timer.DateStarted.Date.ToString("ddd, dd MMM")), MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

                await result;

                if (result.Result == MessageDialogResult.Affirmative)
                {
                    ViewModel.Gallifrey.JiraTimerCollection.RemoveTimer(selectedTimer.Value);
                    ViewModel.RefreshModel();
                }
            }
        }

        private void SearchButton(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenFlyout(new Search(ViewModel, false));
        }

        private async void EditButton(object sender, RoutedEventArgs e)
        {
            var selectedTimerId = ViewModel.GetSelectedTimerId();
            if (selectedTimerId.HasValue)
            {
                var stoppedTimer = false;
                var runningTimerId = ViewModel.Gallifrey.JiraTimerCollection.GetRunningTimerId();
                if (runningTimerId.HasValue && runningTimerId.Value == selectedTimerId.Value)
                {
                    ViewModel.Gallifrey.JiraTimerCollection.StopTimer(selectedTimerId.Value, true);
                    stoppedTimer = true;
                }

                var editTimerFlyout = new EditTimer(ViewModel, selectedTimerId.Value);
                await ViewModel.OpenFlyout(editTimerFlyout);

                if (stoppedTimer)
                {
                    ViewModel.Gallifrey.JiraTimerCollection.StartTimer(editTimerFlyout.EditedTimerId);
                }
            }
        }

        private void ExportButton(object sender, RoutedEventArgs e)
        {
            var selectedTimerId = ViewModel.GetSelectedTimerId();
            if (selectedTimerId != null)
            {
                ViewModel.OpenFlyout(new Export(ViewModel, selectedTimerId.Value, null));
            }
        }

        private void LockTimerButton(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenFlyout(new LockedTimer(ViewModel));
        }

        private void InfoButton(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenFlyout(new Information(ViewModel));
        }

        private void SettingsButton(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenFlyout(new Flyouts.Settings(ViewModel));
        }

        private void EmailButton(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("mailto:contact@gallifreyapp.co.uk?subject=Gallifrey App Contact"));
            e.Handled = true;
        }

        private void TwitterButton(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://twitter.com/GallifreyApp"));
            e.Handled = true;
        }

        private void PayPalButton(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=G3MWL8E6UG4RS"));
            e.Handled = true;
        }

        private void GitHubButton(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/BlythMeister/Gallifrey"));
            e.Handled = true;
        }
    }
}
