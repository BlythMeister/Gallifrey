using System;

namespace Gallifrey.Settings
{
    public interface IInternalSettings
    {
        Version LastChangeLogVersion { get; }
        DateTime LastHeartbeatTracked { get; set; }
        void SetLastChangeLogVersion(Version currentVersion);
    }

    public class InternalSettings : IInternalSettings
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Build { get; set; }
        public int Revision { get; set; }
        public DateTime LastHeartbeatTracked { get; set; }
        public Guid? UniqueId { get; set; }
        
        public Version LastChangeLogVersion { get { return new Version(Major, Minor, Build, Revision); } }

        public void SetLastChangeLogVersion(Version currentVersion)
        {
            Major = currentVersion.Major < 0 ? 0 : currentVersion.Major;
            Minor = currentVersion.Minor < 0 ? 0 : currentVersion.Minor;
            Build = currentVersion.Build < 0 ? 0 : currentVersion.Build;
            Revision = currentVersion.Revision < 0 ? 0 : currentVersion.Revision;
        }
    }
}
