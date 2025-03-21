using Gallifrey.Jira.Enum;
using Gallifrey.Jira.Model;
using Gallifrey.JiraTimers;
using Gallifrey.Settings;
using Gallifrey.UI.Modern.Helpers;
using System;
using System.ComponentModel;

namespace Gallifrey.UI.Modern.Models
{
    public class BulkExportModel : INotifyPropertyChanged
    {
        private bool shouldExport;
        private WorkLogStrategy workLogStrategy;
        private int toExportHours;
        private int toExportMinutes;
        private int remainingHours;
        private int remainingMinutes;
        private TimeSpan toExportMaxTime;
        private bool changeStatus;

        public event PropertyChangedEventHandler PropertyChanged;

        public JiraTimer Timer { get; set; }
        public DateTime ExportDate { get; set; }
        public string Comment { get; set; }
        public TimeSpan OriginalRemaining { get; set; }
        public string DefaultComment { get; set; }
        public bool StandardComment { get; set; }

        public bool ShowRemaining => workLogStrategy == WorkLogStrategy.SetValue;
        public bool HasParent => Timer.HasParent;
        public string JiraParentRef => Timer.JiraParentReference;
        public string JiraParentDesc => Timer.JiraParentName;
        public string JiraRef => Timer.JiraReference;
        public string JiraDesc => Timer.JiraName;
        public int ExportedHours => Timer.ExportedTime.Hours;
        public int ExportedMinutes => Timer.ExportedTime.Minutes;
        public TimeSpan ToExport => new TimeSpan(toExportHours, toExportMinutes, 0);
        public TimeSpan? Remaining => WorkLogStrategy == WorkLogStrategy.SetValue ? new TimeSpan(remainingHours, remainingMinutes, 0) : (TimeSpan?)null;
        public string TickOrCross => shouldExport ? "✓" : "X";
        public string Header => $"{JiraRef} [{ExportDate:ddd, dd MMM}] [{TickOrCross}]";

        public WorkLogStrategy WorkLogStrategy
        {
            get => workLogStrategy;
            set
            {
                workLogStrategy = value;
                SetRemaining();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowRemaining)));
            }
        }

        public bool ChangeStatus
        {
            get => changeStatus;
            set
            {
                changeStatus = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChangeStatus)));
            }
        }

        public bool ShouldExport
        {
            get => shouldExport;
            set
            {
                shouldExport = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShouldExport)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Header)));
            }
        }

        public int? ToExportHours
        {
            get => toExportHours;
            set
            {
                var newValue = value ?? 0;
                HourMinuteHelper.UpdateHours(ref toExportHours, newValue, 23);

                if (ToExport > toExportMaxTime)
                {
                    toExportHours = toExportMaxTime.Hours;
                    toExportMinutes = toExportMaxTime.Minutes;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToExportHours)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToExportMinutes)));
                }
            }
        }

        public int? ToExportMinutes
        {
            get => toExportMinutes;
            set
            {
                var newValue = value ?? 0;
                HourMinuteHelper.UpdateMinutes(ref toExportHours, ref toExportMinutes, newValue, 23, out var hoursChanged);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToExportMinutes)));
                if (hoursChanged)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToExportHours)));
                }

                if (ToExport > toExportMaxTime)
                {
                    toExportHours = toExportMaxTime.Hours;
                    toExportMinutes = toExportMaxTime.Minutes;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToExportHours)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToExportMinutes)));
                }
            }
        }

        public int? RemainingHours
        {
            get => remainingHours;
            set
            {
                var newValue = value ?? 0;
                HourMinuteHelper.UpdateHours(ref remainingHours, newValue, 999);
            }
        }

        public int? RemainingMinutes
        {
            get => remainingMinutes;
            set
            {
                var newValue = value ?? 0;
                HourMinuteHelper.UpdateMinutes(ref remainingHours, ref remainingMinutes, newValue, 999, out var hoursChanged);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingMinutes)));
                if (hoursChanged)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingHours)));
                }
            }
        }

        public BulkExportModel(JiraTimer timer, Issue jiraIssue, IExportSettings exportSettings)
        {
            UpdateTimer(timer);

            ExportDate = timer.DateStarted.Date != DateTime.Now.Date ? timer.DateStarted.Date.AddHours(12) : DateTime.Now;

            switch (exportSettings.DefaultRemainingValue)
            {
                case DefaultRemaining.Auto:
                    WorkLogStrategy = WorkLogStrategy.Automatic;
                    break;

                case DefaultRemaining.Leave:
                    WorkLogStrategy = WorkLogStrategy.LeaveRemaining;
                    break;

                case DefaultRemaining.Set:
                    WorkLogStrategy = WorkLogStrategy.SetValue;
                    break;
            }

            DefaultComment = exportSettings.EmptyExportComment;

            OriginalRemaining = jiraIssue.fields.timetracking != null ? TimeSpan.FromSeconds(jiraIssue.fields.timetracking.remainingEstimateSeconds) : TimeSpan.Zero;

            SetRemaining();
        }

        private void UpdateTimer(JiraTimer timer)
        {
            Timer = timer;

            toExportMaxTime = new TimeSpan(timer.TimeToExport.Hours, timer.TimeToExport.Minutes, 0);
            ToExportHours = toExportMaxTime.Hours;
            ToExportMinutes = toExportMaxTime.Minutes;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToExportHours)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToExportMinutes)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasParent)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JiraParentRef)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JiraParentDesc)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JiraRef)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JiraDesc)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExportedHours)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExportedMinutes)));
        }

        private void SetRemaining()
        {
            switch (workLogStrategy)
            {
                case WorkLogStrategy.Automatic:
                    var autoNewRemaining = OriginalRemaining.Subtract(ToExport);
                    if (autoNewRemaining.TotalSeconds < 0)
                    {
                        autoNewRemaining = TimeSpan.Zero;
                    }

                    RemainingHours = autoNewRemaining.Hours + (autoNewRemaining.Days * 24);
                    RemainingMinutes = autoNewRemaining.Minutes;
                    break;

                case WorkLogStrategy.LeaveRemaining:
                    RemainingHours = OriginalRemaining.Hours + (OriginalRemaining.Days * 24);
                    RemainingMinutes = OriginalRemaining.Minutes;
                    break;

                case WorkLogStrategy.SetValue:
                    RemainingHours = 0;
                    RemainingMinutes = 0;
                    break;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingHours)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingMinutes)));
        }
    }
}
