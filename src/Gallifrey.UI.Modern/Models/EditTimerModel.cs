using System;
using System.ComponentModel;
using Gallifrey.UI.Modern.Helpers;

namespace Gallifrey.UI.Modern.Models
{
    public class EditTimerModel : INotifyPropertyChanged
    {
        private readonly bool hasExportedTime;

        private bool localTimer;
        private bool jiraRefFromSearch;
        private string jiraReference;
        private string localTimerDescription;
        private DateTime? runDate;
        private int hours;
        private int minutes;

        public event PropertyChangedEventHandler PropertyChanged;
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime DisplayDate { get; set; }
        public bool TimeEditable { get; set; }
        public string OriginalJiraReference { get; set; }
        public string OriginalLocalTimerDescription { get; set; }
        public DateTime? OriginalRunDate { get; set; }
        public int OriginalHours { get; set; }
        public int OriginalMinutes { get; set; }
        public bool IsDefaultOnButton { get; set; }

        public bool DateEditable => !hasExportedTime && !HasModifiedJiraReference;
        public bool JiraReferenceEditable => !hasExportedTime && !HasModifiedRunDate && !jiraRefFromSearch;
        public bool HasModifiedJiraReference => (OriginalJiraReference != JiraReference) || (OriginalLocalTimerDescription != LocalTimerDescription);
        public bool HasModifiedRunDate => OriginalRunDate.Value.Date != RunDate.Value.Date;
        public bool HasModifiedTime => OriginalHours != Hours || OriginalMinutes != Minutes;
        

        public EditTimerModel(IBackend gallifrey, Guid timerId)
        {
            var dateToday = DateTime.Now;
            var timer = gallifrey.JiraTimerCollection.GetTimer(timerId);

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

            RunDate = timer.DateStarted;
            DisplayDate = timer.DateStarted;

            Hours = timer.ExactCurrentTime.Hours > 9 ? 9 : timer.ExactCurrentTime.Hours;
            Minutes = timer.ExactCurrentTime.Minutes;

            hasExportedTime = timer.HasExportedTime();
            TimeEditable = !timer.IsRunning;

            LocalTimer = timer.LocalTimer;

            if (LocalTimer)
            {
                LocalTimerDescription = timer.JiraName;
                JiraReference = string.Empty;
            }
            else
            {
                JiraReference = timer.JiraReference;
                LocalTimerDescription = string.Empty;
            }

            OriginalLocalTimerDescription = LocalTimerDescription;
            OriginalJiraReference = JiraReference;
            OriginalRunDate = RunDate;
            OriginalHours = Hours ?? 0;
            OriginalMinutes = Minutes ?? 0;

            IsDefaultOnButton = true;
        }

        public string JiraReference
        {
            get { return jiraReference; }
            set
            {
                jiraReference = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DateEditable"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraReferenceEditable"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedJiraReference"));
            }
        }

        public string LocalTimerDescription
        {
            get { return localTimerDescription; }
            set
            {
                localTimerDescription = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DateEditable"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraReferenceEditable"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedJiraReference"));
            }
        }

        public DateTime? RunDate
        {
            get { return runDate; }
            set
            {
                runDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DateEditable"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraReferenceEditable"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedRunDate"));
            }
        }

        public int? Hours
        {
            get { return hours; }
            set
            {
                var newValue = value ?? 0;
                HourMinuteHelper.UpdateHours(ref hours, newValue, 9);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedTime"));
            }
        }

        public int? Minutes
        {
            get { return minutes; }
            set
            {
                var newValue = value ?? 0;
                bool hoursChanged;
                HourMinuteHelper.UpdateMinutes(ref hours, ref minutes, newValue, 9, out hoursChanged);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Minutes"));
                if (hoursChanged)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hours"));
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedTime"));
            }
        }

        public bool LocalTimer
        {
            get { return localTimer; }
            set
            {
                localTimer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LocalTimer"));
            }
        }

        public void SetNotDefaultButton()
        {
            IsDefaultOnButton = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDefaultOnButton"));
        }
        
        public void AdjustTime(TimeSpan timeAdjustmentAmount, bool addTime)
        {
            var currentTime = new TimeSpan(Hours ?? 0, Minutes ?? 0, 0);

            currentTime = addTime ? currentTime.Add(timeAdjustmentAmount) : currentTime.Subtract(timeAdjustmentAmount);

            if (currentTime.TotalMinutes > 599)
            {
                Hours = 9;
                Minutes = 59;
            }
            else if (currentTime.TotalMinutes < 0)
            {
                Hours = 0;
                Minutes = 0;
            }
            else
            {
                Hours = currentTime.Hours;
                Minutes = currentTime.Minutes;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hours"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Minutes"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedTime"));
        }

        public void SetJiraReference(string jiraRef)
        {
            JiraReference = jiraRef;
            jiraRefFromSearch = true;
            LocalTimer = false;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraReference"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraReferenceEditable"));
        }
    }
}