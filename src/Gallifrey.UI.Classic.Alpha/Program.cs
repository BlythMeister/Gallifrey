using System;
using System.Windows.Forms;
using Gallifrey.Versions;

namespace Gallifrey.UI.Classic.Alpha
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
            Application.Run(new MainWindow(new Backend(InstanceType.Alpha, AppType.Classic)));
        }
    }
}
