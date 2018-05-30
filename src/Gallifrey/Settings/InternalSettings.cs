using System;

namespace Gallifrey.Settings
{
    public interface IInternalSettings
    {
        Version LastChangeLogVersion { get; }
        DateTime LastHeartbeatTracked { get; }
        Guid InstallationInstaceId { get; }
        bool IsPremium { get; }
        void SetLastChangeLogVersion(Version currentVersion);
        void SetLastHeartbeatTracked(DateTime lastHeartbeat);
        void SetIsPremium(bool isPremium);
        bool ValidateInstallationId();
    }

    public class InternalSettings : IInternalSettings
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Build { get; set; }
        public int Revision { get; set; }
        public DateTime LastHeartbeatTracked { get; set; }
        public Guid InstallationInstaceId { get; set; }
        public bool IsPremium { get; set; }

        public Version LastChangeLogVersion => new Version(Major, Minor, Build, Revision);

        public void SetLastChangeLogVersion(Version currentVersion)
        {
            Major = currentVersion.Major < 0 ? 0 : currentVersion.Major;
            Minor = currentVersion.Minor < 0 ? 0 : currentVersion.Minor;
            Build = currentVersion.Build < 0 ? 0 : currentVersion.Build;
            Revision = currentVersion.Revision < 0 ? 0 : currentVersion.Revision;
        }

        public void SetLastHeartbeatTracked(DateTime lastHeartbeat)
        {
            LastHeartbeatTracked = lastHeartbeat;
        }

        public void SetIsPremium(bool isPremium)
        {
            IsPremium = isPremium;
        }

        public bool ValidateInstallationId()
        {
            var setInstallationId = false;

            if (InstallationInstaceId == Guid.Empty)
            {
                setInstallationId = true;
                InstallationInstaceId = Guid.NewGuid();
            }

            return setInstallationId;
        }
    }
}
