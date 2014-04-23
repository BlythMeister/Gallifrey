using System;

namespace Gallifrey.Model
{
    public class JiraTimer
    {
        public string JiraReference { get; private set; }
        public string JiraProjectName { get; private set; }
        public string JiraName { get; private set; }
        public TimeSpan CurrentTime { get; private set; }
        public TimeSpan ExportedTime { get; private set; }

        public JiraTimer(string jiraReference, string jiraProjectName, string jiraName, TimeSpan currentTime, TimeSpan exportedTime)
        {
            JiraReference = jiraReference;
            JiraProjectName = jiraProjectName;
            JiraName = jiraName;
            CurrentTime = currentTime;
            ExportedTime = exportedTime;
        }

        public bool FullyExported
        {
            get { return ExportedTime.TotalMinutes >= CurrentTime.TotalMinutes; }
        }
    }
}
