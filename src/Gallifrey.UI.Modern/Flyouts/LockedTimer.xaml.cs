using System;
using System.Linq;
using System.Windows;
using Gallifrey.ExtensionMethods;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class LockedTimer
    {
        private readonly ModelHelpers modelHelpers;
        private LockedTimerCollectionModel DataModel => (LockedTimerCollectionModel)DataContext;

        public LockedTimer(ModelHelpers modelHelpers)
        {
            this.modelHelpers = modelHelpers;
            InitializeComponent();

            var idleTimers = modelHelpers.Gallifrey.IdleTimerCollection.GetUnusedLockTimers().ToList();
            
            DataContext = new LockedTimerCollectionModel(idleTimers);
        }

        private async void LockedTimer_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!DataModel.LockedTimers.Any())
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "No Timers To Show", "You Have No Locked Timers\nThere is Nothing To Show Here!");
                modelHelpers.CloseFlyout(this);
            }
        }

        private async void AddButton(object sender, RoutedEventArgs e)
        {
            var selected = DataModel.LockedTimers.Where(x => x.IsSelected).ToList();

            if (selected.Count == 0)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Nothing Selected", "You Have Not Selected Any Locked Time To Add");
                Focus();
                return;
            }

            var selectedTime = new TimeSpan();
            selectedTime = selected.Aggregate(selectedTime, (current, lockedTimerModel) => current.Add(lockedTimerModel.IdleTime));

            var lockedTimerDate = DateTime.MinValue;
            foreach (var lockedTimerModel in selected)
            {
                if (lockedTimerDate == DateTime.MinValue || lockedTimerDate.Date == lockedTimerModel.DateForTimer)
                {
                    lockedTimerDate = lockedTimerModel.DateForTimer;
                }
                else
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Selection", "All Timers Must Be On The Same Date!");
                    Focus();
                    return;
                }
            }

            modelHelpers.CloseFlyout(this);
            var addFlyout = new AddTimer(modelHelpers, startDate: lockedTimerDate, enableDateChange: false, preloadTime: selectedTime, enableTimeChange: false);
            await modelHelpers.OpenFlyout(addFlyout);
            
            if (addFlyout.AddedTimer)
            {
                foreach (var lockedTimerModel in selected)
                {
                    modelHelpers.Gallifrey.IdleTimerCollection.RemoveTimer(lockedTimerModel.UniqueId);
                }

                if (modelHelpers.Gallifrey.IdleTimerCollection.GetUnusedLockTimers().Any())
                {
                    modelHelpers.OpenFlyout(this);
                    DataModel.RefreshLockedTimers(modelHelpers.Gallifrey.IdleTimerCollection.GetUnusedLockTimers());
                }
            }
            else
            {
                modelHelpers.OpenFlyout(this);
            }

            Focus();
        }

        private async void DeleteButton(object sender, RoutedEventArgs e)
        {
            var selected = DataModel.LockedTimers.Where(x => x.IsSelected).ToList();

            if (selected.Count == 0)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Nothing Selected", "You Have Not Selected Any Locked Time To Delete");
                Focus();
                return;
            }

            var selectedTime = new TimeSpan();
            selectedTime = selected.Aggregate(selectedTime, (current, lockedTimerModel) => current.Add(lockedTimerModel.IdleTime));

            var result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Are You Sure?", $"Are you Sure You Want To Delete Locked Timers Worth {selectedTime.FormatAsString()}?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

            if (result == MessageDialogResult.Affirmative)
            {
                foreach (var lockedTimerModel in selected)
                {
                    modelHelpers.Gallifrey.IdleTimerCollection.RemoveTimer(lockedTimerModel.UniqueId);
                }

                if (!modelHelpers.Gallifrey.IdleTimerCollection.GetUnusedLockTimers().Any())
                {
                    modelHelpers.CloseFlyout(this);
                    return;
                }

                DataModel.RefreshLockedTimers(modelHelpers.Gallifrey.IdleTimerCollection.GetUnusedLockTimers());
            }

            Focus();
        }
    }
}
