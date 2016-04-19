using System;
using System.Collections.Generic;

namespace Gallifrey.ChangeLog
{
    public class ChangeLogVersion
    {
        public Version Version { get; private set; }
        public string Name { get; private set; }
        public bool NewVersion { get; private set; }
        public List<string> Features { get; private set; }
        public List<string> Bugs { get; private set; }
        public List<string> Others { get; private set; }

        public ChangeLogVersion(Version version, string name, bool newVersion, List<string> features, List<string> bugs, List<string> others)
        {
            Version = version;
            Name = name;
            NewVersion = newVersion;
            Features = features;
            Bugs = bugs;
            Others = others;
        }
    }
}
