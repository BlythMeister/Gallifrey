using System.Windows;
using Gallifrey.Versions;

namespace Gallifrey.UI.Beta
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            UI.App.Run(InstanceType.Beta);
        }
    }
}
