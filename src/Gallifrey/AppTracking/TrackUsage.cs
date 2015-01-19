using System.Net;

namespace Gallifrey.AppTracking
{
    public interface ITrackUsage
    {
        void TrackAppUsage(TrackingType trackingType);
    }

    public class TrackUsage : ITrackUsage
    {
        public void TrackAppUsage(TrackingType trackingType)
        {
            var request = WebRequest.Create(string.Format("http://blythmeister.github.io/Gallifrey.Releases/{0}.html", trackingType.ToString()));

            request.GetResponseAsync();
        }
    }
}
