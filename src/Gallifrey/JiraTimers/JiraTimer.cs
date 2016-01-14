using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.ExtensionMethods;
using Gallifrey.IdleTimers;
using Gallifrey.Jira.Model;
using Newtonsoft.Json;

namespace Gallifrey.JiraTimers
{
    public class JiraTimer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
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
        public DateTime? LastJiraTimeCheck { get; private set; }
        private readonly Stopwatch currentRunningTime;
        private readonly Timer runningWatcher;

        public bool FullyExported => TimeToExport.TotalMinutes < 1;
        public TimeSpan ExactCurrentTime => CurrentTime.Add(currentRunningTime.Elapsed);
        public bool HasParent => !string.IsNullOrWhiteSpace(JiraParentReference);

        [JsonConstructor]
        public JiraTimer(string jiraReference, string jiraProjectName, string jiraName, DateTime dateStarted, TimeSpan currentTime, TimeSpan exportedTime, Guid uniqueId, string jiraParentReference, string jiraParentName, DateTime? lastJiraTimeCheck)
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
            LastJiraTimeCheck = lastJiraTimeCheck;
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
            LastJiraTimeCheck = null;
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
            LastJiraTimeCheck = null;
        }

        void runningWatcherElapsed(object sender, ElapsedEventArgs e)
        {
            if (currentRunningTime.IsRunning)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExactCurrentTime"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimeToExport"));
            }
        }

        public TimeSpan TimeToExport
        {
            get
            {
                var timeToExport = ExactCurrentTime.Subtract(ExportedTime);
                return timeToExport.TotalSeconds > 0 ? timeToExport : new TimeSpan();
            }
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

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
        }

        public TimeSpan StopTimer()
        {
            currentRunningTime.Stop();
            runningWatcher.Stop();
            IsRunning = false;
            var elapsed = currentRunningTime.Elapsed;
            CurrentTime = CurrentTime.Add(elapsed);
            currentRunningTime.Reset();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));

            return elapsed;
        }

        public void AddIdleTimer(IdleTimer idleTimer)
        {
            if (idleTimer.IsRunning)
            {
                throw new IdleTimerRunningException("Cannot add time from a running idle timer!");
            }

            CurrentTime = CurrentTime.Add(idleTimer.IdleTimeValue);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExactCurrentTime"));
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
            var shouldRestart = false;
            if (IsRunning)
            {
                StopTimer();
                shouldRestart = true;
            }
            var calculatedNewTime = addTime ? CurrentTime.Add(changeTimespan) : CurrentTime.Subtract(changeTimespan > CurrentTime ? CurrentTime : changeTimespan);
            var returnValue = true;
            if (!addTime && ExportedTime > calculatedNewTime)
            {
                returnValue = false;
                calculatedNewTime = ExportedTime;
            }

            CurrentTime = calculatedNewTime;

            if (shouldRestart)
            {
                StartTimer();
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExactCurrentTime"));
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExactCurrentTime"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimeToExport"));
        }

        public void AddJiraExportedTime(TimeSpan loggedTime)
        {
            ExportedTime = ExportedTime.Add(loggedTime);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExactCurrentTime"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimeToExport"));
        }

        public void RefreshFromJira(Issue jiraIssue, User currentUser)
        {
            if (jiraIssue == null) return;

            SetJiraExportedTime(jiraIssue.GetCurrentLoggedTimeForDate(DateStarted, currentUser));
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

            LastJiraTimeCheck = DateTime.UtcNow;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExactCurrentTime"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimeToExport"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraReference"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraProjectName"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraName"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraParentReference"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraParentName"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasParent"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LastJiraTimeCheck"));
        }
    }
}
