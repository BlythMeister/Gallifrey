using System;

namespace Gallifrey.JiraTimers
{
    public class ExportPromptDetail
    {
        public Guid TimerId { get; private set; }
        public TimeSpan ExportTime { get; private set; }

        public ExportPromptDetail(Guid timerId, TimeSpan exportTime)
        {
            TimerId = timerId;
            ExportTime = exportTime;
        }
    }
}