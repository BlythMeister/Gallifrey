using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;

namespace Gallifrey.UI.Modern.Models
{
    public class ContributorsModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Timer contributorTimer;
        public string Contributors { get; set; }
        private readonly List<string> contributorList;
        private int position;

        public ContributorsModel()
        {
            contributorTimer = new Timer(1000);
            contributorTimer.Elapsed += ContributorTimerOnElapsed;
            contributorTimer.Start();

            contributorList = new List<string>()
            {
                "Mark Harrison\nTwitter: @HarrisonMeister\nGitHub: @HarrisonMeister",
            };

            position = 0;
            Contributors = contributorList[position];
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