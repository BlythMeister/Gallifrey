using System;
using System.ComponentModel;
using System.Deployment.Application;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gallifrey.AppTracking;
using Gallifrey.Exceptions.Versions;

namespace Gallifrey.Versions
{
    public interface IVersionControl
    {
        InstanceType InstanceType { get; }
        bool IsAutomatedDeploy { get; }
        bool UpdateInstalled { get; }
        string VersionName { get; }
        Version DeployedVersion { get; }
        bool IsFirstRun { get; }
        string AppName { get; }
        Task<UpdateResult> CheckForUpdates(bool manualCheck = false);
        void ManualReinstall();
        event PropertyChangedEventHandler PropertyChanged;
    }

    public class VersionControl : IVersionControl
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ITrackUsage trackUsage;
        public InstanceType InstanceType { get; private set; }
        public string VersionName { get; private set; }
        public string AppName { get; private set; }
        public bool UpdateInstalled { get; private set; }

        private DateTime lastUpdateCheck;

        public bool IsAutomatedDeploy => ApplicationDeployment.IsNetworkDeployed;
        public Version DeployedVersion => UpdateInstalled ? ApplicationDeployment.CurrentDeployment.UpdatedVersion : ApplicationDeployment.CurrentDeployment.CurrentVersion;
        public bool IsFirstRun => ApplicationDeployment.CurrentDeployment.IsFirstRun;

        public VersionControl(InstanceType instanceType, ITrackUsage trackUsage)
        {
            this.trackUsage = trackUsage;
            InstanceType = instanceType;
            lastUpdateCheck = DateTime.MinValue;

            SetVersionName();

            var instance = InstanceType == InstanceType.Stable ? "" : $" ({InstanceType})";
            AppName = $"Gallifrey {instance}";
        }

        private void SetVersionName()
        {
            VersionName = IsAutomatedDeploy ? DeployedVersion.ToString() : Application.ProductVersion;
            if (IsAutomatedDeploy && InstanceType == InstanceType.Stable)
            {
                VersionName = VersionName.Substring(0, VersionName.LastIndexOf("."));
            }

            var betaText = InstanceType == InstanceType.Stable ? "" : $" ({InstanceType})";

            if (!IsAutomatedDeploy)
            {
                betaText = " (Debug)";
            }

            VersionName = $"v{VersionName}{betaText}";

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VersionName")); 
        }
        
        public Task<UpdateResult> CheckForUpdates(bool manualCheck = false)
        {
            if (!IsAutomatedDeploy)
            {
                return Task.Factory.StartNew(() => UpdateResult.NotDeployable);
            }

            if (IsAutomatedDeploy && (manualCheck || lastUpdateCheck < DateTime.UtcNow.AddMinutes(-5)))
            {
                trackUsage.TrackAppUsage(TrackingType.UpdateCheck);
                lastUpdateCheck = DateTime.UtcNow;

                try
                {
                    var updateInfo = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate(false);

                    if (updateInfo.UpdateAvailable && updateInfo.AvailableVersion > ApplicationDeployment.CurrentDeployment.CurrentVersion)
                    {
                        return Task.Factory.StartNew(() => ApplicationDeployment.CurrentDeployment.Update()).ContinueWith(task =>
                        {
                            SetVersionName();
                            UpdateInstalled = true;
                            return UpdateResult.Updated;
                        });
                    }

                    if (manualCheck)
                    {
                        return Task.Factory.StartNew(() =>
                        {
                            Task.Delay(1500);
                            return UpdateResult.NoUpdate;
                        });
                    }
                }
                catch (TrustNotGrantedException)
                {
                    if (manualCheck)
                    {
                        throw new ManualReinstallRequiredException();
                    }
                }
                catch (Exception)
                {
                }
            }

            return Task.Factory.StartNew(() => UpdateResult.TooSoon);
        }
        
        public void ManualReinstall()
        {
            var installUrl = ApplicationDeployment.CurrentDeployment.UpdateLocation.AbsoluteUri;
            DeploymentUtils.Uninstaller.UninstallMe();
            DeploymentUtils.Uninstaller.AutoInstall(installUrl);
            Application.Exit();
        }
    }
}