using System;
using System.CodeDom;
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
        AppType AppType { get; }
        bool IsAutomatedDeploy { get; }
        string ActivationUrl { get; }
        bool AlreadyInstalledUpdate { get; }
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
        public AppType AppType { get; private set; }
        public string VersionName { get; private set; }

        public string AppName { get; private set; }
        
        private DateTime lastUpdateCheck;

        public VersionControl(InstanceType instanceType, AppType appType, ITrackUsage trackUsage)
        {
            this.trackUsage = trackUsage;
            InstanceType = instanceType;
            AppType = appType;
            lastUpdateCheck = DateTime.MinValue;

            SetVersionName();

            var instance = InstanceType == InstanceType.Stable ? "" : string.Format(" ({0})", InstanceType);
            var appName = AppType == AppType.Classic ? "Gallifrey Classic" : "Gallifrey";

            AppName = string.Format("{0}{1}", appName, instance);
        }

        private void SetVersionName()
        {
            VersionName = IsAutomatedDeploy ? DeployedVersion.ToString() : Application.ProductVersion;
            if (IsAutomatedDeploy && InstanceType == InstanceType.Stable)
            {
                VersionName = VersionName.Substring(0, VersionName.LastIndexOf("."));
            }

            var betaText = InstanceType == InstanceType.Stable ? "" : string.Format(" ({0})", InstanceType);

            if (!IsAutomatedDeploy)
            {
                betaText = " (Debug)";
            }

            VersionName = string.Format("v{0}{1}", VersionName, betaText);

            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("VersionName")); 
        }

        public bool IsAutomatedDeploy
        {
            get { return ApplicationDeployment.IsNetworkDeployed; }
        }

        public string ActivationUrl
        {
            get { return ApplicationDeployment.CurrentDeployment.ActivationUri != null ? ApplicationDeployment.CurrentDeployment.ActivationUri.ToString() : string.Empty; }
        }

        public bool AlreadyInstalledUpdate
        {
            get { return ApplicationDeployment.IsNetworkDeployed && ApplicationDeployment.CurrentDeployment != null && ApplicationDeployment.CurrentDeployment.UpdatedVersion != ApplicationDeployment.CurrentDeployment.CurrentVersion; }
        }

        public Version DeployedVersion
        {
            get { return AlreadyInstalledUpdate ? ApplicationDeployment.CurrentDeployment.UpdatedVersion : ApplicationDeployment.CurrentDeployment.CurrentVersion; }
        }

        public bool IsFirstRun
        {
            get { return ApplicationDeployment.CurrentDeployment.IsFirstRun; }
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