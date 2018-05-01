using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Gallifrey.Settings
{
    public interface IAppSettings
    {
        bool AlertWhenNotRunning { get; set; }
        int AlertTimeMilliseconds { get; set; }
        int KeepTimersForDays { get; set; }
        TimeSpan TargetLogPerDay { get; set; }
        IEnumerable<DayOfWeek> ExportDays { get; set; }
        DayOfWeek StartOfWeek { get; set; }
        Guid? TimerRunningOnShutdown { get; set; }
        TimeSpan GetTargetThisWeek();
        bool AutoUpdate { get; set; }
        bool UsageTracking { get; set; }
        bool TrackLockTime { get; set; }
        int LockTimeThresholdMilliseconds { get; set; }
        bool TrackIdleTime { get; set; }
        int IdleTimeThresholdMilliseconds { get; set; }
        List<string> DefaultTimers { get; set; }
    }

    public class AppSettings : IAppSettings
    {
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AlertWhenNotRunning { get; set; }

        [DefaultValue(300000)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int AlertTimeMilliseconds { get; set; }

        [DefaultValue(7)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int KeepTimersForDays { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public TimeSpan TargetLogPerDay { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public IEnumerable<DayOfWeek> ExportDays { get; set; }

        [DefaultValue(DayOfWeek.Monday)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public DayOfWeek StartOfWeek { get; set; }

        [DefaultValue(null)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public Guid? TimerRunningOnShutdown { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AutoUpdate { get; set; }

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsageTracking { get; set; }

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool TrackLockTime { get; set; }

        [DefaultValue(60000)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int LockTimeThresholdMilliseconds { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool TrackIdleTime { get; set; }

        [DefaultValue(300000)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int IdleTimeThresholdMilliseconds { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<string> DefaultTimers { get; set; }

        public AppSettings()
        {
            AlertWhenNotRunning = true;
            AlertTimeMilliseconds = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
            KeepTimersForDays = 7;
            TargetLogPerDay = TimeSpan.Zero;
            ExportDays = new List<DayOfWeek>();
            StartOfWeek = DayOfWeek.Monday;
            TimerRunningOnShutdown = null;
            AutoUpdate = false;
            UsageTracking = true;
            TrackLockTime = true;
            LockTimeThresholdMilliseconds = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            TrackIdleTime = false;
            IdleTimeThresholdMilliseconds = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
            DefaultTimers = new List<string>();
        }

        public TimeSpan GetTargetThisWeek()
        {
            var target = new TimeSpan();
            var moreDays = true;
            var queryDay = StartOfWeek;
            while (moreDays)
            {
                if (ExportDays.Contains(queryDay))
                {
                    target = target.Add(TargetLogPerDay);
                }

                if (queryDay == DateTime.Today.DayOfWeek)
                {
                    moreDays = false;
                }
                else
                {
                    if ((int)queryDay == 6)
                    {
                        queryDay = 0;
                    }
                    else
                    {
                        queryDay = queryDay + 1;
                    }
                }
            }

            return target;
        }
    }
}
