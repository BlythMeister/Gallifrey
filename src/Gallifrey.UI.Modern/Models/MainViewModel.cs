using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using Gallifrey.Comparers;
using Gallifrey.ExtensionMethods;
using Gallifrey.UI.Modern.Helpers;

namespace Gallifrey.UI.Modern.Models
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ModelHelpers ModelHelpers { get; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<TimerDateModel> TimerDates { get; private set; }
        public string InactiveMinutes { get; private set; }

        public MainViewModel(ModelHelpers modelHelpers)
        {
            ModelHelpers = modelHelpers;
            TimerDates = new ObservableCollection<TimerDateModel>();
            var runningWatcher = new Timer(500);
            runningWatcher.Elapsed += runningWatcherElapsed;
            runningWatcher.Start();

            modelHelpers.Gallifrey.VersionControl.PropertyChanged += VersionControlPropertyChanged;
            modelHelpers.RefreshModelEvent += (sender, args) => RefreshModel();
            modelHelpers.SelectRunningTimerEvent += (sender, args) => SelectRunningTimer();
            modelHelpers.SelectTimerEvent += (sender, timerId) => SetSelectedTimer(timerId);
        }

        public string ExportedNumber => ModelHelpers.Gallifrey.JiraTimerCollection.GetNumberExported().Item1.ToString();
        public string TotalTimerCount => ModelHelpers.Gallifrey.JiraTimerCollection.GetNumberExported().Item2.ToString();
        public string UnexportedTime => ModelHelpers.Gallifrey.JiraTimerCollection.GetTotalUnexportedTime().FormatAsString(false);
        public string ExportTarget => ModelHelpers.Gallifrey.Settings.AppSettings.GetTargetThisWeek().FormatAsString(false);
        public string Exported => ModelHelpers.Gallifrey.JiraTimerCollection.GetTotalExportedTimeThisWeek(ModelHelpers.Gallifrey.Settings.AppSettings.StartOfWeek).FormatAsString(false);
        public string ExportedTargetTotalMinutes => ModelHelpers.Gallifrey.Settings.AppSettings.GetTargetThisWeek().TotalMinutes.ToString();
        public string VersionName => ModelHelpers.Gallifrey.VersionControl.UpdateInstalled ? "Click To Install New Version" : ModelHelpers.Gallifrey.VersionControl.VersionName;
        public bool HasUpdate => ModelHelpers.Gallifrey.VersionControl.UpdateInstalled;
        public bool HasInactiveTime => !string.IsNullOrWhiteSpace(InactiveMinutes);
        public bool TimerRunning => !string.IsNullOrWhiteSpace(CurrentRunningTimerDescription);
        public bool HaveTimeToExport => !string.IsNullOrWhiteSpace(TimeToExportMessage);

        public string TimeToExportMessage
        {
            get
            {
                var unexportedTime = ModelHelpers.Gallifrey.JiraTimerCollection.GetTotalExportableTime();
                var unexportedTimers = ModelHelpers.Gallifrey.JiraTimerCollection.GetStoppedUnexportedTimers();
                var unexportedCount = unexportedTimers.Count();

                var excludingRunning = string.Empty;
                var runningTimerId = ModelHelpers.Gallifrey.JiraTimerCollection.GetRunningTimerId();
                if (runningTimerId.HasValue)
                {
                    var runningTimer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(runningTimerId.Value);
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
                var runningTimerId = ModelHelpers.Gallifrey.JiraTimerCollection.GetRunningTimerId();
                if (!runningTimerId.HasValue) return string.Empty;
                var runningTimer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(runningTimerId.Value);

                return $"Currently Running {runningTimer.JiraReference} ({runningTimer.JiraName})";
            }
        }

        public string ExportedTotalMinutes
        {
            get
            {
                var maximum = ModelHelpers.Gallifrey.Settings.AppSettings.GetTargetThisWeek().TotalMinutes;
                var value = ModelHelpers.Gallifrey.JiraTimerCollection.GetTotalExportedTimeThisWeek(ModelHelpers.Gallifrey.Settings.AppSettings.StartOfWeek).TotalMinutes;

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportedTotalMinutes"));
        }

        private void RefreshModel()
        {
            var validTimerDates = ModelHelpers.Gallifrey.JiraTimerCollection.GetValidTimerDates().ToList();

            foreach (var timerDate in validTimerDates)
            {
                var dateModel = TimerDates.FirstOrDefault(x => x.TimerDate.Date == timerDate.Date);

                if (dateModel == null)
                {
                    dateModel = new TimerDateModel(timerDate, ModelHelpers.Gallifrey.JiraTimerCollection);
                    TimerDates.Add(dateModel);
                }

                var dateTimers = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimersForADate(timerDate).ToList();

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

        private void SetSelectedTimer(Guid value)
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

        private void SelectRunningTimer()
        {
            var runningTimer = ModelHelpers.Gallifrey.JiraTimerCollection.GetRunningTimerId();
            if (runningTimer.HasValue)
            {
                SetSelectedTimer(runningTimer.Value);
            }
            else
            {
                var dates = ModelHelpers.Gallifrey.JiraTimerCollection.GetValidTimerDates();
                if (dates.Any())
                {
                    var date = dates.Max();
                    var topTimer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimersForADate(date).OrderBy(x => x.JiraReference, new JiraReferenceComparer()).FirstOrDefault();
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

        public IEnumerable<Guid> GetSelectedTimerIds()
        {
            foreach (var timerDateModel in TimerDates.Where(x => x.IsSelected))
            {
                foreach (var timerModel in timerDateModel.Timers.Where(timerModel => timerModel.IsSelected))
                {
                    yield return timerModel.JiraTimer.UniqueId;
                }
            }
        }
    }
}