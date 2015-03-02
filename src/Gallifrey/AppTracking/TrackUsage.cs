using System;
using System.Deployment.Application;
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
        private readonly AppType appType;
        private string trackingQueryString;
        private bool trackingEnabled;
        private WebBrowser webBrowser;

        public TrackUsage(IAppSettings appSettings, IInternalSettings internalSettings, InstanceType instanceType, AppType appType)
        {
            this.instanceType = instanceType;
            this.appType = appType;
            trackingEnabled = appSettings.UsageTracking;
            SetTrackingQueryString(internalSettings);
            SetupBrowser();
            TrackAppUsage(TrackingType.AppLoad);
        }

        private void SetupBrowser()
        {
            try
            {
                if (trackingEnabled)
                {
                    webBrowser = new WebBrowser
                    {
                        ScrollBarsEnabled = false,
                        ScriptErrorsSuppressed = true
                    };
                }
                else
                {
                    webBrowser.Dispose();
                    webBrowser = null;
                }
            }
            catch (Exception)
            {
                webBrowser = null;
            }

        }

        public void TrackAppUsage(TrackingType trackingType)
        {
            if (trackingEnabled)
            {
                if (webBrowser == null)
                {
                    SetupBrowser();
                }

                try
                {
                    webBrowser.Navigate(string.Format("http://releases.gallifreyapp.co.uk/{0}.html?{1}", trackingType, trackingQueryString));
                    while (webBrowser.ReadyState != WebBrowserReadyState.Complete) { Application.DoEvents(); }
                }
                catch (Exception)
                {
                    SetupBrowser();
                }
            }
        }

        public void UpdateSettings(IAppSettings appSettings, IInternalSettings internalSettings)
        {
            SetTrackingQueryString(internalSettings);

            if (trackingEnabled && !appSettings.UsageTracking)
            {
                TrackAppUsage(TrackingType.OptOut);
            }

            trackingEnabled = appSettings.UsageTracking;
            SetupBrowser();
        }
        
        private void SetTrackingQueryString(IInternalSettings internalSettings)
        {
            var versionName = ApplicationDeployment.IsNetworkDeployed ? instanceType.ToString() : "Debug";
            
            trackingQueryString = string.Format("utm_source=GallifreyApp_{0}&utm_medium={1}&utm_campaign={2}", appType,versionName, internalSettings.LastChangeLogVersion);
        }
    }
}
