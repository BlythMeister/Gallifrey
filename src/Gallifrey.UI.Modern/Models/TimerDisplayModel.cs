using Gallifrey.JiraTimers;

namespace Gallifrey.UI.Modern.Models
{
    public class TimerDisplayModel
    {
        public JiraTimer Timer { get; set; }

        public string Reference => Timer.JiraReference;
        public string Description => Timer.JiraName;
        public string ParentReference => Timer.JiraParentReference;
        public string ParentDescription => Timer.JiraParentName;
        public bool HasParent => Timer.HasParent;

        public TimerDisplayModel(JiraTimer jiraTimer)
        {
            Timer = jiraTimer;
        }

        public override string ToString()
        {
            return $"{Reference} - {Description}";
        }
    }
}