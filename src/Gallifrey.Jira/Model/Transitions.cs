using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Transitions
    {
        public List<Status> transitions { get; set; }
    }
}