using System.Globalization;
using System.Windows.Data;

namespace Leaf.Controls.Converters
{
    public class BoolToOKNGConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "OK" : "NG";
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                return strValue.Equals("OK", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}