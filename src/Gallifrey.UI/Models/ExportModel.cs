using System;
using System.ComponentModel;
using Gallifrey.Jira.Enum;
using Gallifrey.Jira.Model;
using Gallifrey.JiraTimers;
using Gallifrey.Settings;

namespace Gallifrey.UI.Models
{
    public class ExportModel : INotifyPropertyChanged
    {
        private TimeSpan? optionalExportLimit;
        private WorkLogStrategy workLogStrategy;
        public event PropertyChangedEventHandler PropertyChanged;

        public JiraTimer Timer { get; set; }
        public int ToExportHours { get; set; }
        public int ToExportMaxHours { get; set; }
        public int ToExportMinutes { get; set; }
        public int RemainingHours { get; set; }
        public int RemainingMinutes { get; set; }
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
        public TimeSpan ToExport => new TimeSpan(ToExportHours, ToExportMinutes, 0);
        public TimeSpan? Remaining => WorkLogStrategy == WorkLogStrategy.SetValue ? new TimeSpan(RemainingHours, RemainingMinutes, 0) : (TimeSpan?)null;

        public WorkLogStrategy WorkLogStrategy
        {
            get { return workLogStrategy; }
            set
            {
                workLogStrategy = value;
                SetRemaining();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowRemaining"));
            }
        }

        public ExportModel(JiraTimer timer, TimeSpan? exportTime, IExportSettings exportSettings)
        {
            UpdateTimer(timer, exportTime);

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
        }

        public void UpdateTimer(JiraTimer timer, Issue jiraIssue)
        {
            UpdateTimer(timer, optionalExportLimit);

            OriginalRemaining = jiraIssue.fields.timetracking != null ? TimeSpan.FromSeconds(jiraIssue.fields.timetracking.remainingEstimateSeconds) : new TimeSpan();

            SetRemaining();
        }

        private void UpdateTimer(JiraTimer timer, TimeSpan? exportTime)
        {
            Timer = timer;

            if (exportTime.HasValue && exportTime.Value < timer.TimeToExport)
            {
                ToExportHours = exportTime.Value.Hours;
                ToExportMaxHours = exportTime.Value.Hours;
                ToExportMinutes = exportTime.Value.Minutes;
                optionalExportLimit = exportTime;
            }
            else
            {
                ToExportHours = timer.TimeToExport.Hours;
                ToExportMaxHours = timer.TimeToExport.Hours;
                ToExportMinutes = timer.TimeToExport.Minutes;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ToExportHours"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ToExportMaxHours"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ToExportMinutes"));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasParent"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraParentRef"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraParentDesc"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraRef"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraDesc"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportedHours"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportedMinutes"));
        }

        private void SetRemaining()
        {
            switch (workLogStrategy)
            {
                case WorkLogStrategy.Automatic:
                    var autoNewRemaining = OriginalRemaining.Subtract(ToExport);
                    if (autoNewRemaining.TotalSeconds < 0) autoNewRemaining = new TimeSpan();
                    RemainingHours = autoNewRemaining.Hours;
                    RemainingMinutes = autoNewRemaining.Minutes;
                    break;
                case WorkLogStrategy.LeaveRemaining:
                    RemainingHours = OriginalRemaining.Hours;
                    RemainingMinutes = OriginalRemaining.Minutes;
                    break;
                case WorkLogStrategy.SetValue:
                    RemainingHours = 0;
                    RemainingMinutes = 0;
                    break;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RemainingHours"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RemainingMinutes"));
        }
    }
}
