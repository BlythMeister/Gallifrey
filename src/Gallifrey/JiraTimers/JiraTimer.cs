using System;
using System.Diagnostics;
using Atlassian.Jira;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.ExtensionMethods;
using Gallifrey.IdleTimers;
using Newtonsoft.Json;

namespace Gallifrey.JiraTimers
{
    public class JiraTimer
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
        }

        public JiraTimer(JiraTimer previousTimer, DateTime dateStarted)
        {
            JiraReference = previousTimer.JiraReference;
            JiraProjectName = previousTimer.JiraReference;
            JiraName = previousTimer.JiraReference;
            DateStarted = dateStarted;
            CurrentTime = new TimeSpan();
            ExportedTime = new TimeSpan();
            UniqueId = Guid.NewGuid();
            IsRunning = false;
            currentRunningTime = new Stopwatch();
        }

        public bool FullyExported
        {
            get { return ExportedTime.TotalMinutes >= CurrentTime.TotalMinutes; }
        }

        public TimeSpan TimeToExport
        {
            get { return ExactCurrentTime.Subtract(ExportedTime); }
        }

        public TimeSpan ExactCurrentTime
        {
            get { return CurrentTime.Add(currentRunningTime.Elapsed); }
        }

        public void StartTimer()
        {
            currentRunningTime.Start();
            IsRunning = true;
        }

        public void StopTimer()
        {
            currentRunningTime.Stop();
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
        }

        public override string ToString()
        {
            return TimeToExport.TotalMinutes > 0 ? 
                string.Format("{0} - Time [ {1} ] - To Export [ {2} ] - Desc [ {3} ]", JiraReference, ExactCurrentTime.FormatAsString(), TimeToExport.FormatAsString(), JiraName) :
                string.Format("{0} - Time [ {1} ] - Desc [ {2} ]", JiraReference, ExactCurrentTime.FormatAsString(), JiraName);
        }

        public bool HasExportedTime()
        {
            return ExportedTime.TotalSeconds == 0;
        }
    }
}
