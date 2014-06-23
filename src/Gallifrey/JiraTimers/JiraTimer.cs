using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using Atlassian.Jira;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.ExtensionMethods;
using Gallifrey.IdleTimers;
using Newtonsoft.Json;

namespace Gallifrey.JiraTimers
{
    public class JiraTimer : INotifyPropertyChanged
    {
        public string JiraReference { get; private set; }
        public string JiraProjectName { get; private set; }
        public string JiraName { get; private set; }
        public DateTime DateStarted { get; private set; }
        public TimeSpan CurrentTime { get; private set; }
        public TimeSpan ExportedTime { get; private set; }
        public Guid UniqueId { get; private set; }
        public bool IsRunning { get; private set; }
        private readonly Stopwatch currentRunningTime;
        private readonly Timer runningWatcher;

        [JsonConstructor]
        public JiraTimer(string jiraReference, string jiraProjectName, string jiraName, DateTime dateStarted, TimeSpan currentTime, TimeSpan exportedTime, Guid uniqueId)
        {
            JiraReference = jiraReference;
            JiraProjectName = jiraProjectName;
            JiraName = jiraName;
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
            JiraReference = jiraIssue.Key.Value;
            JiraProjectName = jiraIssue.Project;
            JiraName = jiraIssue.Summary;
            DateStarted = dateStarted;
            CurrentTime = currentTime;
            ExportedTime = new TimeSpan();
            UniqueId = Guid.NewGuid();
            IsRunning = false;
            currentRunningTime = new Stopwatch();
            runningWatcher = new Timer(100);
            runningWatcher.Elapsed += runningWatcherElapsed;
        }

        public JiraTimer(JiraTimer previousTimer, DateTime dateStarted)
        {
            JiraReference = previousTimer.JiraReference;
            JiraProjectName = previousTimer.JiraProjectName;
            JiraName = previousTimer.JiraName;
            DateStarted = dateStarted;
            CurrentTime = new TimeSpan();
            ExportedTime = new TimeSpan();
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
            get { return ExactCurrentTime.Subtract(ExportedTime); }
        }

        public TimeSpan ExactCurrentTime
        {
            get { return CurrentTime.Add(currentRunningTime.Elapsed); }
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

        public void StopTimer()
        {
            currentRunningTime.Stop();
            runningWatcher.Stop();
            IsRunning = false;
            CurrentTime = CurrentTime.Add(currentRunningTime.Elapsed);
            currentRunningTime.Reset();
        }

        public void AddIdleTimer(IdleTimer idleTimer)
        {
            if (idleTimer.IsRunning)
            {
                throw new IdleTimerRunningException("Cannot add time from a running idle timer!");
            }

            CurrentTime = CurrentTime.Add(idleTimer.CurrentTime);
            PropertyChanged(this, new PropertyChangedEventArgs("ExactCurrentTime"));
        }

        public override string ToString()
        {
            return TimeToExport.TotalMinutes >= 1 ?
                string.Format("{0} - Time [ {1} ] - To Export [ {2} ] - Desc [ {3} ]", JiraReference, ExactCurrentTime.FormatAsString(), TimeToExport.FormatAsString(), JiraName) :
                string.Format("{0} - Time [ {1} ] - Desc [ {2} ]", JiraReference, ExactCurrentTime.FormatAsString(), JiraName);
        }

        public bool HasExportedTime()
        {
            return ExportedTime.TotalSeconds == 0;
        }

        public void ManualAdjustment(int hours, int minutes, bool addTime)
        {
            var changeTimespan = new TimeSpan(hours, minutes, 0);
            CurrentTime = addTime ? CurrentTime.Add(changeTimespan) : CurrentTime.Subtract(changeTimespan > ExactCurrentTime ? ExactCurrentTime : changeTimespan);
            PropertyChanged(this, new PropertyChangedEventArgs("ExactCurrentTime"));
        }

        public void SetJiraExportedTime(TimeSpan loggedTime)
        {
            ExportedTime = loggedTime;
            var exportedvsActual = ExportedTime.Subtract(ExactCurrentTime);
            if (exportedvsActual.TotalMinutes >= 1)
            {
                ManualAdjustment(exportedvsActual.Hours, exportedvsActual.Minutes, true);
            }
            PropertyChanged(this, new PropertyChangedEventArgs("ExactCurrentTime"));
        }

        public void AddJiraExportedTime(TimeSpan loggedTime)
        {
            ExportedTime = ExportedTime.Add(loggedTime);
            PropertyChanged(this, new PropertyChangedEventArgs("ExactCurrentTime"));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
