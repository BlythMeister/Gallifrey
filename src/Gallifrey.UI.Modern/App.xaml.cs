using System.Windows;
using Gallifrey.UI.Modern.MainViews;
using Gallifrey.Versions;

namespace Gallifrey.UI.Modern
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = new MainWindow(new Backend(InstanceType.Stable, AppType.Modern));
            mainWindow.Show();
        }
    }
}
