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
        private SettingModel DataModel { get { return (SettingModel)DataContext; } }

        public Settings(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
            InitializeComponent();

            DataContext = new SettingModel(viewModel.Gallifrey.Settings);
        }

        private async void SaveSettings(object sender, RoutedEventArgs e)
        {
            var successfulSave = true;
            DataModel.UpdateSettings(viewModel.Gallifrey.Settings);

            try
            {
                viewModel.Gallifrey.SaveSettings(DataModel.JiraSettingsChanged);
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
                ThemeHelper.ChangeTheme(DataModel.Theme);
            }
            else
            {
                await DialogCoordinator.Instance.ShowMessageAsync(viewModel,"Invalid Jira Configuration", "You Cannot Save With Invalid Jira Configuration.\nTo Save You Have To Have A Valid Connection To Jira");
            }
        }
    }
}
