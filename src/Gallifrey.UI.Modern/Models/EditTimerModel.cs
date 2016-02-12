using System;
using System.ComponentModel;

namespace Gallifrey.UI.Modern.Models
{
    public class EditTimerModel : INotifyPropertyChanged
    {
        private bool tempTimer;
        private string jiraReference;
        private string tempTimerDescription;
        private DateTime? runDate;
        private int hours;
        private int minutes;

        public event PropertyChangedEventHandler PropertyChanged;
        public bool JiraReferenceEditable { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime DisplayDate { get; set; }
        public bool DateEditable { get; set; }
        public bool TimeEditable { get; set; }
        public string OriginalJiraReference { get; set; }
        public string OriginalTempTimerDescription { get; set; }
        public DateTime? OriginalRunDate { get; set; }
        public int OriginalHours { get; set; }
        public int OriginalMinutes { get; set; }
        public bool IsDefaultOnButton { get; set; }

        public bool HasModifiedJiraReference => (OriginalJiraReference != JiraReference) || (OriginalTempTimerDescription != TempTimerDescription);
        public bool HasModifiedRunDate => OriginalRunDate != RunDate;
        public bool HasModifiedTime => OriginalHours != Hours || OriginalMinutes != Minutes;

        public EditTimerModel(IBackend gallifrey, Guid timerId)
        {
            var dateToday = DateTime.Now;
            var timer = gallifrey.JiraTimerCollection.GetTimer(timerId);

            TempTimer = timer.TempTimer;
            JiraReference = timer.JiraReference;
            
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

            DateEditable = timer.HasExportedTime();
            JiraReferenceEditable = timer.HasExportedTime();
            TimeEditable = !timer.IsRunning;

            if (TempTimer)
            {
                TempTimerDescription = timer.JiraName;               
            }

            OriginalTempTimerDescription = TempTimerDescription;
            OriginalJiraReference = JiraReference;
            OriginalRunDate = RunDate;
            OriginalHours = Hours;
            OriginalMinutes = Minutes;

            IsDefaultOnButton = true;
        }

        public string JiraReference
        {
            get { return jiraReference; }
            set
            {
                jiraReference = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedJiraReference"));
            }
        }

        public string TempTimerDescription
        {
            get { return tempTimerDescription; }
            set
            {
                tempTimerDescription = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedJiraReference"));
            }
        }

        public DateTime? RunDate
        {
            get { return runDate; }
            set
            {
                runDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedRunDate"));
            }
        }

        public int Hours
        {
            get { return hours; }
            set
            {
                hours = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedTime"));
            }
        }

        public int Minutes
        {
            get { return minutes; }
            set
            {
                minutes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedTime"));
            }
        }

        public bool TempTimer
        {
            get { return tempTimer; }
            set
            {
                tempTimer = value;
                TempTimerDescription = OriginalTempTimerDescription;
                JiraReference = OriginalJiraReference;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TempTimer"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TempTimerDescription"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraReference"));
            }
        }

        public void SetNotDefaultButton()
        {
            IsDefaultOnButton = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDefaultOnButton"));
        }
        
        public void AdjustTime(TimeSpan timeAdjustmentAmount, bool addTime)
        {
            var currentTime = new TimeSpan(Hours, Minutes, 0);

            currentTime = addTime ? currentTime.Add(timeAdjustmentAmount) : currentTime.Subtract(timeAdjustmentAmount);

            Hours = currentTime.Hours > 9 ? 9 : currentTime.Hours;
            Minutes = currentTime.Minutes;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hours"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Minutes"));
        }
    }
}