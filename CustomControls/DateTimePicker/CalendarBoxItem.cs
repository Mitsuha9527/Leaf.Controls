using System.Windows;
using System.Windows.Controls;

namespace Leaf.Controls.CustomControls
{
    public class CalendarBoxItem : ListBoxItem
    {
        public string DateFormat { get; set; } = string.Empty;

        public DateTime Date;
        public bool IsCurrentMonth
        {
            get { return (bool)GetValue(IsCurrentMonthProperty); }
            set { SetValue(IsCurrentMonthProperty, value); }
        }

        public static readonly DependencyProperty IsCurrentMonthProperty =
            DependencyProperty.Register("IsCurrentMonth", typeof(bool), typeof(CalendarBoxItem), new PropertyMetadata(false));


        static CalendarBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarBoxItem), new FrameworkPropertyMetadata(typeof(CalendarBoxItem)));
        }
    }
}
