using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Ham2.utility
{
    public class BooleanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(targetType == typeof(string)) || !(value is bool))
            {
                throw new InvalidOperationException();
            }

            var p = "Yes/No";
            if (!string.IsNullOrEmpty(parameter as string))
            {
                p = parameter as string;
            }

            var options = p.Split('/');
            if (options.Length != 2)
            {
                throw new InvalidOperationException("Parameter format invalid.");
            }

            return ((bool)value) ? options[0] : options[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class BooleanToBrushConverter : IValueConverter
    {
        private Color GetColor(string v)
        {
            string alpha, red, green, blue;
            alpha = "FF";

            if (v.Length == 8)
            {
                alpha = v.Remove(2);
                v = v.Remove(0, 2);
            }
            if (v.Length != 6)
            {
                throw new InvalidOperationException("Parameter format invalid");
            }

            red = v.Remove(2);
            v = v.Remove(0, 2);
            green = v.Remove(2);
            v = v.Remove(0, 2);
            blue = v;

            var color = new Color()
            {
                A = System.Convert.ToByte(alpha, 16),
                R = System.Convert.ToByte(red, 16),
                G = System.Convert.ToByte(green, 16),
                B = System.Convert.ToByte(blue, 16)
            };

            return color;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush) || !(value is bool))
            {
                throw new InvalidOperationException();
            }

            var p = "00FF00/FF0000";
            if (!string.IsNullOrEmpty(parameter as string))
            {
                p = parameter as string;
            }

            var options = p.Split('/');
            if (options.Length != 2)
            {
                throw new InvalidOperationException("Parameter format invalid");
            }

            return new SolidColorBrush(GetColor(((bool)value) ? options[0] : options[1]));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}