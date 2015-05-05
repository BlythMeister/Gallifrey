using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using Microsoft.Win32;

namespace Gallifrey.DeploymentUtils
{
    public class Uninstaller
    {

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        /// <summary>
        /// Uninstalls the current ClickOnce app.
        /// </summary>
        public static void UninstallMe()
        {
            //System.Diagnostics.Debugger.Break();
            string publicKeyToken = GetPublicKeyToken();
            // Find Uninstall string in registry    
            string DisplayName;
            string uninstallString = GetUninstallString(publicKeyToken, out DisplayName);
            string runDLL32 = uninstallString.Substring(0, 12);
            string args = uninstallString.Substring(13);
            Process uninstallProcess = Process.Start(runDLL32, args);
            PushUninstallOKButton(DisplayName);
        }

        private static void PushUninstallOKButton(string DisplayName)
        {
            IntPtr uninstallerWin = FindUninstallerWindow(DisplayName);
            IntPtr OKButton = FindUninstallerOKButton(uninstallerWin);
            Win32Utils.DoButtonClick(OKButton);
        }

        private static IntPtr FindUninstallerOKButton(IntPtr UninstallerWindow)
        {
            Win32Utils w32 = new Win32Utils();
            IntPtr OKButton = IntPtr.Zero;
            while (OKButton == IntPtr.Zero)
            {
                OKButton = w32.SearchForChildWindow(UninstallerWindow, "&OK");
                System.Threading.Thread.Sleep(500);
            }

            return OKButton;
        }

        private static IntPtr FindUninstallerWindow(string DisplayName)
        {

            Win32Utils w32 = new Win32Utils();
            IntPtr uninstallerWindow = IntPtr.Zero;
            while (uninstallerWindow == IntPtr.Zero)
            {
                uninstallerWindow = w32.SearchForTopLevelWindow(DisplayName + " Maintenance");
                System.Threading.Thread.Sleep(500);
            }

            return uninstallerWindow;

        }

        /// <summary>
        /// Gets the public key token for the current ClickOnce app.
        /// </summary>
        /// <returns></returns>
        private static string GetPublicKeyToken()
        {
            ApplicationSecurityInfo asi = new ApplicationSecurityInfo(AppDomain.CurrentDomain.ActivationContext);
            byte[] pk = asi.ApplicationId.PublicKeyToken;
            StringBuilder pkt = new StringBuilder();
            for (int i = 0; i < pk.GetLength(0); i++)
                pkt.Append(String.Format("{0:x}", pk[i]));
            return pkt.ToString();
        }

        /// <summary>
        /// Gets the uninstall string for the current ClickOnce app from the Windows 
        /// Registry.
        /// </summary>
        /// <param name="PublicKeyToken">The public key token of the app.</param>
        /// <param name="DisplayName"></param>
        /// <returns>The command line to execute that will uninstall the app.</returns>
        private static string GetUninstallString(string PublicKeyToken, out string DisplayName)
        {
            string uninstallString = null;
            string searchString = "PublicKeyToken=" + PublicKeyToken;
            RegistryKey uninstallKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
            string[] appKeyNames = uninstallKey.GetSubKeyNames();
            DisplayName = null;
            foreach (string appKeyName in appKeyNames)
            {
                RegistryKey appKey = uninstallKey.OpenSubKey(appKeyName);
                uninstallString = (string)appKey.GetValue("UninstallString");
                DisplayName = (string)appKey.GetValue("DisplayName");
                appKey.Close();
                if (uninstallString.Contains(searchString))
                    break;
            }
            uninstallKey.Close();
            return uninstallString;
        }

        public static void AutoInstall(string ApplicationUriString)
        {
            Process.Start("iexplore.exe", ApplicationUriString);
        }
    }
}
