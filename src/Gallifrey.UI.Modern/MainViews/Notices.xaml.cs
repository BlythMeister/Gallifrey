using Exceptionless;
using Gallifrey.AppTracking;
using Gallifrey.IdleTimers;
using Gallifrey.UI.Modern.Flyouts;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gallifrey.UI.Modern.MainViews
{
    public partial class Notices
    {
        private MainViewModel Model => (MainViewModel)DataContext;
        private ModelHelpers ModelHelpers => Model.ModelHelpers;
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
            timers.RemoveAll(x => x.LocalTimer || x.IsRunning);

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
                await ModelHelpers.ShowMessageAsync("Nothing To Export", "No Un-Exported Timers Avaliable To Export");
            }

            ModelHelpers.RefreshModel();
            unexportedMutex.ReleaseMutex();
        }

        private async void InstallUpdate(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ModelHelpers.Gallifrey.TrackEvent(TrackingType.ManualUpdateRestart);
                ModelHelpers.CloseApp(true);
            }
            catch (Exception ex)
            {
                ExceptionlessClient.Default.CreateEvent().SetException(ex).AddTags("Handled").Submit();
                await ModelHelpers.ShowMessageAsync("Update Error", "There Was An Error Trying To Update Gallifrey, If This Problem Persists Please Contact Support");
            }
        }

        private async void ReinstallUpdate(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ModelHelpers.Gallifrey.TrackEvent(TrackingType.ManualUpdateRestart);
                ModelHelpers.Gallifrey.VersionControl.ManualReinstall();
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                ExceptionlessClient.Default.CreateEvent().SetException(ex).AddTags("Handled").Submit();
                await ModelHelpers.ShowMessageAsync("Reinstall Error", "There Was An Error Trying To Reinstall Gallifrey, You May Need To Re-Download The App");
            }
            finally
            {
                ModelHelpers.CloseApp();
            }
        }

        private void GoToRunningTimer(object sender, MouseButtonEventArgs e)
        {
            ModelHelpers.Gallifrey.TrackEvent(TrackingType.ShowRunningTimer);
            ModelHelpers.SelectRunningTimer();
        }

        private async void CreateTimerFromInactive(object sender, MouseButtonEventArgs e)
        {
            var dummyIdleTimer = new IdleTimer(DateTime.Now, DateTime.Now, Model.TimeActivity, Guid.NewGuid());
            var addFlyout = new AddTimer(ModelHelpers, idleTimers: new List<IdleTimer> { dummyIdleTimer }, enableDateChange: false);
            await ModelHelpers.OpenFlyout(addFlyout);
            if (addFlyout.AddedTimer)
            {
                ModelHelpers.SetSelectedTimer(addFlyout.NewTimerId);
                ModelHelpers.Gallifrey.ResetInactiveAlert();
            }
        }
    }
}
