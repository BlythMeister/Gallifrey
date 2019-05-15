using Gallifrey.Versions;
using System.Windows;

namespace Gallifrey.UI.Modern.Beta
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Modern.App.Run(InstanceType.Beta);
        }
    }
}
