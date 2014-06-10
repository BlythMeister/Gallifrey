using System;

namespace Gallifrey.Settings
{
    public interface IInternalSettings
    {
        Version LastChangeLogVersion { get; }
        void SetLastChangeLogVersion(Version currentVersion);
    }

    public class InternalSettings : IInternalSettings
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Build { get; set; }
        public int Revision { get; set; }

        public Version LastChangeLogVersion { get { return new Version(Major, Minor, Build, Revision); } }
        public void SetLastChangeLogVersion(Version currentVersion)
        {
            Major = currentVersion.Major;
            Minor = currentVersion.Minor;
            Build = currentVersion.Build;
            Revision = currentVersion.Revision;
        }
    }
}
