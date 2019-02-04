using System;
using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class WorkLog
    {
        public double timeSpentSeconds { get; set; }
        public DateTime started { get; set; }
    }
}
