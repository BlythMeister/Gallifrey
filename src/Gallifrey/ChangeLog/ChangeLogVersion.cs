using System;
using System.Collections.Generic;

namespace Gallifrey.ChangeLog
{
    public class ChangeLogVersion
    {
        public Version Version { get; }
        public string Name { get; }
        public bool NewVersion { get; }
        public List<string> Features { get; }
        public List<string> Bugs { get; }
        public List<string> Others { get; }

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
