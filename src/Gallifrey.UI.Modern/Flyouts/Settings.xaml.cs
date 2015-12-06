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
                var themeChanged = ThemeHelper.ChangeTheme(DataModel.Theme, DataModel.Accent);

                if (themeChanged == ThemeChangeDetail.Theme || themeChanged == ThemeChangeDetail.Both)
                {
                    //This is a really ugly solution!!
                    //The overides of system colours do not update automatically which is not good.
                    //This message will hopefully make people restart...
                    await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "Restart Needed", "When Changing Theme, Some Colours Will Not Change Automatically\nIt Is Recommended To Restart The App");
                }
            }
            else
            {
                await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext,"Invalid Jira Configuration", "You Cannot Save With Invalid Jira Configuration.\nTo Save You Have To Have A Valid Connection To Jira");
            }
        }
    }
}
