using System.Linq;
using System.Threading;
using System.Windows.Input;
using Gallifrey.AppTracking;
using Gallifrey.UI.Flyouts;
using Gallifrey.UI.Helpers;
using Gallifrey.UI.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.MainViews
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
            ModelHelpers.Gallifrey.TrackEvent(TrackingType.ExportAll);
            var timers = ModelHelpers.Gallifrey.JiraTimerCollection.GetStoppedUnexportedTimers().ToList();
            timers.RemoveAll(x => x.TempTimer || x.IsRunning);

            if (timers.Any())
            {
                if (timers.Count == 1)
                {
                    await ModelHelpers.OpenFlyout(new Export(ModelHelpers, timers.First().UniqueId, null));
                }
                else
                {
                    await ModelHelpers.OpenFlyout(new BulkExport(ModelHelpers, timers));
                }
            }
            else
            {
                await DialogCoordinator.Instance.ShowMessageAsync(ModelHelpers.DialogContext, "Nothing To Export", "No Un-Exported Timers Avaliable To Export");
            }

            ModelHelpers.RefreshModel();
            unexportedMutex.ReleaseMutex();
        }

        private void InstallUpdate(object sender, MouseButtonEventArgs e)
        {
            if (ModelHelpers.Gallifrey.VersionControl.UpdateInstalled)
            {
                ModelHelpers.CloseApp(true);
                ModelHelpers.Gallifrey.TrackEvent(TrackingType.ManualUpdateRestart);
            }
        }

        private void GoToRunningTimer(object sender, MouseButtonEventArgs e)
        {
            ModelHelpers.Gallifrey.TrackEvent(TrackingType.ShowRunningTimer);
            ModelHelpers.SelectRunningTimer();
        }
    }
}
