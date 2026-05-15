using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Leaf.Controls.Converters
{
    internal class EnumBooleanConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || parameter is null)
                return false;
            var parameterString = parameter.ToString();
            if (Enum.IsDefined(value.GetType(), value))
            {
                object paramValue = Enum.Parse(value.GetType(), parameterString!);
                return paramValue.Equals(value);
            }
            return false;
        }

        public object? ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (parameter == null)
                return Binding.DoNothing;
            string? parameterString = parameter.ToString();
            return Enum.Parse(targetType, parameterString!);
        }
    }
}
