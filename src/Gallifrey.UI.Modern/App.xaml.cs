using Gallifrey.UI.Modern.MainViews;
using Gallifrey.Versions;
using System.Net;

namespace Gallifrey.UI.Modern
{
    public partial class App
    {
        public static void Run(InstanceType instance)
        {
            //Ensure that all TLS protocols are supported
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 |
                                                   SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Ssl3;

            var mainWindow = new MainWindow(instance);
            mainWindow.Show();
        }
    }
}
