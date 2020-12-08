using ControlzEx.Theming;
using Gallifrey.UI.Modern.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Gallifrey.UI.Modern.Helpers
{
    public static class ThemeHelper
    {
        public static bool ChangeTheme(string themeName)
        {
            var theme = ThemeManager.Current.DetectTheme(Application.Current);
            if (theme == null)
            {
                return false;
            }

            var appTheme = theme;

            if (!string.IsNullOrWhiteSpace(themeName))
            {
                appTheme = ThemeManager.Current.GetTheme(themeName) ?? theme;
            }

            if (theme.Name != appTheme.Name)
            {
                ThemeManager.Current.ChangeTheme(Application.Current, appTheme);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static List<AccentThemeModel> GetThemes()
        {
            return ThemeManager.Current.Themes.Select(x => new AccentThemeModel { Name = x.Name, Colour = x.ShowcaseBrush }).OrderBy(x => x.DisplayName).ToList();
        }

        public static string GetCurrentName()
        {
            return ThemeManager.Current.DetectTheme()?.Name;
        }
    }
}
