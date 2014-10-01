using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.ExtensionMethods;
using Gallifrey.IdleTimers;
using Gallifrey.Jira;
using Gallifrey.Jira.Model;
using Newtonsoft.Json;

namespace Gallifrey.JiraTimers
{
    public class JiraTimer : INotifyPropertyChanged
    {
        public string JiraReference { get; private set; }
        public string JiraProjectName { get; private set; }
        public string JiraName { get; private set; }
        public string JiraParentReference { get; private set; }
        public string JiraParentName { get; private set; }
        public DateTime DateStarted { get; private set; }
        public TimeSpan CurrentTime { get; private set; }
        public TimeSpan ExportedTime { get; private set; }
        public Guid UniqueId { get; private set; }
        public bool IsRunning { get; private set; }
        private readonly Stopwatch currentRunningTime;
        private readonly Timer runningWatcher;

        [JsonConstructor]
        public JiraTimer(string jiraReference, string jiraProjectName, string jiraName, DateTime dateStarted, TimeSpan currentTime, TimeSpan exportedTime, Guid uniqueId, string jiraParentReference, string jiraParentName)
        {
            JiraReference = jiraReference;
            JiraProjectName = jiraProjectName;
            JiraName = jiraName;
            JiraParentReference = jiraParentReference;
            JiraParentName = jiraParentName;
            DateStarted = dateStarted;
            CurrentTime = currentTime;
            ExportedTime = exportedTime;
            UniqueId = uniqueId;
            IsRunning = false;
            currentRunningTime = new Stopwatch();
            runningWatcher = new Timer(100);
            runningWatcher.Elapsed += runningWatcherElapsed;
        }

        public JiraTimer(Issue jiraIssue, DateTime dateStarted, TimeSpan currentTime)
        {
            JiraReference = jiraIssue.key;
            JiraProjectName = jiraIssue.fields.project.key;
            JiraName = jiraIssue.fields.summary;
            if (jiraIssue.fields.parent != null)
            {
                JiraParentReference = jiraIssue.fields.parent.key;
                JiraParentName = jiraIssue.fields.parent.fields.summary;
            }
            DateStarted = dateStarted;
            CurrentTime = currentTime;
            ExportedTime = new TimeSpan();
            UniqueId = Guid.NewGuid();
            IsRunning = false;
            currentRunningTime = new Stopwatch();
            runningWatcher = new Timer(100);
            runningWatcher.Elapsed += runningWatcherElapsed;
        }

        public JiraTimer(JiraTimer previousTimer, DateTime dateStarted, bool resetTimes)
        {
            JiraReference = previousTimer.JiraReference;
            JiraProjectName = previousTimer.JiraProjectName;
            JiraName = previousTimer.JiraName;
            JiraParentReference = previousTimer.JiraParentReference;
            JiraParentName = previousTimer.JiraParentName;
            DateStarted = dateStarted;
            CurrentTime = resetTimes ? new TimeSpan() : previousTimer.CurrentTime;
            ExportedTime = resetTimes ? new TimeSpan() : previousTimer.ExportedTime;
            UniqueId = Guid.NewGuid();
            IsRunning = false;
            currentRunningTime = new Stopwatch();
            runningWatcher = new Timer(100);
            runningWatcher.Elapsed += runningWatcherElapsed;
        }

        void runningWatcherElapsed(object sender, ElapsedEventArgs e)
        {
            if (PropertyChanged != null && currentRunningTime.IsRunning)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ExactCurrentTime"));
            }
        }

        public bool FullyExported
        {
            get { return TimeToExport.TotalMinutes < 1; }
        }

        public TimeSpan TimeToExport
        {
            get
            {
                var timeToExport = ExactCurrentTime.Subtract(ExportedTime);
                return timeToExport.TotalSeconds > 0 ? timeToExport : new TimeSpan();
            }
        }

        public TimeSpan ExactCurrentTime
        {
            get { return CurrentTime.Add(currentRunningTime.Elapsed); }
        }

        public bool HasParent
        {
            get { return !string.IsNullOrWhiteSpace(JiraParentReference); }
        }

        public bool IsThisWeek(DayOfWeek startOfWeek)
        {
            var today = DateTime.Today;
            var daysThisWeek = (7 + (today.DayOfWeek - startOfWeek)) % 7;

            var weekStartDate = today.AddDays(daysThisWeek * -1);
            var weekEndDate = weekStartDate.AddDays(7).AddSeconds(-1);

            return DateStarted.Date >= weekStartDate.Date && DateStarted.Date < weekEndDate.Date;
        }

        public void StartTimer()
        {
            runningWatcher.Start();
            currentRunningTime.Start();
            IsRunning = true;
        }

        public TimeSpan StopTimer()
        {
            currentRunningTime.Stop();
            runningWatcher.Stop();
            IsRunning = false;
            var elapsed = currentRunningTime.Elapsed;
            CurrentTime = CurrentTime.Add(elapsed);
            currentRunningTime.Reset();
            return elapsed;
        }

        public void AddIdleTimer(IdleTimer idleTimer)
        {
            if (idleTimer.IsRunning)
            {
                throw new IdleTimerRunningException("Cannot add time from a running idle timer!");
            }

            CurrentTime = CurrentTime.Add(idleTimer.IdleTimeValue);
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ExactCurrentTime"));
        }

        public override string ToString()
        {
            string description;

            if (TimeToExport.TotalMinutes >= 1)
            {
                if (HasParent)
                {
                    description = string.Format("{0} - [ {1} ] - Export [ {2} ] - [ {4} {5} / {0} {3} ]", JiraReference, ExactCurrentTime.FormatAsString(), TimeToExport.FormatAsString(), JiraName, JiraParentReference, JiraParentName); 
                }
                else
                {
                    description = string.Format("{0} - [ {1} ] - Export [ {2} ] - [ {0} - {3} ]", JiraReference, ExactCurrentTime.FormatAsString(), TimeToExport.FormatAsString(), JiraName);
                }
            }
            else
            {
                if (HasParent)
                {
                    description = string.Format("{0} - [ {1} ] - [ {3} {4} / {0} {2} ]", JiraReference, ExactCurrentTime.FormatAsString(), JiraName, JiraParentReference, JiraParentName); 
                }
                else
                {
                    description = string.Format("{0} - [ {1} ] - [ {0} {2} ]", JiraReference, ExactCurrentTime.FormatAsString(), JiraName);
                }
            }

            return description;
        }

        public bool HasExportedTime()
        {
            return ExportedTime.TotalSeconds == 0;
        }

        public bool ManualAdjustment(TimeSpan changeTimespan, bool addTime)
        {
            var calculatedNewTime = addTime ? CurrentTime.Add(changeTimespan) : CurrentTime.Subtract(changeTimespan > ExactCurrentTime ? ExactCurrentTime : changeTimespan);
            var returnValue = true;
            if (!addTime && ExportedTime > calculatedNewTime)
            {
                returnValue = false;
                calculatedNewTime = ExportedTime;
            }

            CurrentTime = calculatedNewTime;
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ExactCurrentTime"));
            return returnValue;
        }

        public void SetJiraExportedTime(TimeSpan loggedTime)
        {
            ExportedTime = loggedTime;
            var exportedvsActual = ExportedTime.Subtract(ExactCurrentTime);
            if (exportedvsActual.TotalMinutes >= 1)
            {
                ManualAdjustment(exportedvsActual, true);
            }
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ExactCurrentTime"));
        }

        public void AddJiraExportedTime(TimeSpan loggedTime)
        {
            ExportedTime = ExportedTime.Add(loggedTime);
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ExactCurrentTime"));
        }

        public void RefreshFromJira(Issue jiraIssue, string currentUserName)
        {
            if (jiraIssue == null) return;

            ExportedTime = jiraIssue.GetCurrentLoggedTimeForDate(DateStarted, currentUserName);
            JiraReference = jiraIssue.key;
            JiraProjectName = jiraIssue.fields.project.key;
            JiraName = jiraIssue.fields.summary;
            if (jiraIssue.fields.parent != null)
            {
                JiraParentReference = jiraIssue.fields.parent.key;
                JiraParentName = jiraIssue.fields.parent.fields.summary;
            }
            else
            {
                JiraParentReference = string.Empty;
                JiraParentName = string.Empty;
            }

            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ExactCurrentTime"));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
