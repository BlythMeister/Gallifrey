using System;

namespace Gallifrey.Settings
{
    public interface IAppSettings
    {
        bool AlertWhenNotRunning { get; set; }
        int AlertTimeMilliseconds { get; set; }
        int KeepTimersForDays { get; set; }
        TimeSpan TargetLogPerDay { get; set; }
        Guid? TimerRunningOnShutdown { get; set; }
    }

    public class AppSettings : IAppSettings
    {
        public bool AlertWhenNotRunning { get; set; }
        public int AlertTimeMilliseconds { get; set; }
        public int KeepTimersForDays { get; set; }
        public TimeSpan TargetLogPerDay { get; set; }
        public Guid? TimerRunningOnShutdown { get; set; }

        public AppSettings()
        {
            KeepTimersForDays = 7;
        }
    }
}
