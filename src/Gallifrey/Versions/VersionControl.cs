using System;
using System.Deployment.Application;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Gallifrey.Versions
{
    public interface IVersionControl
    {
        InstanceType InstanceType { get; }
        bool IsAutomatedDeploy { get; }
        bool UpdateInstalled { get; }
        bool UpdateReinstallNeeded { get; }
        bool UpdateError { get; }
        string VersionName { get; }
        bool IsFirstRun { get; }
        string AppName { get; }
        Version DeployedVersion { get; }

        Task<UpdateResult> CheckForUpdates(bool manualCheck);

        void ManualReinstall();

        string GetApplicationReference();

        event EventHandler UpdateStateChange;
    }

    public class VersionControl : IVersionControl
    {
        public event EventHandler UpdateStateChange;

        public event EventHandler<bool> UpdateCheckOccured;

        public InstanceType InstanceType { get; }
        public string VersionName { get; private set; }
        public string AppName { get; }
        public bool UpdateInstalled { get; private set; }
        public bool UpdateReinstallNeeded { get; private set; }
        public bool UpdateError => updateErrorCount >= 3;

        private DateTime lastUpdateCheck;
        private int updateErrorCount;
        private long updateCheckOngoing;

        public bool IsAutomatedDeploy => ApplicationDeployment.IsNetworkDeployed;
        public Version DeployedVersion => IsAutomatedDeploy ? ApplicationDeployment.CurrentDeployment.CurrentVersion : Assembly.GetExecutingAssembly().GetName().Version;
        public bool IsFirstRun => ApplicationDeployment.CurrentDeployment.IsFirstRun;

        public VersionControl(InstanceType instanceType)
        {
            InstanceType = instanceType;
            lastUpdateCheck = DateTime.MinValue;
            updateErrorCount = 0;
            Interlocked.Exchange(ref updateCheckOngoing, -1);

            SetVersionName();

            var instance = InstanceType == InstanceType.Stable ? "" : $" ({InstanceType})";
            AppName = $"Gallifrey{instance}";
        }

        private void SetVersionName()
        {
            var betaText = InstanceType == InstanceType.Stable ? "" : $" ({InstanceType})";

            if (!IsAutomatedDeploy)
            {
                betaText = " (Debug)";
            }

            VersionName = $"v{DeployedVersion}{betaText}";
        }

        public Task<UpdateResult> CheckForUpdates(bool manualCheck = false)
        {
            if (!IsAutomatedDeploy)
            {
                return Task.Run(() => UpdateResult.NotDeployable);
            }

            if (lastUpdateCheck >= DateTime.UtcNow.AddMinutes(-3) && !manualCheck)
            {
                return Task.Run(() => UpdateResult.TooSoon);
            }

            if (Interlocked.Exchange(ref updateCheckOngoing, 1) != -1 && !manualCheck)
            {
                return Task.Run(() => UpdateResult.TooSoon);
            }

            try
            {
                UpdateCheckOccured?.Invoke(this, manualCheck);
                lastUpdateCheck = DateTime.UtcNow;

                try
                {
                    if (ApplicationDeployment.CurrentDeployment.CheckForUpdate(false))
                    {
                        return Task.Run(() => ApplicationDeployment.CurrentDeployment.Update()).ContinueWith(task =>
                        {
                            SetVersionName();
                            UpdateInstalled = true;
                            UpdateReinstallNeeded = false;
                            UpdateStateChange?.Invoke(this, null);
                            updateErrorCount = 0;
                            return UpdateResult.Updated;
                        });
                    }

                    return Task.Run(() =>
                    {
                        //If manual put a delay in here...the UI goes all weird if it's not
                        if (manualCheck)
                        {
                            UpdateReinstallNeeded = false;
                            updateErrorCount = 0;
                            Task.Delay(TimeSpan.FromSeconds(2));
                        }

                        return UpdateResult.NoUpdate;
                    });
                }
                catch (TrustNotGrantedException)
                {
                    UpdateReinstallNeeded = true;
                    UpdateStateChange?.Invoke(this, null);
                    updateErrorCount += 1;
                    return Task.Run(() => UpdateResult.ReinstallNeeded);
                }
                catch (Exception)
                {
                    try
                    {
                        using (var client = new WebClient())
                        {
                            client.Headers.Add(HttpRequestHeader.UserAgent, $"Gallifrey-{VersionName}");
                            client.DownloadData("https://gallifrey-releases.blyth.me.uk");
                        }

                        UpdateStateChange?.Invoke(this, null);
                        updateErrorCount += 1;
                        throw;
                    }
                    catch
                    {
                        return Task.Run(() => UpdateResult.NoInternet);
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref updateCheckOngoing, -1);
            }
        }

        public void ManualReinstall()
        {
            var installUrl = ApplicationDeployment.CurrentDeployment.UpdateLocation.AbsoluteUri;
            DeploymentUtils.Uninstaller.UninstallMe();
            DeploymentUtils.Uninstaller.AutoInstall(installUrl);
        }

        public string GetApplicationReference()
        {
            var applicationReferencePath = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gallifrey", $"{AppName}.appref-ms"));

            // Only bother generating the .appref-ms file if we haven't done it before.
            var downloadReference = false;
            if (!applicationReferencePath.Exists)
            {
                downloadReference = true;
            }
            else if (applicationReferencePath.CreationTimeUtc < DateTime.UtcNow.AddDays(-7))
            {
                applicationReferencePath.Delete();
                downloadReference = true;
            }

            if (downloadReference)
            {
                XDocument doc;

                // Download the application manifest.
                using (var client = new WebClient())
                {
                    client.Headers.Add(HttpRequestHeader.UserAgent, $"Gallifrey-{VersionName}");
                    var applicationManifest = client.DownloadString(ApplicationDeployment.CurrentDeployment.UpdateLocation.AbsoluteUri);
                    doc = XDocument.Parse(applicationManifest);
                }

                // Build the .appref-ms contents.
                XNamespace asmv1 = "urn:schemas-microsoft-com:asm.v1";
                XNamespace asmv2 = "urn:schemas-microsoft-com:asm.v2";
                var assembly = doc.Element(asmv1 + "assembly");
                var identity = assembly?.Element(asmv1 + "assemblyIdentity");
                var deployment = assembly?.Element(asmv2 + "deployment");
                var provider = deployment?.Element(asmv2 + "deploymentProvider");
                var codebase = provider?.Attribute("codebase");
                var name = identity?.Attribute("name");
                var language = identity?.Attribute("language");
                var publicKeyToken = identity?.Attribute("publicKeyToken");
                var architecture = identity?.Attribute("processorArchitecture");

                var applicationReference = $"{codebase?.Value}#{name?.Value}, Culture={language?.Value}, PublicKeyToken={publicKeyToken?.Value}, processorArchitecture={architecture?.Value}";

                // Write the .appref-ms file (ensure that it's Unicode encoded).
                File.WriteAllText(applicationReferencePath.FullName, applicationReference, Encoding.Unicode);
            }

            return applicationReferencePath.FullName;
        }
    }
}
