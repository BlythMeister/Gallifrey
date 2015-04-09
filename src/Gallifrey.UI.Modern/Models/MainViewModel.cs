using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using Gallifrey.Comparers;
using Gallifrey.ExtensionMethods;
using Gallifrey.UI.Modern.MainViews;

namespace Gallifrey.UI.Modern.Models
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Timer runningWatcher;
        public IBackend Gallifrey { get; private set; }
        public MainWindow MainWindow { get; private set; }
        public ObservableCollection<TimerDateModel> TimerDates { get; private set; }
        public string ExportedNumber { get { return Gallifrey.JiraTimerCollection.GetNumberExported().Item1.ToString(); } }
        public string TotalTimerCount { get { return Gallifrey.JiraTimerCollection.GetNumberExported().Item2.ToString(); } }
        public string UnexportedTime { get { return Gallifrey.JiraTimerCollection.GetTotalUnexportedTime().FormatAsString(false); } }
        public string ExportTarget { get { return Gallifrey.Settings.AppSettings.GetTargetThisWeek().FormatAsString(false); } }
        public string Exported { get { return Gallifrey.JiraTimerCollection.GetTotalExportedTimeThisWeek(Gallifrey.Settings.AppSettings.StartOfWeek).FormatAsString(false); } }
        public string ExportedTargetTotalMinutes { get { return Gallifrey.Settings.AppSettings.GetTargetThisWeek().TotalMinutes.ToString(); } }
        public string VersionName { get { return Gallifrey.VersionControl.AlreadyInstalledUpdate ? "Click To Install New Version" : Gallifrey.VersionControl.VersionName; } }
        public bool HasUpdate { get { return Gallifrey.VersionControl.AlreadyInstalledUpdate; } }
        public bool HasInactiveTime { get { return !string.IsNullOrWhiteSpace(InactiveMinutes); } }
        public string InactiveMinutes { get; private set; }

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
            set { }
        }


        public MainViewModel(IBackend gallifrey, MainWindow mainWindow)
        {
            Gallifrey = gallifrey;
            MainWindow = mainWindow;
            TimerDates = new ObservableCollection<TimerDateModel>();
            runningWatcher = new Timer(1000);
            runningWatcher.Elapsed += runningWatcherElapsed;
            Gallifrey.VersionControl.PropertyChanged += VersionControlPropertyChanged;
        }

        private void VersionControlPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("VersionName"));
        }

        private void runningWatcherElapsed(object sender, ElapsedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ExportedNumber"));
                PropertyChanged(this, new PropertyChangedEventArgs("TotalTimerCount"));
                PropertyChanged(this, new PropertyChangedEventArgs("UnexportedTime"));
                PropertyChanged(this, new PropertyChangedEventArgs("ExportTarget"));
                PropertyChanged(this, new PropertyChangedEventArgs("Exported"));
                PropertyChanged(this, new PropertyChangedEventArgs("ExportedTargetTotalMinutes"));
                PropertyChanged(this, new PropertyChangedEventArgs("HasUpdate"));
            }
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

                var dateTimers = Gallifrey.JiraTimerCollection.GetTimersForADate(timerDate);

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

            TimerDates = new ObservableCollection<TimerDateModel>(TimerDates.Where(x=>validTimerDates.Contains(x.TimerDate)).OrderByDescending(x => x.TimerDate));

            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("TimerDates"));
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

                InactiveMinutes = string.Format("No Timer Running For {0} Minute{1}", minutesSinceActivity, minutesPlural);
            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("InactiveMinutes"));
                PropertyChanged(this, new PropertyChangedEventArgs("HasInactiveTime"));
            }
        }
    }
}