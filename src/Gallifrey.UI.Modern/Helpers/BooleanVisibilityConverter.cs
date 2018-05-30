using System;
using System.Windows;
using System.Windows.Data;

namespace Gallifrey.UI.Modern.Helpers
{
    class BooleanVisibilityConverter : IValueConverter
    {
        //Use collapse or hidden
        public bool Collapse { get; set; }
        //invert to false = visible
        public bool Invert { get; set; }

        //True = Visible
        //False = Collapse/Hidden
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var bValue = value != null && (bool)value;

            if (Invert)
            {
                bValue = !bValue;
            }

            if (bValue)
            {
                return Visibility.Visible;
            }

            return Collapse ? Visibility.Collapsed : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null && ((Visibility)value) == Visibility.Visible;
        }
    }
}
