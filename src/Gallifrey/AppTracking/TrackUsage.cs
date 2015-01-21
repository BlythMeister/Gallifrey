using System;
using System.Net;
using System.Windows.Forms;

namespace Gallifrey.AppTracking
{
    public interface ITrackUsage
    {
        void TrackAppUsage(TrackingType trackingType);
    }

    public class TrackUsage : ITrackUsage
    {
        private readonly WebBrowser webBrowser;

        public TrackUsage()
        {
            webBrowser = new WebBrowser();
            webBrowser.ScrollBarsEnabled = false;
            webBrowser.ScriptErrorsSuppressed = true;
        }

        public void TrackAppUsage(TrackingType trackingType)
        {
            try
            {
                webBrowser.Navigate(string.Format("http://releases.gallifreyapp.co.uk/{0}.html", trackingType));
                while (webBrowser.ReadyState != WebBrowserReadyState.Complete) { Application.DoEvents(); }
            }
            catch (Exception)
            {
                
            }
        }
    }
}
