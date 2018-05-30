using Gallifrey.JiraIntegration;
using Gallifrey.JiraTimers;

namespace Gallifrey.UI.Modern.Models
{
    public class TimerDisplayModel
    {
        public JiraTimer Timer { get; }
        public string Reference { get; }
        public string Description { get; }
        public string ParentReference { get; }
        public string ParentDescription { get; }
        public bool HasParent { get; }

        public TimerDisplayModel(JiraTimer jiraTimer)
        {
            Timer = jiraTimer;
            Reference = jiraTimer.JiraReference;
            Description = jiraTimer.JiraName;
            ParentReference = jiraTimer.JiraParentReference;
            ParentDescription = jiraTimer.JiraParentName;
            HasParent = jiraTimer.HasParent;
        }

        public TimerDisplayModel(RecentJira recentJira)
        {
            Timer = null;
            Reference = recentJira.JiraReference;
            Description = recentJira.JiraName;
            ParentReference = recentJira.JiraParentReference;
            ParentDescription = recentJira.JiraParentName;
            HasParent = !string.IsNullOrWhiteSpace(recentJira.JiraParentReference);
        }

        public override string ToString()
        {
            return $"{Reference} - {Description}";
        }
    }
}