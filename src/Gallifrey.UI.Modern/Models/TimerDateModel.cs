using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Gallifrey.Comparers;
using Gallifrey.JiraTimers;

namespace Gallifrey.UI.Modern.Models
{
    public class TimerDateModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IJiraTimerCollection jiraTimerCollection;
        public DateTime TimerDate { get; private set; }
        public ObservableCollection<TimerModel> Timers { get; set; }

        public string Header { get { return string.Format("{0} [{1}]", TimerDate.ToString("ddd, dd MMM"), jiraTimerCollection.GetTotalTimeForDate(TimerDate)); } }
        public bool IsSelected { get; set; }

        public TimerDateModel(DateTime timerDate, IJiraTimerCollection jiraTimerCollection)
        {
            this.jiraTimerCollection = jiraTimerCollection;
            TimerDate = timerDate;
            Timers = new ObservableCollection<TimerModel>();
        }

        public void AddTimerModel(TimerModel timerModel)
        {
            timerModel.PropertyChanged += TimerModelOnPropertyChanged;
            Timers.Add(timerModel);
            Timers = new ObservableCollection<TimerModel>(Timers.OrderBy(x => x.JiraTimer.JiraReference, new JiraReferenceComparer()));
        }

        public void RemoveTimerModel(TimerModel timerModel)
        {
            Timers.Remove(timerModel);
        }

        private void TimerModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (PropertyChanged != null)
            {
                switch (propertyChangedEventArgs.PropertyName)
                {
                    case "CurrentTime":
                        PropertyChanged(this, new PropertyChangedEventArgs("Header"));
                        break;
                }
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (IsSelected != isSelected)
            {
                IsSelected = isSelected;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));    
            }
        }
    }
}