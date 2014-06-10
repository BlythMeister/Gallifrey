using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gallifrey.ChangeLog
{
    public class ChangeLogVersionDetails
    {
        public List<string> Features { get; private set; }
        public List<string> Bugs { get; private set; }
        public List<string> Others { get; private set; }

        public ChangeLogVersionDetails(List<string> features,List<string> bugs,List<string> others)
        {
            Features = features;
            Bugs = bugs;
            Others = others;
        }
    }
}
