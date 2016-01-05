using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Gallifrey.ChangeLog
{
    internal static class ChangeLogProvider
    {
        static readonly XNamespace ChangelogNamespace = "http://releases.gallifreyapp.co.uk/ChangeLog";

        public static List<ChangeLogVersion> GetFullChangeLog(XDocument changeLogContent)
        {
            var changeLog = changeLogContent.Descendants(ChangelogNamespace + "Version").Select(x => BuildChangeLogItem(x, false));
            return changeLog.OrderByDescending(detail => detail.Version).ToList();
        }

        public static List<ChangeLogVersion> GetChangeLog(Version fromVersion, XDocument changeLogContent)
        {
            var changeLog = changeLogContent.Descendants(ChangelogNamespace + "Version").Select(x => BuildChangeLogItem(x, fromVersion));
            return changeLog.OrderByDescending(detail => detail.Version).ToList();
        }

        private static ChangeLogVersion BuildChangeLogItem(XElement changeVersion, Version fromVersion)
        {
            var fromCompareVersion = new Version(fromVersion.Major, fromVersion.Minor, fromVersion.Build);
            var version = new Version(changeVersion.Attribute("Number").Value);
            var newVersion = false;
            
            var compareVersion = version;
            if (version.Revision > 0)
            {
                compareVersion = new Version(version.Major, version.Minor, version.Build);
            }

            if (compareVersion > fromCompareVersion)
            {
                newVersion = true;
            }

            return BuildChangeLogItem(changeVersion, newVersion);
        }

        private static ChangeLogVersion BuildChangeLogItem(XElement changeVersion, bool newVersion)
        {
            var version = new Version(changeVersion.Attribute("Number").Value);

            var nameAttr = changeVersion.Attribute("Name");
            var name = string.Empty;
            if (nameAttr != null)
            {
                name = nameAttr.Value;
            }

            var features = changeVersion.Descendants(ChangelogNamespace + "Feature").Select(item => item.Value).ToList();
            var bugs = changeVersion.Descendants(ChangelogNamespace + "Bug").Select(item => item.Value).ToList();
            var others = changeVersion.Descendants(ChangelogNamespace + "Other").Select(item => item.Value).ToList();

            return new ChangeLogVersion(version, name, newVersion, features, bugs, others);
        }
    }
}
