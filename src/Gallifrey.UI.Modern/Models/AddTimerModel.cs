using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gallifrey.IdleTimers;

namespace Gallifrey.UI.Modern.Models
{
    public class AddTimerModel : INotifyPropertyChanged
    {
        private bool tempTimer;
        private int startMinutes;
        private int startHours;

        public event PropertyChangedEventHandler PropertyChanged;
        public string JiraReference { get; set; }
        public string TempTimerDescription { get; set; }
        public bool JiraReferenceEditable { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime DisplayDate { get; set; }
        public bool DateEditable { get; set; }
        public bool StartNow { get; set; }
        public bool StartNowEditable { get; set; }
        public bool AssignToMe { get; set; }
        public bool ChangeStatus { get; set; }

        public List<IdleTimer> IdleTimers { get; set; }

        public bool TimeEditable => IdleTimers == null || IdleTimers.Count == 0;

        public bool TempTimer
        {
            get { return tempTimer; }
            set
            {
                tempTimer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TempTimer"));
            }
        }

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
            get { return startHours; }
            set
            {
                startHours = value ?? 0;
                if (startHours < 0)
                {
                    startHours = 0;
                }
                if (startHours > 9)
                {
                    startHours = 9;
                }
            }
        }

        public int? StartMinutes
        {
            get { return startMinutes; }
            set
            {
                var newValue = value ?? 0;

                if (newValue < 0)
                {
                    if (startHours == 0)
                    {
                        startMinutes = 0;
                    }
                    else
                    {
                        startMinutes = 60 + newValue;
                        startHours--;
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StartHours"));
                }
                else if (value >= 60)
                {
                    if (startHours == 9)
                    {
                        startMinutes = 59;
                    }
                    else
                    {
                        startHours++;
                        startMinutes = newValue - 60;
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StartHours"));
                }
                else
                {
                    startMinutes = newValue;
                }
            }
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