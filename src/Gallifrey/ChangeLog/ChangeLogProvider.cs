using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Gallifrey.ChangeLog
{
    internal static class ChangeLogProvider
    {
        public static List<ChangeLogVersion> GetFullChangeLog(XDocument changeLogContent)
        {
            var changeLog = changeLogContent.Descendants("Version").Select(BuildChangeLogItem);
            return changeLog.OrderByDescending(detail => detail.Version).ToList();
        }

        public static List<ChangeLogVersion> GetChangeLog(Version fromVersion, Version toVersion, XDocument changeLogContent)
        {
            var changeLog = changeLogContent.Descendants("Version").Select(x => BuildChangeLogItem(x, fromVersion, toVersion));
            return changeLog.OrderByDescending(detail => detail.Version).ToList();
        }

        private static ChangeLogVersion BuildChangeLogItem(XElement changeVersion, Version fromVersion, Version toVersion)
        {
            var fromCompareVersion = new Version(fromVersion.Major, fromVersion.Minor, fromVersion.Build);
            var toCompareVersion = new Version(toVersion.Major, toVersion.Minor, toVersion.Build);
            var version = new Version(changeVersion.Attribute("Number").Value);
            var name = changeVersion.Attribute("Name").Value;
            var features = changeVersion.Descendants("Feature").Select(item => item.Value).ToList();
            var bugs = changeVersion.Descendants("Bug").Select(item => item.Value).ToList();
            var others = changeVersion.Descendants("Other").Select(item => item.Value).ToList();

            var newVersion = false;

            if (fromCompareVersion <= toCompareVersion)
            {
                var compareVersion = version;
                if (version.Revision > 0)
                {
                    compareVersion = new Version(version.Major, version.Minor, version.Build);
                }

                if (compareVersion == toCompareVersion)
                {
                    newVersion = true;
                }
                else if (compareVersion > fromCompareVersion && compareVersion < toCompareVersion)
                {
                    newVersion = true;
                }
            }

            return new ChangeLogVersion(version, name, newVersion, features, bugs, others);
        }

        private static ChangeLogVersion BuildChangeLogItem(XElement changeVersion)
        {
            var version = new Version(changeVersion.Attribute("Number").Value);
            var name = changeVersion.Attribute("Name").Value;
            var newVersion = false;
            var features = changeVersion.Descendants("Feature").Select(item => item.Value).ToList();
            var bugs = changeVersion.Descendants("Bug").Select(item => item.Value).ToList();
            var others = changeVersion.Descendants("Other").Select(item => item.Value).ToList();

            return new ChangeLogVersion(version, name, newVersion, features, bugs, others);
        }
    }
}
