using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Leaf.Controls.CustomControls
{
    public class CheckBoxAttach
    {
        public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.RegisterAttached(
            "Background",
            typeof(Brush),
            typeof(CheckBoxAttach),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.Inherits)
        );

        public static void SetBackground(DependencyObject element, Brush value) =>
            element.SetValue(BackgroundProperty, value);

        public static Brush GetBackground(DependencyObject element) =>
            (Brush)element.GetValue(BackgroundProperty);
    }
}
