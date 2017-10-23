using Gallifrey.AppTracking;
using Gallifrey.IdleTimers;
using Gallifrey.UI.Modern.Flyouts;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        private void ReinstallUpdate(object sender, MouseButtonEventArgs e)
        {
            if (ModelHelpers.Gallifrey.VersionControl.UpdateReinstallNeeded)
            {
                ModelHelpers.Gallifrey.VersionControl.ManualReinstall();
                ModelHelpers.Gallifrey.TrackEvent(TrackingType.ManualUpdateRestart);
            }
        }

        private void GoToRunningTimer(object sender, MouseButtonEventArgs e)
        {
            ModelHelpers.Gallifrey.TrackEvent(TrackingType.ShowRunningTimer);
            ModelHelpers.SelectRunningTimer();
        }

        private async void CreateTimerFromInactive(object sender, MouseButtonEventArgs e)
        {
            var dummyIdleTimer = new IdleTimer(DateTime.Now, DateTime.Now, Model.TimeTimeActivity, Guid.NewGuid());
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
