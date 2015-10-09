using System.Windows;
using MahApps.Metro;

namespace Gallifrey.UI.Modern.Helpers
{
    public static class ThemeHelper
    {
        public static bool ChangeTheme(string themeName, string accentName)
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var appTheme = theme.Item1;
            var accent = theme.Item2;

            if (!string.IsNullOrWhiteSpace(themeName))
            {
                appTheme = ThemeManager.GetAppTheme(string.Format("Base{0}", themeName));
            }

            if (!string.IsNullOrWhiteSpace(accentName))
            {
                accent = ThemeManager.GetAccent(accentName);
            }

            if (theme.Item1 != appTheme || theme.Item2 != accent)
            {
                ThemeManager.ChangeAppStyle(Application.Current, accent, appTheme);
                return true;
            }
            else
            {
                return false;
            }

            
        }
    }
}