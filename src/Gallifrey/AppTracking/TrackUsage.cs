using Gallifrey.Jira.Model;
using Gallifrey.Settings;
using Gallifrey.Versions;
using GoogleAnalyticsTracker.Core.TrackerParameters;
using GoogleAnalyticsTracker.Simple;
using System;
using System.Globalization;

namespace Gallifrey.AppTracking
{
    public interface ITrackUsage
    {
        void TrackAppUsage(TrackingType trackingType);

        void SetJiraCurrentUser(User user);
    }

    public class TrackUsage : ITrackUsage
    {
        private readonly IVersionControl versionControl;
        private readonly ISettingsCollection settingsCollection;
        private readonly InstanceType instanceType;
        private readonly SimpleTracker tracker;
        private User user;

        public TrackUsage(IVersionControl versionControl, ISettingsCollection settingsCollection, InstanceType instanceType)
        {
            this.versionControl = versionControl;
            this.settingsCollection = settingsCollection;
            this.instanceType = instanceType;
            tracker = new SimpleTracker("UA-51628201-7", new SimpleTrackerEnvironment(Environment.OSVersion.Platform.ToString(), Environment.OSVersion.Version.ToString(), Environment.OSVersion.VersionString));
            TrackAppUsage(TrackingType.AppLoad);
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

        public void SetJiraCurrentUser(User newUser)
        {
            user = newUser;
        }

        private PageView GetPageView(TrackingType trackingType)
        {
            var source = "Gallifrey";
            if (settingsCollection.InternalSettings.IsPremium)
            {
                source = "Gallifrey_Premium";
            }

            return new PageView
            {
                DocumentTitle = trackingType.ToString(),
                DocumentLocationUrl = $"https://releases.gallifreyapp.co.uk/tracking/{trackingType}",
                CampaignSource = source,
                CampaignMedium = instanceType.ToString(),
                CampaignName = versionControl.DeployedVersion.ToString(),
                UserId = settingsCollection.InstallationHash,
                UserLanguage = CultureInfo.InstalledUICulture.NativeName,
                CustomDimension1 = user == null ? "" : user.displayName,
                CustomDimension2 = user == null ? "" : user.name,
                CustomDimension3 = user == null ? "" : user.emailAddress,
                CustomDimension4 = user == null ? "" : user.accountId,
                CustomDimension5 = settingsCollection.JiraConnectionSettings.JiraUrl,
                CustomDimension6 = settingsCollection.JiraConnectionSettings.UseTempo.ToString()
            };
        }
    }
}
