using System.Collections.Generic;
using Gallifrey.ChangeLog;

namespace Gallifrey.UI.Modern.Flyouts
{
    public class ChangeLogModel
    {
        public List<ChangeVersion> ChangeLogs { get; set; }

        public ChangeLogModel(IEnumerable<ChangeLogVersion> changeLog)
        {
            ChangeLogs = new List<ChangeVersion>();
            foreach (var changeLogVersion in changeLog)
            {
                ChangeLogs.Add(new ChangeVersion
                {
                    VersionNumber = changeLogVersion.Version.ToString(),
                    VersionName = changeLogVersion.Name,
                    Features = changeLogVersion.Features,
                    Bugs = changeLogVersion.Bugs,
                    Others = changeLogVersion.Others
                });
            }
        }

        public class ChangeVersion
        {
            public string VersionNumber { get; set; }
            public string VersionName { get; set; }
            public bool HaveVersionName { get { return !string.IsNullOrWhiteSpace(VersionName); } }
            public bool IsSelected { get; set; }
            public List<string> Features { get; set; }
            public List<string> Bugs { get; set; }
            public List<string> Others { get; set; }
        }
    }
}