using Gallifrey.ExtensionMethods;
using Gallifrey.JiraTimers;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Gallifrey.UI.Modern.Models
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TimerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public JiraTimer JiraTimer { get; set; }

        public bool TimerIsSelected { get; set; }
        public bool TrackingOnly { get; private set; }

        public string ExportTime => JiraTimer.TimeToExport.FormatAsString(false);
        public string CurrentTime => JiraTimer.ExactCurrentTime.FormatAsString();
        public string Description => JiraTimer.JiraName;
        public string ParentDescription => JiraTimer.JiraParentName;
        public string Reference => JiraTimer.JiraReference;
        public string ParentReference => JiraTimer.JiraParentReference;
        public bool HasParent => JiraTimer.HasParent;
        public bool HasTimeToExport => !JiraTimer.LocalTimer && !JiraTimer.FullyExported && !TrackingOnly;
        public bool HasTimeToExportAndNotRunning => !JiraTimer.FullyExported && !JiraTimer.IsRunning;
        public bool IsRunning => JiraTimer.IsRunning;
        public bool HighlightTitle => JiraTimer.LocalTimer && !TrackingOnly;

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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTime)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasTimeToExportAndNotRunning)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasTimeToExport)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExportTime)));
                    break;

                case "IsRunning":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasTimeToExportAndNotRunning)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasTimeToExport)));
                    break;

                case "TimeToExport":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExportTime)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasTimeToExport)));
                    break;

                case "JiraName":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
                    break;

                case "JiraParentName":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ParentDescription)));
                    break;

                case "HasParent":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasParent)));
                    break;
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (TimerIsSelected != isSelected)
            {
                TimerIsSelected = isSelected;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimerIsSelected)));
            }
        }

        public void SetTrackingOnly(bool trackingOnly)
        {
            if (TrackingOnly != trackingOnly)
            {
                TrackingOnly = trackingOnly;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrackingOnly)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasTimeToExport)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighlightTitle)));
            }
        }
    }
}
