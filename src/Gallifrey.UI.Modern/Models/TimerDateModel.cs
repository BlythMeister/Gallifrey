using System;
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
        public DateTime TimerDate { get; }
        public ObservableCollection<TimerModel> Timers { get; set; }

        public string Header => $"{TimerDate.ToString("ddd, dd MMM")} [{jiraTimerCollection.GetTotalTimeForDate(TimerDate)}]";
        public bool DateIsSelected { get; set; }

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Timers"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Header"));
        }

        public void RemoveTimerModel(TimerModel timerModel)
        {
            Timers.Remove(timerModel);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Timers"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Header"));
        }

        private void TimerModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "CurrentTime":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Header"));
                    break;
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (DateIsSelected != isSelected)
            {
                DateIsSelected = isSelected;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DateIsSelected"));
            }
        }
    }
}