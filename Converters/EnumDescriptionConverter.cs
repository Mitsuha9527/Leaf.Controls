using System.ComponentModel;
using System.Windows.Data;

namespace Leaf.Controls.Converters
{
    public class EnumDescriptionConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
            var type = value.GetType();
            if (!type.IsEnum)
                return string.Empty;
            var name = Enum.GetName(type, value);
            if (name == null)
                return string.Empty;
            var field = type.GetField(name);
            if (field == null)
                return string.Empty;
            var attrs = field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            if (attrs.Length > 0)
            {
                var descAttr = (System.ComponentModel.DescriptionAttribute)attrs[0];
                return descAttr.Description;
            }
            return name;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !targetType.IsEnum)
                return Binding.DoNothing;

            var description = value.ToString();
            if (string.IsNullOrEmpty(description))
                return Binding.DoNothing;

            // 遍历枚举的所有字段，查找匹配的Description
            foreach (var field in targetType.GetFields())
            {
                if (field.IsSpecialName)
                    continue;

                var attrs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs.Length > 0)
                {
                    var descAttr = (DescriptionAttribute)attrs[0];
                    if (descAttr.Description == description)
                    {
                        return Enum.Parse(targetType, field.Name);
                    }
                }
                // 如果没有Description特性，则直接比较名称
                else if (field.Name == description)
                {
                    return Enum.Parse(targetType, field.Name);
                }
            }

            return Binding.DoNothing;
        }
    }
}
