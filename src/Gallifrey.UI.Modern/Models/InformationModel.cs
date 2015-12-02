using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using Gallifrey.Contributors;

namespace Gallifrey.UI.Modern.Models
{
    public class InformationModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Contributors { get; set; }
        public string InstallationId { get; set; }
        private readonly List<string> contributorList;
        private int position;

        public InformationModel(IBackend gallifrey)
        {
            var contributorTimer = new Timer(1000);
            contributorTimer.Elapsed += ContributorTimerOnElapsed;
            contributorTimer.Start();

            contributorList = gallifrey.WithThanksDefinitions.Select(BuildThanksString).ToList();

            position = 0;
            Contributors = contributorList[position];

            InstallationId = string.Format("Your installation has an identifier of: {0}", gallifrey.Settings.InternalSettings.InstallationInstaceId);
        }

        private string BuildThanksString(WithThanksDefinition withThanksDefinition)
        {
            var returnString = new StringBuilder();

            returnString.AppendLine(withThanksDefinition.Name);

            if (!string.IsNullOrWhiteSpace(withThanksDefinition.ThanksReason)) returnString.AppendLine(string.Format("Reason: {0}", withThanksDefinition.ThanksReason));
            if (!string.IsNullOrWhiteSpace(withThanksDefinition.TwitterHandle)) returnString.AppendLine(string.Format("Twitter: {0}", withThanksDefinition.TwitterHandle));
            if (!string.IsNullOrWhiteSpace(withThanksDefinition.GitHubHandle)) returnString.AppendLine(string.Format("GitHub: {0}", withThanksDefinition.GitHubHandle));

            return returnString.ToString();
        }

        private void ContributorTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            position++;
            if (contributorList.Count == position)
            {
                position = 0;
            }
            Contributors = contributorList[position];

            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Contributors"));
        }
    }
}