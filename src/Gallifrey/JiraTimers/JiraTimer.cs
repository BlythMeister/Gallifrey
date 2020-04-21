using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.IdleTimers;
using Gallifrey.Jira.Model;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;

namespace Gallifrey.JiraTimers
{
    public class JiraTimer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Guid UniqueId { get; }
        public string JiraReference { get; private set; }
        public string JiraProjectName { get; private set; }
        public string JiraName { get; private set; }
        public string JiraParentReference { get; private set; }
        public string JiraParentName { get; private set; }
        public DateTime DateStarted { get; }
        public TimeSpan CurrentTime { get; private set; }
        public TimeSpan ExportedTime { get; private set; }
        public bool IsRunning { get; private set; }
        public DateTime? LastJiraTimeCheck { get; private set; }
        public bool LocalTimer { get; private set; }
        private readonly Stopwatch currentRunningTime;
        private readonly Timer runningWatcher;

        public bool FullyExported => TimeToExport.TotalMinutes < 1;
        public TimeSpan ExactCurrentTime => CurrentTime.Add(currentRunningTime.Elapsed);
        public bool HasParent => !string.IsNullOrWhiteSpace(JiraParentReference);

        [JsonConstructor]
        public JiraTimer(string jiraReference, string jiraProjectName, string jiraName, DateTime dateStarted, TimeSpan currentTime, TimeSpan exportedTime, Guid uniqueId, string jiraParentReference, string jiraParentName, DateTime? lastJiraTimeCheck, bool localTimer)
        {
            JiraReference = jiraReference;
            JiraProjectName = jiraProjectName;
            JiraName = jiraName;
            JiraParentReference = jiraParentReference;
            JiraParentName = jiraParentName;
            DateStarted = dateStarted.Date;
            CurrentTime = currentTime;
            ExportedTime = exportedTime;
            UniqueId = uniqueId;
            IsRunning = false;
            currentRunningTime = new Stopwatch();
            runningWatcher = new Timer(100);
            runningWatcher.Elapsed += RunningWatcherElapsed;
            LastJiraTimeCheck = lastJiraTimeCheck;
            LocalTimer = localTimer;
        }

        public JiraTimer(int localTimerNumber, string localTimerDescription, DateTime dateStarted, TimeSpan currentTime) :
            this($"GALLIFREY-{localTimerNumber}", "Local Timers", localTimerDescription, dateStarted, currentTime, new TimeSpan(), Guid.NewGuid(), string.Empty, string.Empty, null, true)
        {
        }

        public JiraTimer(Issue jiraIssue, DateTime dateStarted, TimeSpan currentTime) :
            this(jiraIssue.key, jiraIssue.fields.project.key, jiraIssue.fields.summary, dateStarted, currentTime, new TimeSpan(), Guid.NewGuid(), jiraIssue.fields.parent == null ? string.Empty : jiraIssue.fields.parent.key, jiraIssue.fields.parent == null ? string.Empty : jiraIssue.fields.parent.fields.summary, null, false)
        {
        }

        public JiraTimer(JiraTimer previousTimer, DateTime dateStarted, bool resetTimes) :
            this(previousTimer.JiraReference, previousTimer.JiraProjectName, previousTimer.JiraName, dateStarted, resetTimes ? new TimeSpan() : previousTimer.CurrentTime, resetTimes ? new TimeSpan() : previousTimer.ExportedTime, Guid.NewGuid(), previousTimer.JiraParentReference, previousTimer.JiraParentName, null, previousTimer.LocalTimer)
        {
        }

        private void RunningWatcherElapsed(object sender, ElapsedEventArgs e)
        {
            if (currentRunningTime.IsRunning)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExactCurrentTime)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeToExport)));
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

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
        }

        public TimeSpan StopTimer()
        {
            currentRunningTime.Stop();
            runningWatcher.Stop();
            IsRunning = false;
            var elapsed = currentRunningTime.Elapsed;
            CurrentTime = CurrentTime.Add(elapsed);
            currentRunningTime.Reset();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));

            return elapsed;
        }

        public void AddIdleTimer(IdleTimer idleTimer)
        {
            if (idleTimer.IsRunning)
            {
                throw new IdleTimerRunningException("Cannot add time from a running idle timer!");
            }

            CurrentTime = CurrentTime.Add(idleTimer.IdleTimeValue);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExactCurrentTime)));
        }

        public bool HasExportedTime()
        {
            return ExportedTime.TotalSeconds > 0;
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

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExactCurrentTime)));
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

            LastJiraTimeCheck = DateTime.UtcNow;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExactCurrentTime)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeToExport)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastJiraTimeCheck)));
        }

        public void AddJiraExportedTime(TimeSpan loggedTime)
        {
            ExportedTime = ExportedTime.Add(loggedTime);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExactCurrentTime)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeToExport)));
        }

        public void RefreshFromJira(Issue jiraIssue)
        {
            if (jiraIssue == null) return;

            LocalTimer = false;

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

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JiraReference)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JiraProjectName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JiraName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JiraParentReference)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JiraParentName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasParent)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastJiraTimeCheck)));
        }

        public void UpdateLocalTimerDescription(string localTimerDescription)
        {
            if (!LocalTimer) return;
            JiraName = localTimerDescription;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JiraName)));
        }

        public void ClearLastJiraCheck()
        {
            LastJiraTimeCheck = null;
        }
    }
}
