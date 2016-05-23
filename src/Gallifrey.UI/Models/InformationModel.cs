using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using Gallifrey.Contributors;

namespace Gallifrey.UI.Models
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

            InstallationId = gallifrey.Settings.InternalSettings.InstallationInstaceId.ToString();
        }

        private string BuildThanksString(WithThanksDefinition withThanksDefinition)
        {
            var returnString = new StringBuilder();

            returnString.AppendLine(withThanksDefinition.Name);

            if (!string.IsNullOrWhiteSpace(withThanksDefinition.ThanksReason)) returnString.AppendLine($"Reason: {withThanksDefinition.ThanksReason}");
            if (!string.IsNullOrWhiteSpace(withThanksDefinition.TwitterHandle)) returnString.AppendLine($"Twitter: {withThanksDefinition.TwitterHandle}");
            if (!string.IsNullOrWhiteSpace(withThanksDefinition.GitHubHandle)) returnString.AppendLine($"GitHub: {withThanksDefinition.GitHubHandle}");

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

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Contributors"));
        }
    }
}