using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;

namespace Leaf.Controls.CustomControls
{
    internal class TitleBarButton : Button
    {
        static TitleBarButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TitleBarButton),
                new FrameworkPropertyMetadata(typeof(TitleBarButton))
            );
        }

        public static readonly DependencyProperty ButtonTypeProperty = DependencyProperty.Register(
            nameof(ButtonType),
            typeof(TitleBarButtonType),
            typeof(TitleBarButton),
            new PropertyMetadata(TitleBarButtonType.Unknown)
        );

        public TitleBarButtonType ButtonType
        {
            get { return (TitleBarButtonType)GetValue(ButtonTypeProperty); }
            set { SetValue(ButtonTypeProperty, value); }
        }

        public TitleBarButton()
        {
            SetResourceReference(StyleProperty, typeof(TitleBarButton));
        }
    }
}
