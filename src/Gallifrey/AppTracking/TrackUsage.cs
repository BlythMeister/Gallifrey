using Gallifrey.Settings;
using Gallifrey.Versions;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        private WebBrowser webBrowser;

        public TrackUsage(IVersionControl versionControl, ISettingsCollection settingsCollection, InstanceType instanceType)
        {
            this.versionControl = versionControl;
            this.settingsCollection = settingsCollection;
            this.instanceType = instanceType;
            SetupBrowser();
            TrackAppUsage(TrackingType.AppLoad);
        }

        private bool IsTrackingEnabled(TrackingType trackingType)
        {
            return versionControl.IsAutomatedDeploy && (settingsCollection.AppSettings.UsageTracking || trackingType == TrackingType.DailyHearbeat);
        }

        private void SetupBrowser()
        {
            try
            {
                webBrowser = new WebBrowser
                {
                    ScriptErrorsSuppressed = true
                };
            }
            catch (Exception) //Internal handled error
            {
                webBrowser = null;
            }
        }

        public async void TrackAppUsage(TrackingType trackingType)
        {
            if (IsTrackingEnabled(trackingType))
            {
                if (webBrowser == null)
                {
                    SetupBrowser();
                }

                try
                {
                    webBrowser.Navigate(GetNavigateUrl(trackingType));
                    while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                    {
                        await Task.Delay(500);
                    }
                }
                catch (Exception) //Internal handled error
                {
                    SetupBrowser();
                }
            }
        }

        private string GetNavigateUrl(TrackingType trackingType)
        {
            var prem = "Gallifrey";
            if (settingsCollection.InternalSettings.IsPremium)
            {
                prem = "Gallifrey_Premium";
            }

            return $"https://releases.gallifreyapp.co.uk/tracking/{trackingType}.html?utm_source={prem}&utm_medium={instanceType}&utm_campaign={versionControl.DeployedVersion}&uid={settingsCollection.InstallationHash}";
        }
    }
}
