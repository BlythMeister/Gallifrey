using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Gallifrey.ExtensionMethods;
using Gallifrey.JiraTimers;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class LockedTimer
    {
        private readonly ModelHelpers modelHelpers;
        private TimerDisplayModel selectedTimerSelectorModel;
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

            var selectedTimers = selected.Select(x => modelHelpers.Gallifrey.IdleTimerCollection.GetTimer(x.UniqueId)).Where(x => x != null).ToList();
            if (selected.Count != selectedTimers.Count)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Out Of Date", "The Timer Window Is Out Of Date And Needs To Be Refreshed");
                DataModel.RefreshLockedTimers(modelHelpers.Gallifrey.IdleTimerCollection.GetUnusedLockTimers());
                Focus();
                return;
            }

            var selectedTime = new TimeSpan();
            var lockedTimerDate = DateTime.MinValue;
            foreach (var lockedTimerModel in selected)
            {
                if (lockedTimerDate == DateTime.MinValue || lockedTimerDate.Date == lockedTimerModel.DateForTimer)
                {
                    lockedTimerDate = lockedTimerModel.DateForTimer;
                    selectedTime = selectedTime.Add(lockedTimerModel.IdleTime);
                }
                else
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Selection", "All Timers Must Be On The Same Date!");
                    Focus();
                    return;
                }
            }

            JiraTimer runningTimer = null;
            var runningTimerId = modelHelpers.Gallifrey.JiraTimerCollection.GetRunningTimerId();
            if (runningTimerId.HasValue)
            {
                runningTimer = modelHelpers.Gallifrey.JiraTimerCollection.GetTimer(runningTimerId.Value);
                if (runningTimer.DateStarted.Date != lockedTimerDate.Date)
                {
                    runningTimer = null;
                }
            }

            MessageDialogResult result;
            if (runningTimer != null)
            {
                result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Add Time Where?", $"Where Would You Like To Add The Time Worth {selectedTime.FormatAsString()}?", MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary, new MetroDialogSettings { AffirmativeButtonText = "New Timer", NegativeButtonText = $"Running Timer ({runningTimer.JiraReference})", FirstAuxiliaryButtonText = "Existing Timer", SecondAuxiliaryButtonText = "Cancel" });

                if (result == MessageDialogResult.SecondAuxiliary)
                {
                    Focus();
                    return;
                }
            }
            else
            {
                result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Add Time Where?", $"Where Would You Like To Add The Time Worth {selectedTime.FormatAsString()}?", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings { AffirmativeButtonText = "New Timer", NegativeButtonText = "Existing Timer", FirstAuxiliaryButtonText = "Cancel" });

                if (result == MessageDialogResult.FirstAuxiliary)
                {
                    Focus();
                    return;
                }

                //All buttons moved allong one as no running timer.
                if (result == MessageDialogResult.Negative)
                {
                    result = MessageDialogResult.FirstAuxiliary;
                }
            }

            if (result == MessageDialogResult.Affirmative)
            {
                modelHelpers.CloseFlyout(this);
                var addFlyout = new AddTimer(modelHelpers, startDate: lockedTimerDate, enableDateChange: false, idleTimers: selectedTimers);
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
            }
            else if (result == MessageDialogResult.Negative)
            {
                modelHelpers.Gallifrey.JiraTimerCollection.AddIdleTimer(runningTimerId.Value, selectedTimers);

                foreach (var lockedTimerModel in selected)
                {
                    modelHelpers.Gallifrey.IdleTimerCollection.RemoveTimer(lockedTimerModel.UniqueId);
                }

                if (modelHelpers.Gallifrey.IdleTimerCollection.GetUnusedLockTimers().Any())
                {
                    DataModel.RefreshLockedTimers(modelHelpers.Gallifrey.IdleTimerCollection.GetUnusedLockTimers());
                }
                else
                {
                    modelHelpers.CloseFlyout(this);
                }
            }
            else if (result == MessageDialogResult.FirstAuxiliary)
            {
                var items = modelHelpers.Gallifrey.JiraTimerCollection.GetTimersForADate(lockedTimerDate).Select(x => new TimerDisplayModel(x)).ToList();

                var dialog = (BaseMetroDialog)this.Resources["TimerSelector"];

                await DialogCoordinator.Instance.ShowMetroDialogAsync(modelHelpers.DialogContext, dialog);

                var comboBox = dialog.FindChild<ComboBox>("Items");
                comboBox.ItemsSource = items;

                await dialog.WaitUntilUnloadedAsync();

                if (selectedTimerSelectorModel != null)
                {
                    modelHelpers.Gallifrey.JiraTimerCollection.AddIdleTimer(selectedTimerSelectorModel.Timer.UniqueId, selectedTimers);

                    foreach (var lockedTimerModel in selected)
                    {
                        modelHelpers.Gallifrey.IdleTimerCollection.RemoveTimer(lockedTimerModel.UniqueId);
                    }
                }

                if (modelHelpers.Gallifrey.IdleTimerCollection.GetUnusedLockTimers().Any())
                {
                    DataModel.RefreshLockedTimers(modelHelpers.Gallifrey.IdleTimerCollection.GetUnusedLockTimers());
                }
                else
                {
                    modelHelpers.CloseFlyout(this);
                }

            }
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

        private async void CloseTimerSelector(object sender, RoutedEventArgs e)
        {
            var dialog = (BaseMetroDialog)this.Resources["TimerSelector"];
            var comboBox = dialog.FindChild<ComboBox>("Items");

            selectedTimerSelectorModel = (TimerDisplayModel)comboBox.SelectedItem;

            await DialogCoordinator.Instance.HideMetroDialogAsync(modelHelpers.DialogContext, dialog);
        }

        private async void CancelTimerSelector(object sender, RoutedEventArgs e)
        {
            var dialog = (BaseMetroDialog)this.Resources["TimerSelector"];
            
            selectedTimerSelectorModel = null;

            await DialogCoordinator.Instance.HideMetroDialogAsync(modelHelpers.DialogContext, dialog);
        }
    }
}
