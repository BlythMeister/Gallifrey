using Gallifrey.Settings;
using Gallifrey.Versions;
using GoogleAnalyticsTracker.Core.TrackerParameters;
using GoogleAnalyticsTracker.Simple;
using System;
using System.Globalization;
using System.Timers;

namespace Gallifrey.AppTracking
{
    public interface ITrackUsage
    {
        void TrackAppUsage(TrackingType trackingType);
    }

    public class TrackUsage : ITrackUsage
    {
        private readonly IVersionControl versionControl;
        private readonly ISettingsCollection settingsCollection;
        private readonly InstanceType instanceType;
        private readonly SimpleTracker tracker;
        private readonly Timer heartbeat;

        public TrackUsage(IVersionControl versionControl, ISettingsCollection settingsCollection, InstanceType instanceType)
        {
            this.versionControl = versionControl;
            this.settingsCollection = settingsCollection;
            this.instanceType = instanceType;
            tracker = new SimpleTracker("UA-51628201-7", new SimpleTrackerEnvironment(Environment.OSVersion.Platform.ToString(), Environment.OSVersion.Version.ToString(), Environment.OSVersion.VersionString));
            TrackAppUsage(TrackingType.AppLoad);
            heartbeat = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            heartbeat.Elapsed += (sender, args) => TrackAppUsage(TrackingType.Heartbeat);
            heartbeat.Enabled = true;
        }

        private bool IsTrackingEnabled(TrackingType trackingType)
        {
            return versionControl.IsAutomatedDeploy && (settingsCollection.AppSettings.UsageTracking || trackingType == TrackingType.DailyHeartbeat);
        }

        public async void TrackAppUsage(TrackingType trackingType)
        {
            if (IsTrackingEnabled(trackingType))
            {
                try
                {
                    var pageView = GetPageView(trackingType);
                    await tracker.TrackAsync(pageView);
                }
                catch (Exception) //Internal handled error
                {
                    //Ignored
                }
            }
        }

        private PageView GetPageView(TrackingType trackingType)
        {
            return new PageView
            {
                DocumentTitle = trackingType.ToString(),
                DocumentLocationUrl = $"https://releases.gallifreyapp.co.uk/tracking/{trackingType}",
                CampaignSource = instanceType.ToString(),
                CampaignMedium = $"{versionControl.DeployedVersion.Major}.{versionControl.DeployedVersion.Minor}.{versionControl.DeployedVersion.Build}",
                CampaignName = $"{versionControl.DeployedVersion.Major}.{versionControl.DeployedVersion.Minor}.{versionControl.DeployedVersion.Build}.{versionControl.DeployedVersion.Revision}",
                UserId = settingsCollection.InstallationHash,
                UserLanguage = CultureInfo.InstalledUICulture.NativeName
            };
        }
    }
}
