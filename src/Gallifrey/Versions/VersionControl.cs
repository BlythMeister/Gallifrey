using System;
using System.Deployment.Application;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        event EventHandler NewVersionInstalled;
        event EventHandler<bool> UpdateCheckOccured;
    }

    public class VersionControl : IVersionControl
    {
        public event EventHandler NewVersionInstalled;
        public event EventHandler<bool> UpdateCheckOccured;

        public InstanceType InstanceType { get; }
        public string VersionName { get; private set; }
        public string AppName { get; }
        public bool UpdateInstalled { get; private set; }

        private DateTime lastUpdateCheck;

        public bool IsAutomatedDeploy => ApplicationDeployment.IsNetworkDeployed;
        public Version DeployedVersion => UpdateInstalled ? ApplicationDeployment.CurrentDeployment.UpdatedVersion : ApplicationDeployment.CurrentDeployment.CurrentVersion;
        public bool IsFirstRun => ApplicationDeployment.CurrentDeployment.IsFirstRun;

        public VersionControl(InstanceType instanceType)
        {
            InstanceType = instanceType;
            lastUpdateCheck = DateTime.MinValue;

            SetVersionName();

            var instance = InstanceType == InstanceType.Stable ? "" : $" ({InstanceType})";
            AppName = $"Gallifrey{instance}";
        }

        private void SetVersionName()
        {
            VersionName = IsAutomatedDeploy ? DeployedVersion.ToString() : Application.ProductVersion;
            var betaText = InstanceType == InstanceType.Stable ? "" : $" ({InstanceType})";

            if (!IsAutomatedDeploy)
            {
                betaText = " (Debug)";
            }

            VersionName = $"v{VersionName}{betaText}";
        }

        public Task<UpdateResult> CheckForUpdates(bool manualCheck = false)
        {
            if (!IsAutomatedDeploy)
            {
                return Task.Factory.StartNew(() => UpdateResult.NotDeployable);
            }

            if (lastUpdateCheck >= DateTime.UtcNow.AddMinutes(-5) && !manualCheck)
            {
                return Task.Factory.StartNew(() => UpdateResult.TooSoon);
            }

            if (manualCheck)
            {
                UpdateCheckOccured?.Invoke(this, true);
            }
            else
            {
                UpdateCheckOccured?.Invoke(this, false);

            }
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
                        NewVersionInstalled?.Invoke(this, null);
                        return UpdateResult.Updated;
                    });
                }

                return Task.Factory.StartNew(() =>
                {
                    if (manualCheck)
                    {
                        Task.Delay(1500);
                    }
                    return UpdateResult.NoUpdate;
                });
            }
            catch (TrustNotGrantedException)
            {
                return Task.Factory.StartNew(() => UpdateResult.ReinstallNeeded);
            }
            catch (Exception)
            {
                return Task.Factory.StartNew(() => UpdateResult.Error);
            }
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