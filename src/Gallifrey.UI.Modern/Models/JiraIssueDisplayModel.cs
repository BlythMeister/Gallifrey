using Gallifrey.Jira.Model;

namespace Gallifrey.UI.Modern.Models
{
    public class JiraIssueDisplayModel
    {
        public string Reference { get; set; }
        public string Description { get; set; }
        public bool HasParent { get { return !string.IsNullOrWhiteSpace(ParentReference); } }
        public string ParentReference { get; set; }
        public string ParentDescription { get; set; }

        public JiraIssueDisplayModel(Issue issue)
        {
            Reference = issue.key;
            Description = issue.fields.summary;
            if (issue.fields.parent != null)
            {
                ParentReference = issue.fields.parent.key;
                ParentDescription = issue.fields.parent.fields.summary;
            }
        }
    }
}