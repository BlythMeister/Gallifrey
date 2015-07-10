using System.ComponentModel;
using Gallifrey.ExtensionMethods;
using Gallifrey.JiraTimers;

namespace Gallifrey.UI.Modern.Models
{
    public class TimerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public JiraTimer JiraTimer { get; set; }
        public string ExportTime { get { return JiraTimer.TimeToExport.FormatAsString(false); } }
        public string CurrentTime { get { return JiraTimer.ExactCurrentTime.FormatAsString(); } }
        public string Description { get { return JiraTimer.JiraName; } }
        public string ParentDescription { get { return JiraTimer.JiraParentName; } }
        public string Reference { get { return JiraTimer.JiraReference; } }
        public string ParentReference { get { return JiraTimer.JiraParentReference; } }
        public bool HasParent { get { return JiraTimer.HasParent; } }
        public bool HasTimeToExport { get { return !JiraTimer.FullyExported; } }
        public bool HasTimeToExportAndNotRunning { get { return !JiraTimer.FullyExported && !JiraTimer.IsRunning; } }
        public bool IsRunning { get { return JiraTimer.IsRunning; } }
        public bool IsSelected { get; set; }

        public TimerModel(JiraTimer timer)
        {
            JiraTimer = timer;
            JiraTimer.PropertyChanged += JiraTimerOnPropertyChanged;
        }

        private void JiraTimerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (PropertyChanged != null)
            {
                switch (propertyChangedEventArgs.PropertyName)
                {
                    case "ExactCurrentTime":
                        PropertyChanged(this, new PropertyChangedEventArgs("CurrentTime"));
                        PropertyChanged(this, new PropertyChangedEventArgs("IsRunning"));
                        PropertyChanged(this, new PropertyChangedEventArgs("HasTimeToExportAndNotRunning"));
                        PropertyChanged(this, new PropertyChangedEventArgs("HasTimeToExport"));
                        PropertyChanged(this, new PropertyChangedEventArgs("ExportTime"));
                        break;
                    case "IsRunning":
                        PropertyChanged(this, new PropertyChangedEventArgs("IsRunning"));
                        PropertyChanged(this, new PropertyChangedEventArgs("HasTimeToExportAndNotRunning"));
                        PropertyChanged(this, new PropertyChangedEventArgs("HasTimeToExport"));
                        break;
                    case "TimeToExport":
                        PropertyChanged(this, new PropertyChangedEventArgs("ExportTime"));
                        PropertyChanged(this, new PropertyChangedEventArgs("HasTimeToExport"));
                        break;
                    case "JiraName":
                        PropertyChanged(this, new PropertyChangedEventArgs("Description"));
                        break;
                    case "JiraParentName":
                        PropertyChanged(this, new PropertyChangedEventArgs("ParentDescription"));
                        break;
                    case "HasParent":
                        PropertyChanged(this, new PropertyChangedEventArgs("HasParent"));
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