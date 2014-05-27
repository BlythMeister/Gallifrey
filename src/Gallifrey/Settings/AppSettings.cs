using System;
using Gallifrey.Serialization;

namespace Gallifrey.Settings
{
    public interface IAppSettings
    {
        bool AlertWhenNotRunning { get; set; }
        int AlertTimeMilliseconds { get; set; }
        int KeepTimersForDays { get; set; }
        TimeSpan TargetLogPerDay { get; set; }
        int UiHeight { get; set; }
        int UiWidth { get; set; }
        bool UiAlwaysOnTop { get; set; }
        UiAnimationLevel UiAnimationLevel { get; set; }
    }

    public class AppSettings : IAppSettings
    {
        public bool AlertWhenNotRunning { get; set; }
        public int AlertTimeMilliseconds { get; set; }
        public int KeepTimersForDays { get; set; }
        public TimeSpan TargetLogPerDay { get; set; }
        public int UiHeight { get; set; }
        public int UiWidth { get; set; }
        public bool UiAlwaysOnTop { get; set; }
        public UiAnimationLevel UiAnimationLevel { get; set; }

        internal void SaveSettings()
        {
            AppSettingsSerializer.Serialize(this);    
        }
    }
}
