using System.Windows;
using MahApps.Metro;

namespace Gallifrey.UI.Modern.Helpers
{
    public enum ThemeChangeDetail
    {
        Both,
        Accent,
        Theme,
        None
    }

    public static class ThemeHelper
    {
        public static ThemeChangeDetail ChangeTheme(string themeName, string accentName)
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var appTheme = theme.Item1;
            var accent = theme.Item2;

            if (!string.IsNullOrWhiteSpace(themeName))
            {
                appTheme = ThemeManager.GetAppTheme($"Base{themeName}") ?? theme.Item1;
            }

            if (!string.IsNullOrWhiteSpace(accentName))
            {
                accent = ThemeManager.GetAccent(accentName) ?? theme.Item2;
            }

            if (theme.Item1 != appTheme || theme.Item2 != accent)
            {
                var changeDetail = ThemeChangeDetail.Both;
                if (theme.Item1 == appTheme)
                {
                    changeDetail = ThemeChangeDetail.Accent;
                }
                if (theme.Item2 == accent)
                {
                    changeDetail = ThemeChangeDetail.Theme;
                }

                ThemeManager.ChangeAppStyle(Application.Current, accent, appTheme);

                return changeDetail;
            }
            else
            {
                return ThemeChangeDetail.None;
            }
        }
    }
}