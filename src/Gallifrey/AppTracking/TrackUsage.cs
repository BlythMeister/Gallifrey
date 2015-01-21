using System;
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
        private IAppSettings appSettings;
        private WebBrowser webBrowser;

        public TrackUsage(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
            SetupBrowser();
            TrackAppUsage(TrackingType.AppLoad);
        }

        private void SetupBrowser()
        {
            try
            {
                if (appSettings.UsageTracking)
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
            if (webBrowser != null)
            {
                try
                {
                    webBrowser.Navigate(string.Format("http://releases.gallifreyapp.co.uk/{0}.html", trackingType));
                    while (webBrowser.ReadyState != WebBrowserReadyState.Complete) { Application.DoEvents(); }
                }
                catch (Exception)
                {
                    SetupBrowser();
                }
            }
        }

        public void UpdateAppSettings(IAppSettings newAppSettings)
        {
            if (webBrowser != null && !newAppSettings.UsageTracking)
            {
                TrackAppUsage(TrackingType.OptOut);
            }

            appSettings = newAppSettings;
            SetupBrowser();
        }
    }
}
