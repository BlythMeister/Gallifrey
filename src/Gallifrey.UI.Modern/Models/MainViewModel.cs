using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Gallifrey.Comparers;
using Gallifrey.ExtensionMethods;
using Gallifrey.UI.Modern.Helpers;
using MahApps.Metro.Controls;

namespace Gallifrey.UI.Modern.Models
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel(IBackend gallifrey, FlyoutsControl flyoutsControl)
        {
            this.flyoutsControl = flyoutsControl;
            DialogContext = new DialogContext();
            Gallifrey = gallifrey;
            TimerDates = new ObservableCollection<TimerDateModel>();
            var runningWatcher = new Timer(500);
            runningWatcher.Elapsed += runningWatcherElapsed;
            runningWatcher.Start();
            Gallifrey.VersionControl.PropertyChanged += VersionControlPropertyChanged;
        }

        private readonly FlyoutsControl flyoutsControl;
        public event PropertyChangedEventHandler PropertyChanged;
        public IBackend Gallifrey { get; }
        public DialogContext DialogContext { get; private set; }
        public ObservableCollection<TimerDateModel> TimerDates { get; private set; }
        public string InactiveMinutes { get; private set; }

        public string ExportedNumber => Gallifrey.JiraTimerCollection.GetNumberExported().Item1.ToString();
        public string TotalTimerCount => Gallifrey.JiraTimerCollection.GetNumberExported().Item2.ToString();
        public string UnexportedTime => Gallifrey.JiraTimerCollection.GetTotalUnexportedTime().FormatAsString(false);
        public string ExportTarget => Gallifrey.Settings.AppSettings.GetTargetThisWeek().FormatAsString(false);
        public string Exported => Gallifrey.JiraTimerCollection.GetTotalExportedTimeThisWeek(Gallifrey.Settings.AppSettings.StartOfWeek).FormatAsString(false);
        public string ExportedTargetTotalMinutes => Gallifrey.Settings.AppSettings.GetTargetThisWeek().TotalMinutes.ToString();
        public string VersionName => Gallifrey.VersionControl.UpdateInstalled ? "Click To Install New Version" : Gallifrey.VersionControl.VersionName;
        public bool HasUpdate => Gallifrey.VersionControl.UpdateInstalled;
        public bool HasInactiveTime => !string.IsNullOrWhiteSpace(InactiveMinutes);
        public bool TimerRunning => !string.IsNullOrWhiteSpace(CurrentRunningTimerDescription);
        public bool HaveTimeToExport => !string.IsNullOrWhiteSpace(TimeToExportMessage);

        public string TimeToExportMessage
        {
            get
            {
                var unexportedTime = Gallifrey.JiraTimerCollection.GetTotalExportableTime();
                var unexportedTimers = Gallifrey.JiraTimerCollection.GetStoppedUnexportedTimers();
                var unexportedCount = unexportedTimers.Count();

                var excludingRunning = string.Empty;
                var runningTimerId = Gallifrey.JiraTimerCollection.GetRunningTimerId();
                if (runningTimerId.HasValue)
                {
                    var runningTimer = Gallifrey.JiraTimerCollection.GetTimer(runningTimerId.Value);
                    if (!runningTimer.FullyExported)
                    {
                        excludingRunning = "(Excluding 1 Running Timer)";
                    }
                }

                return unexportedTime.TotalMinutes > 0 ? $"You Have {unexportedCount} Timer{(unexportedCount > 1 ? "s" : "")} Worth {unexportedTime.FormatAsString(false)} To Export {excludingRunning}" : string.Empty;
            }
        }

        public string CurrentRunningTimerDescription
        {
            get
            {
                var runningTimerId = Gallifrey.JiraTimerCollection.GetRunningTimerId();
                if (!runningTimerId.HasValue) return string.Empty;
                var runningTimer = Gallifrey.JiraTimerCollection.GetTimer(runningTimerId.Value);

                return $"Currently Running {runningTimer.JiraReference} ({runningTimer.JiraName})";
            }
        }

        public string ExportedTotalMinutes
        {
            get
            {
                var maximum = Gallifrey.Settings.AppSettings.GetTargetThisWeek().TotalMinutes;
                var value = Gallifrey.JiraTimerCollection.GetTotalExportedTimeThisWeek(Gallifrey.Settings.AppSettings.StartOfWeek).TotalMinutes;

                if (value > maximum)
                {
                    value = maximum;
                }

                return value.ToString();
            }
        }

        private void VersionControlPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VersionName"));
        }

        private void runningWatcherElapsed(object sender, ElapsedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportedNumber"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalTimerCount"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UnexportedTime"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportTarget"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Exported"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportedTargetTotalMinutes"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasUpdate"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimerRunning"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentRunningTimerDescription"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimeToExportMessage"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HaveTimeToExport"));
        }

        public void RefreshModel()
        {
            var validTimerDates = Gallifrey.JiraTimerCollection.GetValidTimerDates().ToList();

            foreach (var timerDate in validTimerDates)
            {
                var dateModel = TimerDates.FirstOrDefault(x => x.TimerDate.Date == timerDate.Date);

                if (dateModel == null)
                {
                    dateModel = new TimerDateModel(timerDate, Gallifrey.JiraTimerCollection);
                    TimerDates.Add(dateModel);
                }

                var dateTimers = Gallifrey.JiraTimerCollection.GetTimersForADate(timerDate).ToList();

                foreach (var timer in dateTimers)
                {
                    if (!dateModel.Timers.Any(x => x.JiraTimer.UniqueId == timer.UniqueId))
                    {
                        dateModel.AddTimerModel(new TimerModel(timer));
                    }
                }

                var removeTimers = dateModel.Timers.Where(timerModel => !dateTimers.Any(x => x.UniqueId == timerModel.JiraTimer.UniqueId)).ToList();

                foreach (var removeTimer in removeTimers)
                {
                    dateModel.RemoveTimerModel(removeTimer);
                }
            }

            //see if the order would be different now, and if so, recreate the TimerDates
            var orderedCollection = TimerDates.Where(x => validTimerDates.Contains(x.TimerDate)).OrderByDescending(x => x.TimerDate).ToList();
            if (orderedCollection.Count != TimerDates.Count)
            {
                TimerDates = new ObservableCollection<TimerDateModel>(orderedCollection);
            }
            else
            {
                for (var i = 0; i < TimerDates.Count; i++)
                {
                    var main = TimerDates[i];
                    var ordered = orderedCollection[i];

                    if (main.TimerDate != ordered.TimerDate)
                    {
                        TimerDates = new ObservableCollection<TimerDateModel>(orderedCollection);
                        break;
                    }
                }
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimerDates"));

            runningWatcherElapsed(this, null);
        }

        public Guid? GetSelectedTimerId()
        {
            foreach (var timerDateModel in TimerDates.Where(x => x.IsSelected))
            {
                foreach (var timerModel in timerDateModel.Timers.Where(timerModel => timerModel.IsSelected))
                {
                    return timerModel.JiraTimer.UniqueId;
                }
            }

            return null;
        }

        public DateTime? GetSelectedDateTab()
        {
            var selectedDate = TimerDates.FirstOrDefault(x => x.IsSelected);
            return selectedDate?.TimerDate;
        }

        public bool HaveTimerToday()
        {
            return TimerDates.Any(x => x.TimerDate.Date == DateTime.Now.Date);
        }

        public void SetSelectedTimer(Guid value)
        {
            foreach (var timerDateModel in TimerDates)
            {
                var selectedTimerFound = false;
                foreach (var timerModel in timerDateModel.Timers)
                {
                    if (timerModel.JiraTimer.UniqueId == value)
                    {
                        timerModel.SetSelected(true);
                        selectedTimerFound = true;
                    }
                    else
                    {
                        timerModel.SetSelected(false);
                    }
                }

                timerDateModel.SetSelected(selectedTimerFound);
            }
        }

        public void SelectRunningTimer()
        {
            var runningTimer = Gallifrey.JiraTimerCollection.GetRunningTimerId();
            if (runningTimer.HasValue)
            {
                SetSelectedTimer(runningTimer.Value);
            }
            else
            {
                var dates = Gallifrey.JiraTimerCollection.GetValidTimerDates();
                if (dates.Any())
                {
                    var date = dates.Max();
                    var topTimer = Gallifrey.JiraTimerCollection.GetTimersForADate(date).OrderBy(x => x.JiraReference, new JiraReferenceComparer()).FirstOrDefault();
                    if (topTimer != null)
                    {
                        SetSelectedTimer(topTimer.UniqueId);
                    }
                }
            }
        }

        public void SetNoActivityMilliseconds(int millisecondsSinceActivity)
        {
            if (millisecondsSinceActivity == 0)
            {
                InactiveMinutes = string.Empty;
            }
            else
            {
                var minutesSinceActivity = (millisecondsSinceActivity / 1000) / 60;
                var minutesPlural = string.Empty;
                if (minutesSinceActivity > 1)
                {
                    minutesPlural = "s";
                }

                InactiveMinutes = $"No Timer Running For {minutesSinceActivity} Minute{minutesPlural}";
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InactiveMinutes"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasInactiveTime"));
        }

        public void CloseAllFlyouts()
        {
            foreach (Flyout item in flyoutsControl.Items)
            {
                item.IsOpen = false;
            }
        }

        public Task<Flyout> OpenFlyout(Flyout flyout)
        {
            var actualType = flyout.GetType();
            var taskCompletion = new TaskCompletionSource<Flyout>();

            //Prevent 2 identical flyouts from opening
            if (flyoutsControl.Items.Cast<Flyout>().Any(item => item.GetType() == actualType))
            {
                taskCompletion.SetResult(flyout);
            }
            else
            {
                flyoutsControl.Items.Add(flyout);

                // when the flyout is closed, remove it from the hosting FlyoutsControl
                RoutedEventHandler closingFinishedHandler = null;
                closingFinishedHandler = (o, args) =>
                {
                    flyout.ClosingFinished -= closingFinishedHandler;
                    flyoutsControl.Items.Remove(flyout);
                    taskCompletion.SetResult(flyout);
                };

                flyout.ClosingFinished += closingFinishedHandler;
                flyout.IsOpen = true;
            }

            return taskCompletion.Task;
        }
    }
}