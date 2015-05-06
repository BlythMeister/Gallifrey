using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Gallifrey.UI.Modern.Flyouts;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.MainViews
{
    /// <summary>
    /// Interaction logic for TimerSummary.xaml
    /// </summary>
    public partial class TimerSummary : UserControl
    {
        private MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }

        public TimerSummary()
        {
            InitializeComponent();
        }

        private async void UnExportedClick(object sender, MouseButtonEventArgs e)
        {
            var timers = ViewModel.Gallifrey.JiraTimerCollection.GetAllUnexportedTimersInStartOrder();

            if (timers.Any())
            {
                foreach (var jiraTimer in timers)
                {
                    await ViewModel.MainWindow.OpenFlyout(new Export(ViewModel, jiraTimer.UniqueId, null));

                    var updatedTimer = ViewModel.Gallifrey.JiraTimerCollection.GetTimer(jiraTimer.UniqueId);
                    if (!updatedTimer.FullyExported)
                    {
                        ViewModel.MainWindow.ShowMessageAsync("Stopping Bulk Export", "Will Stop Bulk Export As Timer Was Not Fully Exported\n\nWill Select The Cancelled Timer");
                        ViewModel.SetSelectedTimer(jiraTimer.UniqueId);
                    }
                }
            }
            else
            {
                ViewModel.MainWindow.ShowMessageAsync("Nothing To Export", "No Un-Exported Timers To Bulk Export");
            }

            ViewModel.RefreshModel();
        }
    }
}
