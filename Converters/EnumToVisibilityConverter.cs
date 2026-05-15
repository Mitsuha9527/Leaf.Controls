using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Leaf.Controls.Converters
{
    internal class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture
        )
        {
            if (value is null || parameter is null)
                return Visibility.Collapsed;
            var parameterString = parameter.ToString();
            if (Enum.IsDefined(value.GetType(), value))
            {
                object paramValue = Enum.Parse(value.GetType(), parameterString!);
                return paramValue.Equals(value) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }
}
