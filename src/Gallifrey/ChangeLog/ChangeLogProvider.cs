using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Gallifrey.ChangeLog
{
    internal static class ChangeLogProvider
    {
        private static readonly XNamespace ChangelogNamespace = "https://releases.gallifreyapp.co.uk/ChangeLog";

        public static List<ChangeLogVersion> GetChangeLog(Version fromVersion, XDocument changeLogContent)
        {
            var changeLog = changeLogContent.Descendants(ChangelogNamespace + "Version").Select(x => BuildChangeLogItem(x, fromVersion));
            return changeLog.OrderByDescending(detail => detail.Version).ToList();
        }

        private static ChangeLogVersion BuildChangeLogItem(XElement changeVersion, Version fromVersion)
        {
            var versionAttribute = changeVersion.Attribute("Number");
            if (versionAttribute == null)
            {
                return null;
            }
            var version = new Version(versionAttribute.Value);
            var newVersion = version > fromVersion;
            return BuildChangeLogItem(changeVersion, newVersion);
        }

        private static ChangeLogVersion BuildChangeLogItem(XElement changeVersion, bool newVersion)
        {
            var versionAttribute = changeVersion.Attribute("Number");
            if (versionAttribute == null)
            {
                return null;
            }
            var version = new Version(versionAttribute.Value);

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
