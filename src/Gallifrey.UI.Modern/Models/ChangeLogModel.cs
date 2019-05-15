using Gallifrey.ChangeLog;
using System.Collections.Generic;
using System.Linq;

namespace Gallifrey.UI.Modern.Models
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

            if (ChangeLogs.Any())
            {
                ChangeLogs[0].IsSelected = true;
            }
        }

        public class ChangeVersion
        {
            public string VersionNumber { get; set; }
            public string VersionName { get; set; }
            public bool IsSelected { get; set; }

            public List<string> Features { get; set; }
            public List<string> Bugs { get; set; }
            public List<string> Others { get; set; }

            public bool HaveVersionName => !string.IsNullOrWhiteSpace(VersionName);
            public bool HasFeatures => Features != null && Features.Any();
            public bool HasBugs => Bugs != null && Bugs.Any();
            public bool HasOthers => Others != null && Others.Any();
        }
    }
}
