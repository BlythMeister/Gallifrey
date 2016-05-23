using Gallifrey.UI.MainViews;
using Gallifrey.Versions;

namespace Gallifrey.UI
{
    public partial class App
    {
        public static void Run(InstanceType instance)
        {
            var mainWindow = new MainWindow(instance);
            mainWindow.Show();
        }
    }
}
