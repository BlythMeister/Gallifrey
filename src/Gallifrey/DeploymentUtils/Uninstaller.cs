using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gallifrey.DeploymentUtils
{
    public class Uninstaller
    {
        public static void UninstallMe()
        {
            var publicKeyToken = GetPublicKeyToken();
            var uninstallString = GetUninstallString(publicKeyToken, out var displayName);
            var runDLL32 = uninstallString.Substring(0, 12);
            var args = uninstallString.Substring(13);
            Process.Start(runDLL32, args);
            PushUninstallOKButton(displayName);
        }

        public static async void AutoInstall(string applicationUriString)
        {
            var webBrowser = new WebBrowser
            {
                ScriptErrorsSuppressed = true,
                AllowNavigation = true
            };

            webBrowser.Navigate(applicationUriString);

            while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
            {
                await Task.Delay(500);
            }
        }

        private static void PushUninstallOKButton(string displayName)
        {
            var uninstallerWin = FindUninstallerWindow(displayName);
            var OKButton = FindUninstallerOKButton(uninstallerWin);
            Win32Utils.DoButtonClick(OKButton);
        }

        private static IntPtr FindUninstallerOKButton(IntPtr uninstallerWindow)
        {
            var w32 = new Win32Utils();
            var OKButton = IntPtr.Zero;
            while (OKButton == IntPtr.Zero)
            {
                OKButton = w32.SearchForChildWindow(uninstallerWindow, "&OK");
                Thread.Sleep(500);
            }

            return OKButton;
        }

        private static IntPtr FindUninstallerWindow(string displayName)
        {

            var w32 = new Win32Utils();
            var uninstallerWindow = IntPtr.Zero;
            while (uninstallerWindow == IntPtr.Zero)
            {
                uninstallerWindow = w32.SearchForTopLevelWindow(displayName + " Maintenance");
                Thread.Sleep(500);
            }

            return uninstallerWindow;

        }

        private static string GetPublicKeyToken()
        {
            var asi = new ApplicationSecurityInfo(AppDomain.CurrentDomain.ActivationContext);
            var pk = asi.ApplicationId.PublicKeyToken;
            var pkt = new StringBuilder();
            for (var i = 0; i < pk.GetLength(0); i++)
                pkt.Append($"{pk[i]:x}");
            return pkt.ToString();
        }


        private static string GetUninstallString(string PublicKeyToken, out string DisplayName)
        {
            string uninstallString = null;
            var searchString = "PublicKeyToken=" + PublicKeyToken;
            var uninstallKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
            var appKeyNames = uninstallKey.GetSubKeyNames();
            DisplayName = null;
            foreach (var appKeyName in appKeyNames)
            {
                var appKey = uninstallKey.OpenSubKey(appKeyName);
                uninstallString = (string)appKey.GetValue("UninstallString");
                DisplayName = (string)appKey.GetValue("DisplayName");
                appKey.Close();
                if (uninstallString.Contains(searchString))
                    break;
            }
            uninstallKey.Close();
            return uninstallString;
        }
    }
}
