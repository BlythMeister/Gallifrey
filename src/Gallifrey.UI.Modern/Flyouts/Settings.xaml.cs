using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using System.Windows;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class Settings
    {
        private readonly ModelHelpers modelHelpers;
        private readonly ProgressDialogHelper progressDialogHelper;
        private SettingModel DataModel => (SettingModel)DataContext;

        public Settings(ModelHelpers modelHelpers)
        {
            this.modelHelpers = modelHelpers;
            InitializeComponent();

            progressDialogHelper = new ProgressDialogHelper(modelHelpers);
            DataContext = new SettingModel(modelHelpers.Gallifrey.Settings, modelHelpers.Gallifrey.VersionControl);
        }

        private async void SaveSettings(object sender, RoutedEventArgs e)
        {
            var successfulSave = true;
            DataModel.UpdateSettings(modelHelpers.Gallifrey.Settings, modelHelpers.Gallifrey.VersionControl);

            try
            {
                if (DataModel.JiraSettingsChanged)
                {
                    var trackingOptOut = DataModel.TrackingOptOut;
                    await progressDialogHelper.Do(() => modelHelpers.Gallifrey.SaveSettings(true, trackingOptOut), "Checking Jira Credentials", false, true);
                }
                else
                {
                    modelHelpers.Gallifrey.SaveSettings(false, DataModel.TrackingOptOut);
                }
            }
            catch (MissingJiraConfigException)
            {
                successfulSave = false;
            }
            catch (JiraConnectionException)
            {
                successfulSave = false;
            }
            catch (TempoConnectionException)
            {
                successfulSave = false;
            }

            if (successfulSave)
            {
                modelHelpers.CloseFlyout(this);
                ThemeHelper.ChangeTheme(DataModel.Theme.Name);
            }
            else
            {
                if (DataModel.UseTempo)
                {
                    await modelHelpers.ShowMessageAsync("Invalid Jira Or Tempo Configuration", "You Cannot Save With Invalid Jira Or Tempo Configuration.\nTo Save You Have To Have A Valid Connection To Jira And Tempo");
                }
                else
                {
                    await modelHelpers.ShowMessageAsync("Invalid Jira Configuration", "You Cannot Save With Invalid Jira Configuration.\nTo Save You Have To Have A Valid Connection To Jira");
                }

                Focus();
            }
        }
    }
}
