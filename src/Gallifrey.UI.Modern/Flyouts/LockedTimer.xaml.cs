using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Gallifrey.ChangeLog;
using Gallifrey.ExtensionMethods;
using Gallifrey.IdleTimers;
using Gallifrey.JiraTimers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for ChangeLog.xaml
    /// </summary>
    public partial class LockedTimer
    {
        private readonly MainViewModel viewModel;
        private LockedTimerCollectionModel DataModel { get { return (LockedTimerCollectionModel)DataContext; } }

        public LockedTimer(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
            InitializeComponent();

            var idleTimers = viewModel.Gallifrey.IdleTimerCollection.GetUnusedLockTimers();

            if (!idleTimers.Any())
            {
                DialogCoordinator.Instance.ShowMessageAsync(viewModel, "No Timers To Show", "You Have No Locked Timers, Come Back When You Do!");
            }

            DataContext = new LockedTimerCollectionModel(idleTimers);
        }

        private async void AddButton(object sender, RoutedEventArgs e)
        {
            var selected = DataModel.LockedTimers.Where(x => x.IsSelected).ToList();

            if (selected.Count == 0)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(viewModel, "Nothing Selected", "You Have Not Selected Any Locked Time To Add");
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
                    await DialogCoordinator.Instance.ShowMessageAsync(viewModel, "Invalid Selection", "All Timers Must Be On The Same Date!");
                    return;
                }
            }

            var addFlyout = new AddTimer(viewModel, startDate: lockedTimerDate, enableDateChange: false, preloadTime: selectedTime, enableTimeChange: false);
            await viewModel.OpenFlyout(addFlyout);

            if (addFlyout.AddedTimer)
            {
                foreach (var lockedTimerModel in selected)
                {
                    viewModel.Gallifrey.IdleTimerCollection.RemoveTimer(lockedTimerModel.UniqueId);
                }

                DataModel.RefreshLockedTimers(viewModel.Gallifrey.IdleTimerCollection.GetUnusedLockTimers());
            }
        }

        private async void DeleteButton(object sender, RoutedEventArgs e)
        {
            var selected = DataModel.LockedTimers.Where(x => x.IsSelected).ToList();

            var selectedTime = new TimeSpan();
            selectedTime = selected.Aggregate(selectedTime, (current, lockedTimerModel) => current.Add(lockedTimerModel.IdleTime));

            var result = await DialogCoordinator.Instance.ShowMessageAsync(viewModel, "Are You Sure?", string.Format("Are you Sure You Want To Delete Locked Timers Worth {0}?", selectedTime.FormatAsString()), MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative)
            {
                foreach (var lockedTimerModel in selected)
                {
                    viewModel.Gallifrey.IdleTimerCollection.RemoveTimer(lockedTimerModel.UniqueId);
                }

                DataModel.RefreshLockedTimers(viewModel.Gallifrey.IdleTimerCollection.GetUnusedLockTimers());
            }
        }
    }

    public class LockedTimerCollectionModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<LockedTimerModel> LockedTimers { get; set; }

        public LockedTimerCollectionModel(IEnumerable<IdleTimer> unexportedTimers)
        {
            RefreshLockedTimers(unexportedTimers);
        }

        public void RefreshLockedTimers(IEnumerable<IdleTimer> unexportedTimers)
        {
            LockedTimers = new ObservableCollection<LockedTimerModel>(unexportedTimers.Select(x => new LockedTimerModel(x)).OrderBy(x => x.Date));
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("LockedTimers"));
        }
    }

    public class LockedTimerModel
    {

        public Guid UniqueId { get; private set; }
        public string Date { get; private set; }
        public string IdleTimeValue { get; private set; }
        public TimeSpan IdleTime { get; private set; }
        public bool IsSelected { get; set; }
        public DateTime DateForTimer { get; private set; }

        public LockedTimerModel(IdleTimer idleTimer)
        {
            DateForTimer = idleTimer.DateStarted.Date;
            Date = string.Format("{0} at {1}", idleTimer.DateStarted.ToString("ddd, dd MMM"), idleTimer.DateStarted.ToString("t"));
            UniqueId = idleTimer.UniqueId;
            IdleTimeValue = idleTimer.IdleTimeValue.FormatAsString();
            IdleTime = idleTimer.IdleTimeValue;
            IsSelected = false;
        }
    }
}
