using System.Windows;
using MahApps.Metro;

namespace Gallifrey.UI.Modern.Helpers
{
    public static class ThemeHelper
    {
        public static void ChangeTheme(string themeName)
        {
            if (!string.IsNullOrWhiteSpace(themeName))
            {
                var theme = ThemeManager.DetectAppStyle(Application.Current);
                var appTheme = ThemeManager.GetAppTheme(string.Format("Base{0}", themeName));
                ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
            }
        }
    }
}