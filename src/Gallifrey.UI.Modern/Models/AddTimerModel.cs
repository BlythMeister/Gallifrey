using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gallifrey.IdleTimers;

namespace Gallifrey.UI.Modern.Models
{
    public class AddTimerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string JiraReference { get; set; }
        public bool JiraReferenceEditable { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime DisplayDate { get; set; }
        public bool DateEditable { get; set; }
        public bool TimeEditable { get; set; }
        public int StartHours { get; set; }
        public int StartMinutes { get; set; }
        public bool StartNow { get; set; }
        public bool StartNowEditable { get; set; }
        public bool AssignToMe { get; set; }
        public bool InProgress { get; set; }
        public List<IdleTimer> IdleTimers { get; set; }

        public AddTimerModel(IBackend gallifrey, string jiraRef, DateTime? startDate, bool? enableDateChange, List<IdleTimer> idleTimers, bool? startNow)
        {
            var dateToday = DateTime.Now;

            JiraReference = jiraRef;
            JiraReferenceEditable = string.IsNullOrWhiteSpace(jiraRef);

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

            if (!startDate.HasValue) startDate = dateToday;

            if (startDate.Value < MinDate || startDate.Value > MaxDate)
            {
                DisplayDate = dateToday;
                StartDate = dateToday;
            }
            else
            {
                DisplayDate = startDate.Value;
                StartDate = startDate.Value;
            }

            DateEditable = !enableDateChange.HasValue || enableDateChange.Value;
            TimeEditable = true;

            if (idleTimers != null && idleTimers.Any())
            {
                var preloadTime = new TimeSpan();
                preloadTime = idleTimers.Aggregate(preloadTime, (current, idleTimer) => current.Add(idleTimer.IdleTimeValue));
                StartHours = preloadTime.Hours > 9 ? 9 : preloadTime.Hours;
                StartMinutes = preloadTime.Minutes;
                IdleTimers = idleTimers;
                TimeEditable = false;
            }

            StartNow = startNow.HasValue && startNow.Value;
        }

        public void SetStartNowEnabled(bool enabled)
        {
            if (enabled)
            {
                StartNowEditable = true;
            }
            else
            {
                StartNow = false;
                StartNowEditable = false;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StartNow"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StartNowEditable"));
        }

        public void SetJiraReference(string jiraRef)
        {
            JiraReference = jiraRef;
            JiraReferenceEditable = false;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraReference"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraReferenceEditable"));
        }
    }
}