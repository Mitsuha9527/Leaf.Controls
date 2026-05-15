using System.Windows;
using System.Windows.Controls;

namespace Leaf.Controls.CustomControls
{
    public class LeafWindow : Window
    {
        public bool IsMaximizedOnStartup
        {
            get { return (bool)GetValue(IsMaximizedOnStartupProperty); }
            set { SetValue(IsMaximizedOnStartupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsInitAsMaximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMaximizedOnStartupProperty =
            DependencyProperty.Register(
                nameof(IsMaximizedOnStartup),
                typeof(bool),
                typeof(LeafWindow),
                new PropertyMetadata(false)
            );

        public LeafWindow()
        {
            SetResourceReference(StyleProperty, typeof(LeafWindow));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
        }
    }
}
