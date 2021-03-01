using Exceptionless;
using Gallifrey.Comparers;
using Gallifrey.ExtensionMethods;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.Versions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Timers;
using System.Windows;

namespace Gallifrey.UI.Modern.Models
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly TargetBarValues targetBarValues;

        public ModelHelpers ModelHelpers { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TimerDateModel> TimerDates { get; private set; }
        public string InactiveMinutes { get; private set; }
        public TimeSpan TimeActivity { get; private set; }

        public MainViewModel(ModelHelpers modelHelpers)
        {
            ModelHelpers = modelHelpers;
            TimerDates = new ObservableCollection<TimerDateModel>();

            var backgroundRefresh = new Timer(TimeSpan.FromHours(1).TotalMilliseconds);
            backgroundRefresh.Elapsed += (sender, args) => RefreshModel();
            backgroundRefresh.Start();

            modelHelpers.Gallifrey.VersionControl.UpdateStateChange += (sender, args) => NewVersionPresent();
            modelHelpers.Gallifrey.BackendModifiedTimers += (sender, args) => BackendModification();
            modelHelpers.Gallifrey.SettingsChanged += (sender, args) => SettingsChanged();
            modelHelpers.Gallifrey.JiraConnection.LoggedIn += (sender, args) => UserLoggedIn();
            modelHelpers.Gallifrey.JiraTimerCollection.GeneralTimerModification += (sender, args) => GeneralTimerModification();
            modelHelpers.Gallifrey.DailyTrackingEvent += (sender, args) => DailyEvent();
            modelHelpers.RefreshModelEvent += (sender, args) => RefreshModel();
            modelHelpers.SelectRunningTimerEvent += (sender, args) => SelectRunningTimer();
            modelHelpers.SelectTimerEvent += (sender, timerId) => SetSelectedTimer(timerId);

            targetBarValues = new TargetBarValues(modelHelpers.Gallifrey);
        }

        public string ExportedNumber => ModelHelpers.Gallifrey.JiraTimerCollection.GetNumberExported().Item1.ToString();
        public string TotalTimerCount => ModelHelpers.Gallifrey.JiraTimerCollection.GetNumberExported().Item2.ToString();
        public string LocalTime => ModelHelpers.Gallifrey.JiraTimerCollection.GetTotalLocalTime().FormatAsString(false);
        public string ExportableTime => ModelHelpers.Gallifrey.JiraTimerCollection.GetTotalExportableTime().FormatAsString(false);
        public string Exported => ModelHelpers.Gallifrey.JiraTimerCollection.GetTotalExportedTimeThisWeek(ModelHelpers.Gallifrey.Settings.AppSettings.StartOfWeek).FormatAsString(false);
        public string ExportTarget => ModelHelpers.Gallifrey.Settings.AppSettings.GetTargetThisWeek().FormatAsString(false);

        public string VersionName => ModelHelpers.Gallifrey.VersionControl.UpdateInstalled || ModelHelpers.Gallifrey.VersionControl.UpdateReinstallNeeded ? "NEW VERSION AVAILABLE" : ModelHelpers.Gallifrey.VersionControl.VersionName.ToUpper();
        public bool HasUpdate => ModelHelpers.Gallifrey.VersionControl.UpdateInstalled;
        public bool ReinstallNeeded => ModelHelpers.Gallifrey.VersionControl.UpdateReinstallNeeded;
        public bool UpdateError => ModelHelpers.Gallifrey.VersionControl.UpdateError;

        public bool HasInactiveTime => !string.IsNullOrWhiteSpace(InactiveMinutes);
        public bool TimerRunning => !string.IsNullOrWhiteSpace(CurrentRunningTimerDescription);
        public bool HaveTimeToExport => !string.IsNullOrWhiteSpace(TimeToExportMessage);
        public bool HaveLocalTime => !string.IsNullOrWhiteSpace(LocalTimeMessage);
        public bool IsStable => ModelHelpers.Gallifrey.VersionControl.InstanceType == InstanceType.Stable;
        public bool TrackingOnly => ModelHelpers.Gallifrey.Settings.ExportSettings.TrackingOnly;

        public string TargetBarExportedPercentage => $"{targetBarValues.ExportedWidth}*";
        public string TargetBarUnexportedPercentage => $"{targetBarValues.UnexportedWidth}*";
        public string TargetBarRemainingPercentage => $"{targetBarValues.RemainingWidth}*";

        public string TargetBarExportedLabel => $"Exported: {targetBarValues.ExportedTime.FormatAsString(false)}";
        public string TargetBarUnexportedLabel => $"Un-Exported (inc Local): {targetBarValues.UnexportedTime.FormatAsString(false)}";
        public string TargetBarRemainingLabel => $"Remaining: {targetBarValues.RemainingTime.FormatAsString(false)}";

        public string AppTitle
        {
            get
            {
                var instanceType = ModelHelpers.Gallifrey.VersionControl.InstanceType;
                return instanceType == InstanceType.Stable ? "Gallifrey" : $"Gallifrey ({instanceType})";
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

                var unexportedTime = ModelHelpers.Gallifrey.JiraTimerCollection.GetStoppedTotalExportableTime();
                var unexportedCount = ModelHelpers.Gallifrey.JiraTimerCollection.GetStoppedUnexportedTimers().Count(x => !x.LocalTimer);

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
                var unexportedCount = ModelHelpers.Gallifrey.JiraTimerCollection.GetAllLocalTimers().Count(x => x.LocalTimer);

                return localTime.TotalMinutes > 0 ? $"You Have {unexportedCount} Local Timer{(unexportedCount > 1 ? "s" : "")} Worth {localTime.FormatAsString(false)}" : string.Empty;
            }
        }

        public string CurrentRunningTimerDescription
        {
            get
            {
                var runningTimerId = ModelHelpers.Gallifrey.JiraTimerCollection.GetRunningTimerId();
                if (!runningTimerId.HasValue)
                {
                    return string.Empty;
                }

                var runningTimer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(runningTimerId.Value);

                return $"Currently Running {runningTimer.JiraReference} ({runningTimer.JiraName})";
            }
        }

        public void SetNoActivityMilliseconds(int millisecondsSinceActivity)
        {
            TimeActivity = TimeSpan.FromMilliseconds(millisecondsSinceActivity);
            TimeActivity = TimeActivity.Subtract(TimeSpan.FromMilliseconds(TimeActivity.Milliseconds));
            TimeActivity = TimeActivity.Subtract(TimeSpan.FromSeconds(TimeActivity.Seconds));

            if (TimeActivity.TotalMinutes > 0)
            {
                var minutesPlural = TimeActivity.TotalMinutes > 1 ? "s" : "";
                var newMessage = $"No Timer Running For {TimeActivity.TotalMinutes} Minute{minutesPlural}";

                if (newMessage != InactiveMinutes)
                {
                    InactiveMinutes = newMessage;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InactiveMinutes)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasInactiveTime)));

                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (TimeActivity.TotalMinutes % TimeSpan.FromMilliseconds(ModelHelpers.Gallifrey.Settings.AppSettings.AlertTimeMilliseconds).TotalMinutes == 0)
                    {
                        ModelHelpers.ShowNotification(InactiveMinutes);
                    }
                }
            }
            else
            {
                InactiveMinutes = string.Empty;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InactiveMinutes)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasInactiveTime)));
            }
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

        private class TargetBarValues
        {
            private readonly IBackend gallifrey;
            public double UnexportedWidth { get; private set; }
            public double ExportedWidth { get; private set; }
            public double RemainingWidth { get; private set; }
            public TimeSpan ExportedTime { get; private set; }
            public TimeSpan UnexportedTime { get; private set; }
            public TimeSpan RemainingTime { get; private set; }

            public TargetBarValues(IBackend gallifrey)
            {
                this.gallifrey = gallifrey;
            }

            public void Update()
            {
                var targetTime = gallifrey.Settings.AppSettings.GetTargetThisWeek();

                ExportedTime = gallifrey.JiraTimerCollection.GetTotalExportedTimeThisWeek(gallifrey.Settings.AppSettings.StartOfWeek);
                UnexportedTime = gallifrey.JiraTimerCollection.GetTotalTimeThisWeekNoSeconds(gallifrey.Settings.AppSettings.StartOfWeek).Subtract(ExportedTime);
                RemainingTime = targetTime.Subtract(ExportedTime).Subtract(UnexportedTime);

                var target = targetTime.TotalMinutes;
                var exported = ExportedTime.TotalMinutes;
                var unexported = UnexportedTime.TotalMinutes;
                var remaining = RemainingTime.TotalMinutes;

                RemainingWidth = remaining / target * 100;
                ExportedWidth = exported / target * 100;
                UnexportedWidth = unexported / target * 100;

                if (ExportedWidth >= 100)
                {
                    ExportedWidth = 100;
                    UnexportedWidth = 0;
                    RemainingWidth = 0;
                }
                else if (ExportedWidth + UnexportedWidth >= 100)
                {
                    UnexportedWidth = 100 - ExportedWidth;
                    RemainingWidth = 0;
                }
            }
        }

        private void RefreshModel()
        {
            var workingDays = ModelHelpers.Gallifrey.Settings.AppSettings.ExportDays.ToList();
            var workingDate = DateTime.Now.AddDays((ModelHelpers.Gallifrey.Settings.AppSettings.KeepTimersForDays - 1) * -1).Date;
            var validTimerDates = new List<DateTime>();
            var jiraCache = new List<Issue>();

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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimerDates)));
                }

                var dateTimers = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimersForADate(timerDate.Date).ToList();

                foreach (var timer in dateTimers)
                {
                    if (dateModel.Timers.All(x => x.JiraTimer.UniqueId != timer.UniqueId))
                    {
                        timer.PropertyChanged += (sender, args) => TimerChanged();
                        dateModel.AddTimerModel(new TimerModel(timer));
                    }
                }

                foreach (var removeTimer in dateModel.Timers.Where(timerModel => dateTimers.All(x => x.UniqueId != timerModel.JiraTimer.UniqueId)).ToList())
                {
                    dateModel.RemoveTimerModel(removeTimer);
                }

                if (dateModel.TimerDate.Date <= DateTime.Now.Date)
                {
                    var defaultTimers = (ModelHelpers.Gallifrey.Settings.AppSettings.DefaultTimers ?? new List<string>()).Select(x => x.ToUpper().Trim()).Distinct();
                    foreach (var defaultJira in defaultTimers)
                    {
                        if (!dateModel.Timers.Any(x => string.Equals(x.JiraTimer.JiraReference, defaultJira, StringComparison.InvariantCultureIgnoreCase) && x.JiraTimer.DateStarted.Date == dateModel.TimerDate.Date))
                        {
                            try
                            {
                                var jira = jiraCache.FirstOrDefault(x => string.Equals(x.key, defaultJira, StringComparison.InvariantCultureIgnoreCase));
                                if (jira == null && ModelHelpers.Gallifrey.JiraConnection.IsConnected)
                                {
                                    if (ModelHelpers.Gallifrey.JiraConnection.DoesJiraExist(defaultJira))
                                    {
                                        jira = ModelHelpers.Gallifrey.JiraConnection.GetJiraIssue(defaultJira);
                                        jiraCache.Add(jira);
                                    }
                                }

                                if (jira != null && string.Equals(jira.key, defaultJira, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    var timerId = ModelHelpers.Gallifrey.JiraTimerCollection.AddTimer(jira, dateModel.TimerDate.Date, TimeSpan.Zero, false);
                                    var timer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(timerId);
                                    if (timer != null)
                                    {
                                        timer.PropertyChanged += (sender, args) => TimerChanged();
                                        dateModel.AddTimerModel(new TimerModel(timer));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionlessClient.Default.CreateEvent().SetException(ex).AddTags("Hidden").Submit();
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimerDates)));
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
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimerDates)));
                        break;
                    }
                }
            }

            SetTrackingOnlyInModel();
            targetBarValues.Update();
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
                var dates = ModelHelpers.Gallifrey.JiraTimerCollection.GetValidTimerDates().ToList();
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimerDates)));
        }

        #endregion Private Helpers

        #region Events

        private void NewVersionPresent()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VersionName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasUpdate)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReinstallNeeded)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpdateError)));
        }

        private void BackendModification()
        {
            Application.Current.Dispatcher.Invoke(RefreshModel);
        }

        private void UserLoggedIn()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LoggedInAs"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LoggedInDisplayName"));
        }

        private void GeneralTimerModification()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExportedNumber)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalTimerCount)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UnexportedTime"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Exported)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeToExportMessage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HaveTimeToExport)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimerRunning)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentRunningTimerDescription)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HaveLocalTime)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalTimeMessage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalTime)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExportableTime)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExportTarget)));

            targetBarValues.Update();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarExportedPercentage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarUnexportedPercentage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarRemainingPercentage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarExportedLabel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarUnexportedLabel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarRemainingLabel)));
        }

        private void SettingsChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExportTarget)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrackingOnly)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HaveTimeToExport)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExportableTime)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeToExportMessage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HaveLocalTime)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalTimeMessage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalTime)));

            targetBarValues.Update();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarExportedPercentage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarUnexportedPercentage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarRemainingPercentage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarExportedLabel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarUnexportedLabel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarRemainingLabel)));

            SetTrackingOnlyInModel();
            RefreshModel();
        }

        private void TimerChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeToExportMessage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HaveTimeToExport)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UnexportedTime"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExportableTime)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimerRunning)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentRunningTimerDescription)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExportTarget)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HaveLocalTime)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalTimeMessage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalTime)));

            targetBarValues.Update();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarExportedPercentage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarUnexportedPercentage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarRemainingPercentage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarExportedLabel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarUnexportedLabel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarRemainingLabel)));
        }

        private void DailyEvent()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExportTarget)));

            targetBarValues.Update();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarExportedPercentage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarUnexportedPercentage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarRemainingPercentage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarExportedLabel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarUnexportedLabel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetBarRemainingLabel)));
        }

        #endregion Events
    }
}
