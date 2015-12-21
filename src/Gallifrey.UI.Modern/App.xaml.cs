using Gallifrey.UI.Modern.MainViews;
using Gallifrey.Versions;

namespace Gallifrey.UI.Modern
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static void Run(InstanceType instance, AppType appType)
        {
            var mainWindow = new MainWindow(instance, appType);
            mainWindow.Show();
        }
    }
}
