using System;
using System.Deployment.Application;
using System.Net;
using System.Windows.Forms;
using Gallifrey.Settings;

namespace Gallifrey.AppTracking
{
    public interface ITrackUsage
    {
        void TrackAppUsage(TrackingType trackingType);
        void UpdateAppSettings(IAppSettings appSettings);
    }

    public class TrackUsage : ITrackUsage
    {
        private readonly string trackingQueryString;
        private bool trackingEnabled;
        private WebBrowser webBrowser;

        public TrackUsage(IAppSettings appSettings, IInternalSettings internalSettings, bool isBeta)
        {
            var betaText = isBeta ? "Beta" : "Stable";
            if (!ApplicationDeployment.IsNetworkDeployed)
            {
                betaText = "Debug";
            }
            
            trackingQueryString = string.Format("utm_source=GallifreyApp&utm_medium={0}&utm_campaign={1}", betaText, internalSettings.LastChangeLogVersion);
            trackingEnabled = appSettings.UsageTracking;
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

        public void UpdateAppSettings(IAppSettings appSettings)
        {
            if (trackingEnabled && !appSettings.UsageTracking)
            {
                TrackAppUsage(TrackingType.OptOut);
            }

            trackingEnabled = appSettings.UsageTracking;
            SetupBrowser();
        }
    }
}
