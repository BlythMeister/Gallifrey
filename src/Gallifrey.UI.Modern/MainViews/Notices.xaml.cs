using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Gallifrey.UI.Modern.Flyouts;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.MainViews
{
    public partial class Notices
    {
        private ModelHelpers ModelHelpers => ((MainViewModel)DataContext).ModelHelpers;
        private readonly Mutex unexportedMutex;

        public Notices()
        {
            InitializeComponent();
            unexportedMutex = new Mutex(false);
        }

        private async void UnExportedClick(object sender, MouseButtonEventArgs e)
        {
            unexportedMutex.WaitOne();
            var timers = ModelHelpers.Gallifrey.JiraTimerCollection.GetStoppedUnexportedTimers().ToList();

            if (timers.Any())
            {
                foreach (var jiraTimer in timers)
                {
                    await ModelHelpers.OpenFlyout(new Export(ModelHelpers, jiraTimer.UniqueId, null));

                    var updatedTimer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(jiraTimer.UniqueId);
                    if (!updatedTimer.FullyExported)
                    {
                        await DialogCoordinator.Instance.ShowMessageAsync(ModelHelpers.DialogContext, "Stopping Bulk Export", "Will Stop Bulk Export As Timer Was Not Fully Exported\n\nWill Select The Cancelled Timer");
                        ModelHelpers.SetSelectedTimer(jiraTimer.UniqueId);
                        break;
                    }
                }
            }
            else
            {
                await DialogCoordinator.Instance.ShowMessageAsync(ModelHelpers.DialogContext, "Nothing To Export", "No Un-Exported Timers To Bulk Export");
            }

            ModelHelpers.RefreshModel();
            unexportedMutex.ReleaseMutex();
        }

        private void InstallUpdate(object sender, MouseButtonEventArgs e)
        {
            if (ModelHelpers.Gallifrey.VersionControl.UpdateInstalled)
            {
                System.Windows.Forms.Application.Restart();
                Application.Current.Shutdown();
            }
        }

        private void GoToRunningTimer(object sender, MouseButtonEventArgs e)
        {
            ModelHelpers.SelectRunningTimer();
        }
    }
}
