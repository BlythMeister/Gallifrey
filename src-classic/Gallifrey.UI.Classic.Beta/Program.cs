using System;
using System.Windows.Forms;
using Gallifrey.Versions;

namespace Gallifrey.UI.Classic.Beta
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow(new Backend(InstanceType.Beta, AppType.Classic)));
        }
    }
}
