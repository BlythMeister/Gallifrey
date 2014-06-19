using System.Collections.Generic;

namespace Gallifrey.ChangeLog
{
    public class ChangeLogVersionDetails
    {
        public string Name { get; private set; }
        public List<string> Features { get; private set; }
        public List<string> Bugs { get; private set; }
        public List<string> Others { get; private set; }

        public ChangeLogVersionDetails(string name, List<string> features, List<string> bugs, List<string> others)
        {
            Name = name;
            Features = features;
            Bugs = bugs;
            Others = others;
        }
    }
}
