using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;

namespace Leaf.Controls.CustomControls
{
    /// <summary>
    /// 为TextBox提供按设定的Key键更新绑定源的附加属性
    /// </summary>
    public class TextBoxAttatch
    {
        /// <summary>
        /// 获取TextBox是否在按下设定的Key键时更新绑定源
        /// </summary>
        public static readonly DependencyProperty UpdateSourceOnKeyProperty =
            DependencyProperty.RegisterAttached(
                "UpdateSourceOnKey",
                typeof(Key),
                typeof(TextBoxAttatch),
                new FrameworkPropertyMetadata(Key.None, OnUpdateSourceOnKeyChanged)
            );

        /// <summary>
        /// 设置TextBox是否在按下设定的Key键时更新绑定源
        /// </summary>
        public static void SetUpdateSourceOnKey(UIElement element, Key value)
        {
            element.SetValue(UpdateSourceOnKeyProperty, value);
        }

        /// <summary>
        /// 获取TextBox是否在按下设定的Key键时更新绑定源
        /// </summary>
        public static Key GetUpdateSourceOnKey(UIElement element)
        {
            return (Key)element.GetValue(UpdateSourceOnKeyProperty);
        }

        private static void OnUpdateSourceOnKeyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            if (d is TextBox textBox)
            {
                textBox.KeyUp-= TextBox_KeyUp;
                textBox.KeyUp += TextBox_KeyUp;
            }
        }

        private static void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (
                sender is TextBox textBox
                && e.Key == (Key)textBox.GetValue(UpdateSourceOnKeyProperty)
            )
            {
                BindingExpression bindingExpression = textBox.GetBindingExpression(
                    TextBox.TextProperty
                );
                bindingExpression?.UpdateSource();
                e.Handled = true;
            }
        }
    }
}
