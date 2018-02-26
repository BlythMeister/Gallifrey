using System.Collections;
using System.Net;
using System.Windows;
using Gallifrey.UI.Modern.MainViews;
using Gallifrey.Versions;

namespace Gallifrey.UI.Modern
{
    public partial class App
    {
        public static void Run(InstanceType instance, ResourceDictionary resources)
        {
            //Ensure that all TLS protocols are supported
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 |
                                                   SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Ssl3;

            WalkDictionary(resources);
            var mainWindow = new MainWindow(instance);
            mainWindow.Show();
        }

        private static void WalkDictionary(ResourceDictionary resources)
        {
            foreach (DictionaryEntry entry in resources)
            {
            }

            foreach (ResourceDictionary rd in resources.MergedDictionaries)
                WalkDictionary(rd);
        }
    }
}
