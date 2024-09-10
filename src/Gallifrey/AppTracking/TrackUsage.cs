using Gallifrey.Settings;
using Gallifrey.Versions;
using System;

namespace Gallifrey.AppTracking
{
    public interface ITrackUsage
    {
        void TrackAppUsage(TrackingType trackingType);

        event EventHandler<TrackingType> TrackEvent;
    }

    public class TrackUsage : ITrackUsage
    {
        public event EventHandler<TrackingType> TrackEvent;

        private readonly IVersionControl versionControl;
        private readonly ISettingsCollection settingsCollection;

        public TrackUsage(IVersionControl versionControl, ISettingsCollection settingsCollection)
        {
            this.versionControl = versionControl;
            this.settingsCollection = settingsCollection;
        }

        private bool IsTrackingEnabled(TrackingType trackingType)
        {
            return versionControl.IsAutomatedDeploy && (settingsCollection.AppSettings.UsageTracking || trackingType == TrackingType.DailyCheckIn);
        }

        public void TrackAppUsage(TrackingType trackingType)
        {
            if (IsTrackingEnabled(trackingType))
            {
                try
                {
                    TrackEvent?.Invoke(this, trackingType);
                }
                catch (Exception) //Internal handled error
                {
                    //Ignored
                }
            }
        }
    }
}
