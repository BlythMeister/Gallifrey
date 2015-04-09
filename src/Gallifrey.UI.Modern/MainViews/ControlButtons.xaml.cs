using System;
using System.Windows;
using System.Windows.Controls;
using Gallifrey.UI.Modern.Flyouts;
using Gallifrey.UI.Modern.Models;
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
            ViewModel.MainWindow.OpenFlyout(new AddTimer(ViewModel));
        }

        private async void DeleteButton(object sender, RoutedEventArgs e)
        {
            var selectedTimer = ViewModel.GetSelectedTimerId();

            if (selectedTimer.HasValue)
            {
                var timer = ViewModel.Gallifrey.JiraTimerCollection.GetTimer(selectedTimer.Value);

                var result = ViewModel.MainWindow.ShowMessageAsync("Are You Sure?", string.Format("Are You Sure You Want To Delete {0}\n\n{1}\nFor: {2}", timer.JiraReference, timer.JiraName, timer.DateStarted.Date.ToString("ddd, dd MMM")), MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

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
            ViewModel.MainWindow.OpenFlyout(new Search(ViewModel, false));
        }

        private void EditButton(object sender, RoutedEventArgs e)
        {
            ViewModel.MainWindow.OpenFlyout(new EditTimer(ViewModel));
        }

        private void ExportButton(object sender, RoutedEventArgs e)
        {
            var selectedTimerId = ViewModel.GetSelectedTimerId();
            if (selectedTimerId != null)
            {
                ViewModel.MainWindow.OpenFlyout(new Export(ViewModel, selectedTimerId.Value, null));
            }
        }

        private void LockTimerButton(object sender, RoutedEventArgs e)
        {
            ViewModel.MainWindow.OpenFlyout(new LockedTimer(ViewModel));
        }

        private void InfoButton(object sender, RoutedEventArgs e)
        {
            ViewModel.MainWindow.OpenFlyout(new Information(ViewModel));
        }

        private void SettingsButton(object sender, RoutedEventArgs e)
        {
            ViewModel.MainWindow.OpenFlyout(new Flyouts.Settings(ViewModel));
        }
    }
}
