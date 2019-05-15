using System;

namespace Gallifrey.JiraTimers
{
    public class ExportPromptDetail
    {
        public Guid TimerId { get; }
        public TimeSpan ExportTime { get; }

        public ExportPromptDetail(Guid timerId, TimeSpan exportTime)
        {
            TimerId = timerId;
            ExportTime = exportTime;
        }
    }
}
