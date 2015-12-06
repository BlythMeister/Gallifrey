using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Gallifrey.IdleTimers;

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
            LockedTimers = new ObservableCollection<LockedTimerModel>(unexportedTimers.Select(x => new LockedTimerModel(x)).OrderByDescending(x => x.DateAndTimeForTimer));
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("LockedTimers"));
        }
    }
}