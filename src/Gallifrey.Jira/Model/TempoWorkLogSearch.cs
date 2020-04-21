using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class TempoWorkLogSearch
    {
        public List<TempoWorkLog> results { get; set; }
    }
}
