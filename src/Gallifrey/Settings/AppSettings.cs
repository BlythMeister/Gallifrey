﻿using Newtonsoft.Json;
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
    }

    public class AppSettings : IAppSettings
    {
        public bool AlertWhenNotRunning { get; set; }
        public int AlertTimeMilliseconds { get; set; }
        public int KeepTimersForDays { get; set; }
        public TimeSpan TargetLogPerDay { get; set; }
        public IEnumerable<DayOfWeek> ExportDays { get; set; }
        public DayOfWeek StartOfWeek { get; set; }
        public Guid? TimerRunningOnShutdown { get; set; }
        public bool AutoUpdate { get; set; }
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
        
        public AppSettings()
        {
            KeepTimersForDays = 7;
            ExportDays = new List<DayOfWeek>();
            StartOfWeek = DayOfWeek.Monday;
            UsageTracking = true;
            TrackLockTime = true;
            LockTimeThresholdMilliseconds = 60000;
            IdleTimeThresholdMilliseconds = 300000;
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
