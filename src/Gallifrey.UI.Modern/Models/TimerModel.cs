using System.ComponentModel;
using Gallifrey.ExtensionMethods;
using Gallifrey.JiraTimers;

namespace Gallifrey.UI.Modern.Models
{
    public class TimerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public JiraTimer JiraTimer { get; set; }

        public bool TimerIsSelected { get; set; }

        public string ExportTime => JiraTimer.TimeToExport.FormatAsString(false);
        public string CurrentTime => JiraTimer.ExactCurrentTime.FormatAsString();
        public string Description => JiraTimer.JiraName;
        public string ParentDescription => JiraTimer.JiraParentName;
        public string Reference => JiraTimer.JiraReference;
        public string ParentReference => JiraTimer.JiraParentReference;
        public bool HasParent => JiraTimer.HasParent;
        public bool HasTimeToExport => !JiraTimer.FullyExported;
        public bool HasTimeToExportAndNotRunning => !JiraTimer.FullyExported && !JiraTimer.IsRunning;
        public bool IsRunning => JiraTimer.IsRunning;
        public bool IsLocalTimer => JiraTimer.LocalTimer;

        public TimerModel(JiraTimer timer)
        {
            JiraTimer = timer;
            JiraTimer.PropertyChanged += JiraTimerOnPropertyChanged;
        }

        private void JiraTimerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "ExactCurrentTime":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentTime"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasTimeToExportAndNotRunning"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasTimeToExport"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportTime"));
                    break;
                case "IsRunning":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasTimeToExportAndNotRunning"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasTimeToExport"));
                    break;
                case "TimeToExport":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportTime"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasTimeToExport"));
                    break;
                case "JiraName":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Description"));
                    break;
                case "JiraParentName":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ParentDescription"));
                    break;
                case "HasParent":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasParent"));
                    break;
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (TimerIsSelected != isSelected)
            {
                TimerIsSelected = isSelected;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimerIsSelected"));
            }
        }
    }
}