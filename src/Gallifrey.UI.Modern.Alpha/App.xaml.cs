using System.Windows;
using Gallifrey.Versions;

namespace Gallifrey.UI.Modern.Alpha
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Modern.App.Run(InstanceType.Alpha);
        }
    }
}
