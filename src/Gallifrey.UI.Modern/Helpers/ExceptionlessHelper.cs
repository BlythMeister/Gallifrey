using Exceptionless;
using Exceptionless.Models.Data;
using Gallifrey.Settings;
using Gallifrey.Versions;
using System;
using System.Windows;
using Error = Gallifrey.UI.Modern.Flyouts.Error;

namespace Gallifrey.UI.Modern.Helpers
{
    public class ExceptionlessHelper
    {
        private readonly ModelHelpers modelHelpers;
        private bool registered;

        public ExceptionlessHelper(ModelHelpers modelHelpers)
        {
            this.modelHelpers = modelHelpers;
            registered = false;
            modelHelpers.Gallifrey.DailyTrackingEvent += GallifreyOnDailyTrackingEvent;
        }

        public void RegisterExceptionless()
        {
            if (!modelHelpers.Gallifrey.VersionControl.IsAutomatedDeploy || string.IsNullOrWhiteSpace(ConfigKeys.ExceptionlessApiKey))
            {
                return;
            }

            var userInfo = new UserInfo(modelHelpers.Gallifrey.Settings.InstallationHash, "Unknown");
            if (modelHelpers.Gallifrey.JiraConnection.IsConnected)
            {
                if (!string.IsNullOrWhiteSpace(modelHelpers.Gallifrey.JiraConnection.CurrentUser.displayName))
                {
                    userInfo.Name = modelHelpers.Gallifrey.JiraConnection.CurrentUser.displayName;
                }
                else if (!string.IsNullOrWhiteSpace(modelHelpers.Gallifrey.JiraConnection.CurrentUser.name))
                {
                    userInfo.Name = modelHelpers.Gallifrey.JiraConnection.CurrentUser.name;
                }
                else
                {
                    userInfo.Name = modelHelpers.Gallifrey.Settings.JiraConnectionSettings.JiraUsername;
                }

                userInfo.Data.Add("Jira-DisplayName", modelHelpers.Gallifrey.JiraConnection.CurrentUser.displayName);
                userInfo.Data.Add("Jira-Name", modelHelpers.Gallifrey.JiraConnection.CurrentUser.name);
                userInfo.Data.Add("Jira-EmailAddress", modelHelpers.Gallifrey.JiraConnection.CurrentUser.emailAddress);
                userInfo.Data.Add("Jira-AccountId", modelHelpers.Gallifrey.JiraConnection.CurrentUser.accountId);
            }
            else
            {
                userInfo.Name = modelHelpers.Gallifrey.Settings.JiraConnectionSettings.JiraUsername;
            }

            CloseExceptionless();

            ExceptionlessClient.Default.Configuration.ApiKey = ConfigKeys.ExceptionlessApiKey;
            ExceptionlessClient.Default.Configuration.DefaultTags.Add(modelHelpers.Gallifrey.VersionControl.VersionName.Replace("\n", " - "));
            ExceptionlessClient.Default.Configuration.SetVersion(modelHelpers.Gallifrey.VersionControl.DeployedVersion);
            ExceptionlessClient.Default.Configuration.IncludeIpAddress = true;
            ExceptionlessClient.Default.Configuration.IncludeUserName = true;
            ExceptionlessClient.Default.Configuration.IncludeMachineName = true;
            ExceptionlessClient.Default.Configuration.IncludePrivateInformation = false;
            ExceptionlessClient.Default.Configuration.SetUserIdentity(userInfo);
            ExceptionlessClient.Default.Configuration.UseSessions(true, TimeSpan.FromMinutes(30), true);
            ExceptionlessClient.Default.Configuration.Enabled = true;

            ExceptionlessClient.Default.SubmittingEvent += ExceptionlessSubmittingEvent;

            ExceptionlessClient.Default.Startup(ConfigKeys.ExceptionlessApiKey);

            registered = true;
            //Prevent the framework from auto closing the app and let exceptionless handle errors
            Application.Current.Dispatcher.UnhandledException += (sender, args) => args.Handled = true;
        }

        public void CloseExceptionless()
        {
            if (registered)
            {
                ExceptionlessClient.Default.Shutdown();
                registered = false;
            }
        }

        public void TrackFeature(string feature)
        {
            var featureName = $"Gallifrey v{modelHelpers.Gallifrey.Settings.InternalSettings.LastChangeLogVersion}";

            if (modelHelpers.Gallifrey.VersionControl.IsAutomatedDeploy)
            {
                if (modelHelpers.Gallifrey.VersionControl.InstanceType != InstanceType.Stable)
                {
                    featureName = $"{featureName} ({modelHelpers.Gallifrey.VersionControl.InstanceType})";
                }
            }
            else
            {
                featureName = $"{featureName} (Debug)";
            }

            featureName = $"{featureName} - {feature}";

            ExceptionlessClient.Default.SubmitFeatureUsage(featureName);
        }

        private async void ExceptionlessSubmittingEvent(object sender, EventSubmittingEventArgs e)
        {
            if (!registered)
            {
                return;
            }

            if (e.IsUnhandledError)
            {
                e.Cancel = true;

                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    modelHelpers.CloseAllFlyouts();

                    var resetTopMostSetting = false;
                    if (!modelHelpers.Gallifrey.Settings.UiSettings.TopMostOnFlyoutOpen)
                    {
                        modelHelpers.Gallifrey.Settings.UiSettings.TopMostOnFlyoutOpen = true;
                        resetTopMostSetting = true;
                    }

                    await modelHelpers.OpenFlyout(new Error(modelHelpers, e.Event));

                    if (resetTopMostSetting)
                    {
                        modelHelpers.Gallifrey.Settings.UiSettings.TopMostOnFlyoutOpen = false;
                    }

                    modelHelpers.CloseApp(true);
                });
            }
            else if (e.Event.IsError() || e.Event.IsCritical())
            {
                e.Event.AddTags("Handled");
            }
        }

        private void GallifreyOnDailyTrackingEvent(object sender, EventArgs eventArgs)
        {
            if (!registered)
            {
                return;
            }

            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TrackFeature("Daily Event");
                });
            }
            catch (Exception ex)
            {
                ExceptionlessClient.Default.CreateEvent().SetException(ex).AddTags("Events").Submit();
            }
        }
    }
}
