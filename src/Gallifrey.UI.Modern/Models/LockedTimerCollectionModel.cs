using Gallifrey.IdleTimers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Gallifrey.UI.Modern.Models
{
    public class LockedTimerCollectionModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LockedTimerModel> LockedTimers { get; set; }

        public LockedTimerCollectionModel(IEnumerable<IdleTimer> unexportedTimers)
        {
            RefreshLockedTimers(unexportedTimers);
        }

        public void RefreshLockedTimers(IEnumerable<IdleTimer> unexportedTimers)
        {
            LockedTimers = new ObservableCollection<LockedTimerModel>(unexportedTimers.ToList().Select(x => new LockedTimerModel(x)).OrderByDescending(x => x.DateAndTimeForTimer));
            var firstTimer = LockedTimers.FirstOrDefault();
            if (firstTimer != null)
            {
                firstTimer.IsSelected = true;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LockedTimers)));
        }
    }
}
