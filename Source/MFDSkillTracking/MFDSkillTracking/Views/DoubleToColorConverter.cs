using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MFDSkillTracking.Views
{
    public class DoubleToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double)) return DependencyProperty.UnsetValue;
            var d = (double)value;

            var r = d >= 100 ? (byte)115
                : (d < 100 && d >= 90) ? Linear(115, 172, 100, 90, d)
                : (d < 90 && d >= 80) ? Linear(172, 230, 90, 80, d)
                : (d < 80 && d >= 70) ? Linear(230, 255, 80, 70, d)
                : (d < 70 && d >= 65) ? Linear(255, 255, 70, 65, d)
                : (d < 65 && d >= 58) ? Linear(255, 230, 65, 58, d)
                : (d < 58 && d >= 50) ? Linear(230, 230, 58, 50, d)
                : (d < 50 && d >= 0) ? Linear(230, 150, 50, 0, d)
                : (byte)150;

            var g = d >= 80 ? (byte)230
                : (d < 80 && d >= 70) ? Linear(230, 204, 80, 70, d)
                : (d < 70 && d >= 65) ? Linear(204, 153, 70, 65, d)
                : (d < 65 && d >= 58) ? Linear(153, 92, 65, 58, d)
                : (d < 58 && d >= 50) ? Linear(92, 0, 58, 50, d)
                : (byte)0;

            return new SolidColorBrush(Color.FromRgb(r, g, 0));
        }

        private byte Linear(double from, double to, double start, double end, double input)
        {
            var scale = (start - input)/(start - end);
            var scaledDist = (from - to) * scale;
            var output = from - scaledDist;
            return (byte)Math.Round(output, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
