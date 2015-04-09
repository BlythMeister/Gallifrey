using System.Windows;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        private readonly MainViewModel viewModel;

        public Settings(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
            InitializeComponent();

            DataContext = new SettingModel(viewModel.Gallifrey.Settings);
        }

        private async void SaveSettings(object sender, RoutedEventArgs e)
        {
            var successfulSave = true;
            var settingsModel = (SettingModel)DataContext;

            settingsModel.UpdateSettings(viewModel.Gallifrey.Settings);

            try
            {
                viewModel.Gallifrey.SaveSettings(settingsModel.JiraSettingsChanged);
            }
            catch (MissingJiraConfigException)
            {
                successfulSave = false;
            }
            catch (JiraConnectionException)
            {
                successfulSave = false;
            }

            if (successfulSave)
            {
                IsOpen = false;
                ThemeHelper.ChangeTheme(settingsModel.Theme);
            }
            else
            {
                await viewModel.MainWindow.ShowMessageAsync("Invalid Jira Configuration", "You Cannot Save With Invalid Jira Configuration.\nTo Save You Have To Have A Valid Connection To Jira");
            }
        }
    }
}
