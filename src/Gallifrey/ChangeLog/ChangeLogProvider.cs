using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Gallifrey.ChangeLog
{
    internal static class ChangeLogProvider
    {
        public static IDictionary<Version, ChangeLogVersionDetails> GetChangeLog(Version fromVersion, Version toVersion)
        {
            var changeLogPath = Path.Combine(Environment.CurrentDirectory, "ChangeLog.xml");
            var changeLog = new Dictionary<Version, ChangeLogVersionDetails>();
            if (File.Exists(changeLogPath) && fromVersion > new Version("0.0.0.0"))
            {
                var changeLogFile = XDocument.Load(changeLogPath);
                foreach (var changeVersion in changeLogFile.Descendants("Version"))
                {
                    var changeLogItem = BuildChangeLogItem(changeVersion);
                    changeLog.Add(changeLogItem.Key, changeLogItem.Value);
                }
            }

            if (fromVersion < toVersion && changeLog.Any(x => x.Key > fromVersion && x.Key <= toVersion))
            {
                return changeLog.Where(x => x.Key > fromVersion && x.Key <= toVersion).ToDictionary(detail => detail.Key, detail => detail.Value);
            }

            return changeLog.Where(x => x.Key <= toVersion).ToDictionary(detail => detail.Key, detail => detail.Value);;
        }

        private static KeyValuePair<Version, ChangeLogVersionDetails> BuildChangeLogItem(XElement changeVersion)
        {
            var version = new Version(changeVersion.Element("Number").Value);
            var details = BuildChangeLogVersionDetails(changeVersion);
            return new KeyValuePair<Version, ChangeLogVersionDetails>(version, details);
        }

        private static ChangeLogVersionDetails BuildChangeLogVersionDetails(XElement changeVersion)
        {
            var features = changeVersion.Descendants("Feature").Select(item => item.Value).ToList();
            var bugs = changeVersion.Descendants("Bug").Select(item => item.Value).ToList();
            var others = changeVersion.Descendants("Other").Select(item => item.Value).ToList();

            return new ChangeLogVersionDetails(features, bugs, others);
        }
    }
}
