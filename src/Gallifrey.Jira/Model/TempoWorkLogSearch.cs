using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.Jira.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class TempoWorkLogSearch
    {
        public List<TempoWorkLog> results { get; set; }
        public TempoWorkLogSearchMeta metadata { get; set; }

        public class TempoWorkLogSearchMeta
        {
            public int count { get; set; }
            public int offset { get; set; }
            public int limit { get; set; }
        }
    }
}
