using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Error
    {
        public List<string> errorMessages { get; set; }
    }
}