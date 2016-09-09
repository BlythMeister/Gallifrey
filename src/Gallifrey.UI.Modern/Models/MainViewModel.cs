using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using Gallifrey.Comparers;
using Gallifrey.ExtensionMethods;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.Versions;

namespace Gallifrey.UI.Modern.Models
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ModelHelpers ModelHelpers { get; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<TimerDateModel> TimerDates { get; private set; }
        public string InactiveMinutes { get; private set; }
        public TimeSpan TimeTimeActivity { get; private set; }

        public MainViewModel(ModelHelpers modelHelpers)
        {
            ModelHelpers = modelHelpers;
            TimerDates = new ObservableCollection<TimerDateModel>();

            var backgroundRefresh = new Timer(3600000);
            backgroundRefresh.Elapsed += (sender, args) => RefreshModel();
            backgroundRefresh.Start();

            modelHelpers.Gallifrey.VersionControl.NewVersionInstalled += (sender, args) => NewVersionInstalled();
            modelHelpers.Gallifrey.IsPremiumChanged += (sender, args) => PremiumChanged();
            modelHelpers.Gallifrey.BackendModifiedTimers += (sender, args) => BackendModification();
            modelHelpers.Gallifrey.SettingsChanged += (sender, args) => SettingsChanged();
            modelHelpers.Gallifrey.JiraConnection.LoggedIn += (sender, args) => UserLoggedIn();
            modelHelpers.Gallifrey.JiraTimerCollection.GeneralTimerModification += (sender, args) => GeneralTimerModification();
            modelHelpers.Gallifrey.DailyTrackingEvent += (sender, args) => DailyEvent();
            modelHelpers.RefreshModelEvent += (sender, args) => RefreshModel();
            modelHelpers.SelectRunningTimerEvent += (sender, args) => SelectRunningTimer();
            modelHelpers.SelectTimerEvent += (sender, timerId) => SetSelectedTimer(timerId);
        }

        public string ExportedNumber => ModelHelpers.Gallifrey.JiraTimerCollection.GetNumberExported().Item1.ToString();
        public string TotalTimerCount => ModelHelpers.Gallifrey.JiraTimerCollection.GetNumberExported().Item2.ToString();
        public string UnexportedTime => ModelHelpers.Gallifrey.JiraTimerCollection.GetTotalUnexportedTime().FormatAsString(false);
        public string Exported => ModelHelpers.Gallifrey.JiraTimerCollection.GetTotalExportedTimeThisWeek(ModelHelpers.Gallifrey.Settings.AppSettings.StartOfWeek).FormatAsString(false);

        public string ExportTarget => ModelHelpers.Gallifrey.Settings.AppSettings.GetTargetThisWeek().FormatAsString(false);
        public string ExportedTargetTotalMinutes => ModelHelpers.Gallifrey.Settings.AppSettings.GetTargetThisWeek().TotalMinutes.ToString();

        public string VersionName => ModelHelpers.Gallifrey.VersionControl.UpdateInstalled ? "Click To Install New Version" : ModelHelpers.Gallifrey.VersionControl.VersionName;
        public bool HasUpdate => ModelHelpers.Gallifrey.VersionControl.UpdateInstalled;

        public bool HasInactiveTime => !string.IsNullOrWhiteSpace(InactiveMinutes);
        public bool TimerRunning => !string.IsNullOrWhiteSpace(CurrentRunningTimerDescription);
        public bool HaveTimeToExport => !string.IsNullOrWhiteSpace(TimeToExportMessage);
        public bool HaveLocalTime => !string.IsNullOrWhiteSpace(LocalTimeMessage);
        public bool IsPremium => ModelHelpers.Gallifrey.Settings.InternalSettings.IsPremium;
        public bool IsStable => ModelHelpers.Gallifrey.VersionControl.InstanceType == InstanceType.Stable;
        public bool TrackingOnly => ModelHelpers.Gallifrey.Settings.ExportSettings.TrackingOnly;

        public string AppTitle
        {
            get
            {
                var instanceType = ModelHelpers.Gallifrey.VersionControl.InstanceType;
                var appName = IsPremium ? "Gallifrey Premium" : "Gallifrey";
                return instanceType == InstanceType.Stable ? $"{appName}" : $"{appName} ({instanceType})";
            }
        }

        public string TimeToExportMessage
        {
            get
            {
                if (TrackingOnly)
                {
                    return string.Empty;
                }

                var unexportedTime = ModelHelpers.Gallifrey.JiraTimerCollection.GetTotalExportableTime();
                var unexportedTimers = ModelHelpers.Gallifrey.JiraTimerCollection.GetStoppedUnexportedTimers();
                var unexportedCount = unexportedTimers.Count(x => !x.LocalTimer);

                var excludingRunning = string.Empty;
                var runningTimerId = ModelHelpers.Gallifrey.JiraTimerCollection.GetRunningTimerId();
                if (runningTimerId.HasValue)
                {
                    var runningTimer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(runningTimerId.Value);
                    if (!runningTimer.FullyExported && !runningTimer.LocalTimer)
                    {
                        excludingRunning = "(Excluding 1 Running Timer)";
                    }
                }

                return unexportedTime.TotalMinutes > 0 ? $"You Have {unexportedCount} Timer{(unexportedCount > 1 ? "s" : "")} Worth {unexportedTime.FormatAsString(false)} To Export {excludingRunning}" : string.Empty;
            }
        }

        public string LocalTimeMessage
        {
            get
            {
                if (TrackingOnly)
                {
                    return string.Empty;
                }

                var localTime = ModelHelpers.Gallifrey.JiraTimerCollection.GetTotalLocalTime();
                var unexportedTimers = ModelHelpers.Gallifrey.JiraTimerCollection.GetStoppedUnexportedTimers();
                var unexportedCount = unexportedTimers.Count(x => x.LocalTimer);

                var excludingRunning = string.Empty;
                var runningTimerId = ModelHelpers.Gallifrey.JiraTimerCollection.GetRunningTimerId();
                if (runningTimerId.HasValue)
                {
                    var runningTimer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(runningTimerId.Value);
                    if (runningTimer.LocalTimer)
                    {
                        excludingRunning = "(Excluding 1 Running Timer)";
                    }
                }

                return localTime.TotalMinutes > 0 ? $"You Have {unexportedCount} Local Timer{(unexportedCount > 1 ? "s" : "")} Worth {localTime.FormatAsString(false)} {excludingRunning}" : string.Empty;
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

        public void SetNoActivityMilliseconds(int millisecondsSinceActivity)
        {
            TimeTimeActivity = TimeSpan.FromMilliseconds(millisecondsSinceActivity);
            TimeTimeActivity = TimeTimeActivity.Subtract(TimeSpan.FromMilliseconds(TimeTimeActivity.Milliseconds));
            TimeTimeActivity = TimeTimeActivity.Subtract(TimeSpan.FromSeconds(TimeTimeActivity.Seconds));

            if (TimeTimeActivity.TotalMinutes > 0)
            {
                var minutesPlural = TimeTimeActivity.TotalMinutes > 1 ? "s" : "";
                InactiveMinutes = $"No Timer Running For {TimeTimeActivity.TotalMinutes} Minute{minutesPlural}";
            }
            else
            {
                InactiveMinutes = string.Empty;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InactiveMinutes"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasInactiveTime"));
        }

        public IEnumerable<Guid> GetSelectedTimerIds()
        {
            foreach (var timerDateModel in TimerDates.Where(x => x.DateIsSelected))
            {
                foreach (var timerModel in timerDateModel.Timers.Where(timerModel => timerModel.TimerIsSelected))
                {
                    yield return timerModel.JiraTimer.UniqueId;
                }
            }
        }

        #region Private Helpers

        private void RefreshModel()
        {
            var workingDays = ModelHelpers.Gallifrey.Settings.AppSettings.ExportDays.ToList();
            var workingDate = DateTime.Now.AddDays((ModelHelpers.Gallifrey.Settings.AppSettings.KeepTimersForDays - 1) * -1).Date;
            var validTimerDates = new List<DateTime>();

            while (workingDate.Date <= DateTime.Now.Date)
            {
                if (workingDays.Contains(workingDate.DayOfWeek))
                {
                    validTimerDates.Add(workingDate.Date);
                }
                workingDate = workingDate.AddDays(1);
            }

            foreach (var timerDate in ModelHelpers.Gallifrey.JiraTimerCollection.GetValidTimerDates())
            {
                if (!validTimerDates.Contains(timerDate))
                {
                    validTimerDates.Add(timerDate.Date);
                }
            }

            foreach (var timerDate in validTimerDates.OrderBy(x => x.Date))
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
                        timer.PropertyChanged += (sender, args) => TimerChanged();
                        dateModel.AddTimerModel(new TimerModel(timer));
                    }
                }

                foreach (var removeTimer in dateModel.Timers.Where(timerModel => !dateTimers.Any(x => x.UniqueId == timerModel.JiraTimer.UniqueId)).ToList())
                {
                    dateModel.RemoveTimerModel(removeTimer);
                }

                if (!dateModel.Timers.Any())
                {
                    foreach (var defaultJira in ModelHelpers.Gallifrey.Settings.AppSettings.DefaultTimers ?? new List<string>())
                    {
                        if (!dateModel.Timers.Any(x => x.JiraTimer.JiraReference == defaultJira))
                        {
                            try
                            {
                                var jira = ModelHelpers.Gallifrey.JiraConnection.GetJiraIssue(defaultJira);
                                if (jira != null)
                                {
                                    var timerId = ModelHelpers.Gallifrey.JiraTimerCollection.AddTimer(jira, timerDate.Date, TimeSpan.Zero, false);
                                    var timer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(timerId);
                                    timer.PropertyChanged += (sender, args) => TimerChanged();
                                    dateModel.AddTimerModel(new TimerModel(timer));
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
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

            SetTrackingOnlyInModel();
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
                ModelHelpers.SetSelectedTimer(runningTimer.Value);
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

        private void SetTrackingOnlyInModel()
        {
            foreach (var timerDateModel in TimerDates)
            {
                timerDateModel.SetTrackingOnly(TrackingOnly);

                foreach (var timerModel in timerDateModel.Timers)
                {
                    timerModel.SetTrackingOnly(TrackingOnly);
                }
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimerDates"));
        }

        #endregion

        #region Events

        private void NewVersionInstalled()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VersionName"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasUpdate"));
        }

        private void BackendModification()
        {
            Application.Current.Dispatcher.Invoke(RefreshModel);
        }

        private void PremiumChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AppTitle"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsPremium"));
        }

        private void UserLoggedIn()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LoggedInAs"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LoggedInDisplayName"));
        }

        private void GeneralTimerModification()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportedNumber"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalTimerCount"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UnexportedTime"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Exported"));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimeToExportMessage"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HaveTimeToExport"));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimerRunning"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentRunningTimerDescription"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportedTotalMinutes"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HaveLocalTime"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LocalTimeMessage"));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportTarget"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportedTargetTotalMinutes"));
        }

        private void SettingsChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportTarget"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportedTargetTotalMinutes"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TrackingOnly"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HaveTimeToExport"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimeToExportMessage"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HaveLocalTime"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimeToExportMessage"));

            SetTrackingOnlyInModel();
            RefreshModel();
        }

        private void TimerChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimeToExportMessage"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HaveTimeToExport"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UnexportedTime"));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimerRunning"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentRunningTimerDescription"));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportTarget"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportedTargetTotalMinutes"));
        }

        private void DailyEvent()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportTarget"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportedTargetTotalMinutes"));
        }

        #endregion
    }
}