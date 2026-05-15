using System;
using System.Globalization;
using System.Windows.Data;

namespace Leaf.Controls.Converters
{
    public class MathMinConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (
                values.Length < 2
                || values[0] is not double width
                || values[1] is not double height
                || double.IsNaN(width)
                || double.IsNaN(height)
                || width <= 0
                || height <= 0
            )
            {
                return 12.0;
            }

            var minSize = Math.Min(width, height);
            var scale = 1.0;

            if (values.Length > 2 && values[2] is double ratio && ratio > 0)
            {
                scale = ratio;
            }

            if (parameter is string p && double.TryParse(p, out var parsed) && parsed > 0)
            {
                scale = parsed;
            }

            var fontSize = minSize * scale;
            return Math.Max(0.0, fontSize);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
