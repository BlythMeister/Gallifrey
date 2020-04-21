using MahApps.Metro;
using System.Windows;

namespace Gallifrey.UI.Modern.Helpers
{
    public static class ThemeHelper
    {
        public static bool ChangeTheme(string themeName)
        {
            var theme = ThemeManager.DetectTheme(Application.Current);
            if (theme == null) return false;

            var appTheme = theme;

            if (!string.IsNullOrWhiteSpace(themeName))
            {
                appTheme = ThemeManager.GetTheme(themeName) ?? theme;
            }

            if (theme.Name != appTheme.Name)
            {
                ThemeManager.ChangeTheme(Application.Current, appTheme);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
