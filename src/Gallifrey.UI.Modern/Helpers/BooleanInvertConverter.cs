using System;
using System.Windows.Data;

namespace Gallifrey.UI.Modern.Helpers
{
    internal class BooleanInvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null && !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null && !(bool)value;
        }
    }
}
