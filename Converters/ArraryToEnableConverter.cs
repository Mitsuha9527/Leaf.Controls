using System.Collections;
using System.Windows.Data;

namespace Leaf.Controls.Converters
{
    internal class ArraryToEnableConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is IEnumerable enumerable)
            {
                return enumerable.Cast<object>().Any();
            }
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
