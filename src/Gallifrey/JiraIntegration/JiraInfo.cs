using System;

namespace Gallifrey.JiraIntegration
{
    public class JiraInfo : IComparable<JiraInfo>, IEquatable<JiraInfo>
    {
        public string JiraReference { get; private set; }
        public string JiraProjectName { get; private set; }
        public string JiraName { get; private set; }

        public JiraInfo(string jiraReference, string jiraProjectName, string jiraName)
        {
            JiraReference = jiraReference;
            JiraProjectName = jiraProjectName;
            JiraName = jiraName;
        }

        public void UpdateDetail(string jiraProjectName, string jiraName)
        {
            JiraProjectName = jiraProjectName;
            JiraName = jiraName;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", JiraReference, JiraName);
        }

        public int CompareTo(JiraInfo other)
        {
            var jiraParts = JiraReference.Split('-');
            var otherJiraParts = other.JiraReference.Split('-');

            return jiraParts[0] == otherJiraParts[0] 
                       ? int.Parse(jiraParts[1]).CompareTo(int.Parse(otherJiraParts[1])) 
                       : JiraReference.CompareTo(other.JiraReference);
        }

        public bool Equals(JiraInfo other)
        {
            return JiraReference == other.JiraReference;
        }
    }
}