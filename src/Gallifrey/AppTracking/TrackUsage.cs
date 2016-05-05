using System;
using System.Deployment.Application;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gallifrey.Settings;
using Gallifrey.Versions;

namespace Gallifrey.AppTracking
{
    public interface ITrackUsage
    {
        void TrackAppUsage(TrackingType trackingType);
        void UpdateSettings(IAppSettings appSettings, IInternalSettings internalSettings);
    }

    public class TrackUsage : ITrackUsage
    {
        private readonly InstanceType instanceType;      
        private string trackingQueryString;
        private IAppSettings appSettings;
        private WebBrowser webBrowser;

        public TrackUsage(IAppSettings appSettings, IInternalSettings internalSettings, InstanceType instanceType)
        {
            this.appSettings = appSettings;
            this.instanceType = instanceType;
            SetTrackingQueryString(internalSettings);
            SetupBrowser();
            TrackAppUsage(TrackingType.AppLoad);
        }

        private bool IsTrackingEnabled(TrackingType trackingType)
        {
            return appSettings.UsageTracking || trackingType == TrackingType.DailyHearbeat;
        }

        private void SetupBrowser()
        {
            try
            {
                webBrowser = new WebBrowser
                                {
                                    ScrollBarsEnabled = false,
                                    ScriptErrorsSuppressed = true
                                };

            }
            catch (Exception)
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
                    webBrowser.Navigate($"http://releases.gallifreyapp.co.uk/tracking/{trackingType}.html?{trackingQueryString}");
                    while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                    {
                        await Task.Delay(1000);
                    }
                }
                catch (Exception)
                {
                    SetupBrowser();
                }
            }
        }

        public void UpdateSettings(IAppSettings newAppSettings, IInternalSettings internalSettings)
        {
            SetTrackingQueryString(internalSettings);

            if (appSettings.UsageTracking && !newAppSettings.UsageTracking)
            {
                TrackAppUsage(TrackingType.OptOut);
            }

            appSettings = newAppSettings;
            SetupBrowser();
        }

        private void SetTrackingQueryString(IInternalSettings internalSettings)
        {
            var versionName = ApplicationDeployment.IsNetworkDeployed ? instanceType.ToString() : "Debug";

            trackingQueryString = $"utm_source=GallifreyApp&utm_medium={versionName}&utm_campaign={internalSettings.LastChangeLogVersion}&uid={internalSettings.InstallationInstaceId}";
        }
    }
}
