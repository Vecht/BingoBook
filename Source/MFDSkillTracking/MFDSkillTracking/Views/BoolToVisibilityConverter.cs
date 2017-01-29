using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MFDSkillTracking.Views
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return DependencyProperty.UnsetValue;
            var b = (bool)value;

            return b ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
