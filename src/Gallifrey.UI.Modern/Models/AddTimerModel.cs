using Gallifrey.IdleTimers;
using Gallifrey.UI.Modern.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Gallifrey.UI.Modern.Models
{
    public class AddTimerModel : INotifyPropertyChanged
    {
        private bool localTimer;
        private bool datePeriod;
        private int startMinutes;
        private int startHours;
        private DateTime? startDate;
        private DateTime? endDate;
        private bool startNow;
        private bool assignToMe;
        private bool changeStatus;
        private string jiraReference;
        private readonly string jiraUrl;

        public event PropertyChangedEventHandler PropertyChanged;

        public string LocalTimerDescription { get; set; }
        public bool JiraReferenceEditable { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime DisplayDate { get; set; }
        public bool DateEditable { get; set; }
        public bool StartNowEditable { get; set; }
        public List<IdleTimer> IdleTimers { get; set; }

        public bool StartNow
        {
            get => startNow;
            set
            {
                startNow = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartNow)));
            }
        }

        public bool AssignToMe
        {
            get => assignToMe;
            set
            {
                assignToMe = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AssignToMe)));
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

        public bool TimeEditable => IdleTimers == null || IdleTimers.Count == 0;

        public bool LocalTimer
        {
            get => localTimer;
            set
            {
                localTimer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalTimer)));
            }
        }

        public bool DatePeriod
        {
            get => datePeriod;
            set
            {
                datePeriod = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DatePeriod)));
                SetStartNowEnabled();
            }
        }

        public DateTime? StartDate
        {
            get => startDate;
            set
            {
                startDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartDate)));
                if (EndDate < startDate)
                {
                    EndDate = startDate;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EndDate)));
                }

                SetStartNowEnabled();
            }
        }

        public DateTime? EndDate
        {
            get => endDate;
            set
            {
                endDate = value;
                if (EndDate < startDate)
                {
                    endDate = startDate;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EndDate)));
                SetStartNowEnabled();
            }
        }

        public string JiraReference
        {
            get => jiraReference;
            set
            {
                jiraReference = value;

                if (Uri.TryCreate(jiraReference, UriKind.Absolute, out var pastedUri) && Uri.TryCreate(jiraUrl, UriKind.Absolute, out var jiraUri) && pastedUri.Host == jiraUri.Host)
                {
                    var uriDrag = pastedUri.AbsolutePath;
                    jiraReference = uriDrag.Substring(uriDrag.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase) + 1);
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JiraReference)));
            }
        }

        public AddTimerModel(IBackend gallifrey, string jiraRef, DateTime? startDate, bool? enableDateChange, List<IdleTimer> idleTimers, bool? startNow)
        {
            var dateToday = DateTime.Now;

            JiraReference = jiraRef;
            JiraReferenceEditable = string.IsNullOrWhiteSpace(jiraRef);

            jiraUrl = gallifrey.Settings.JiraConnectionSettings.JiraUrl;

            if (gallifrey.Settings.AppSettings.KeepTimersForDays > 0)
            {
                MinDate = dateToday.AddDays(gallifrey.Settings.AppSettings.KeepTimersForDays * -1);
                MaxDate = dateToday.AddDays(gallifrey.Settings.AppSettings.KeepTimersForDays);
            }
            else
            {
                MinDate = dateToday.AddDays(-300);
                MaxDate = dateToday.AddDays(300);
            }

            if (!startDate.HasValue)
            {
                startDate = dateToday;
            }

            if (startDate.Value < MinDate || startDate.Value > MaxDate)
            {
                DisplayDate = dateToday;
                StartDate = dateToday;
                EndDate = dateToday;
            }
            else
            {
                DisplayDate = startDate.Value;
                StartDate = startDate.Value;
                EndDate = startDate.Value;
            }

            DateEditable = !enableDateChange.HasValue || enableDateChange.Value;
            StartNow = startNow.HasValue && startNow.Value;

            if (idleTimers != null && idleTimers.Any())
            {
                var preloadTime = new TimeSpan();
                preloadTime = idleTimers.Aggregate(preloadTime, (current, idleTimer) => current.Add(idleTimer.IdleTimeValue));
                StartHours = preloadTime.Hours > 9 ? 9 : preloadTime.Hours;
                StartMinutes = preloadTime.Minutes;
                IdleTimers = idleTimers;
            }
        }

        public int? StartHours
        {
            get => startHours;
            set
            {
                var newValue = value ?? 0;
                HourMinuteHelper.UpdateHours(ref startHours, newValue, 23);
            }
        }

        public int? StartMinutes
        {
            get => startMinutes;
            set
            {
                var newValue = value ?? 0;
                HourMinuteHelper.UpdateMinutes(ref startHours, ref startMinutes, newValue, 23, out var hoursChanged);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartMinutes)));
                if (hoursChanged)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartHours)));
                }
            }
        }

        public void SetStartNowEnabled()
        {
            var enabled = true;

            if (DatePeriod && StartDate.HasValue && EndDate.HasValue && StartDate.Value.Date != EndDate.Value.Date)
            {
                enabled = false;
            }
            else if (StartDate.HasValue && StartDate.Value.Date != DateTime.Now.Date)
            {
                enabled = false;
            }

            if (enabled)
            {
                StartNowEditable = true;
            }
            else
            {
                StartNow = false;
                StartNowEditable = false;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartNow)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartNowEditable)));
        }

        public void SetJiraReference(string jiraRef)
        {
            JiraReference = jiraRef;
            JiraReferenceEditable = false;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JiraReference)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JiraReferenceEditable)));
        }
    }
}
