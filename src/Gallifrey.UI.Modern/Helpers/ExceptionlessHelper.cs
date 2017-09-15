using Exceptionless;
using Gallifrey.Settings;
using Gallifrey.UI.Modern.Flyouts;
using Gallifrey.Versions;
using System;
using System.Windows;

namespace Gallifrey.UI.Modern.Helpers
{
    public class ExceptionlessHelper
    {
        private readonly ModelHelpers modelHelpers;

        public ExceptionlessHelper(ModelHelpers modelHelpers)
        {
            this.modelHelpers = modelHelpers;

            modelHelpers.Gallifrey.DailyTrackingEvent += GallifreyOnDailyTrackingEvent;
        }

        public void RegisterExceptionless()
        {
            ExceptionlessClient.Default.Unregister();
            ExceptionlessClient.Default.Configuration.ApiKey = ConfigKeys.ExceptionlessApiKey;
            ExceptionlessClient.Default.Configuration.DefaultTags.Add(modelHelpers.Gallifrey.VersionControl.VersionName.Replace("\n", " - "));
            ExceptionlessClient.Default.Configuration.SetUserIdentity(modelHelpers.Gallifrey.Settings.InstallationHash, modelHelpers.Gallifrey.JiraConnection.IsConnected ? modelHelpers.Gallifrey.JiraConnection.CurrentUser.displayName : "Unknown");
            ExceptionlessClient.Default.Configuration.SessionsEnabled = false;
            ExceptionlessClient.Default.Configuration.Enabled = true;
            ExceptionlessClient.Default.SubmittingEvent += ExceptionlessSubmittingEvent;

            if (modelHelpers.Gallifrey.VersionControl.IsAutomatedDeploy)
            {
                ExceptionlessClient.Default.Register();
                //Prevent the framework from auto closing the app and let exceptionless handle errors
                Application.Current.Dispatcher.UnhandledException += (sender, args) => args.Handled = true;
            }
        }

        private async void ExceptionlessSubmittingEvent(object sender, EventSubmittingEventArgs e)
        {
            if (e.IsUnhandledError)
            {
                e.Cancel = true;

                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    modelHelpers.CloseAllFlyouts();
                    await modelHelpers.OpenFlyout(new Error(modelHelpers, e.Event));
                    modelHelpers.CloseApp(true);
                });
            }
        }

        private void GallifreyOnDailyTrackingEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var featureName = $"Gallifrey v{modelHelpers.Gallifrey.Settings.InternalSettings.LastChangeLogVersion}";
                    if (modelHelpers.Gallifrey.Settings.InternalSettings.IsPremium)
                    {
                        featureName = $"Gallifrey Premium v{modelHelpers.Gallifrey.Settings.InternalSettings.LastChangeLogVersion}";
                    }

                    if (modelHelpers.Gallifrey.VersionControl.IsAutomatedDeploy)
                    {
                        if (modelHelpers.Gallifrey.VersionControl.InstanceType != InstanceType.Stable)
                        {
                            featureName += $" ({modelHelpers.Gallifrey.VersionControl.InstanceType})";
                        }
                    }
                    else
                    {
                        featureName += " (Debug)";
                    }

                    ExceptionlessClient.Default.SubmitFeatureUsage(featureName);
                });
            }
            catch (Exception)
            {
                //suppress errors if tracking fails
            }
        }
    }
}
