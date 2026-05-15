using Leaf.Controls.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Leaf.Controls.CustomControls
{
    /// <summary>
    /// 状态指示标签控件，用于显示三态（True/False/Null）的圆形指示灯和文本
    /// </summary>
    public class ToggleLabel : Control
    {
        static ToggleLabel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ToggleLabel),
                new FrameworkPropertyMetadata(typeof(ToggleLabel))
            );
        }

        public ToggleLabel()
        {
            SetResourceReference(StyleProperty, typeof(ToggleLabel));
        }

        #region Dependency Properties

        /// <summary>
        /// 状态值（true/false/null）
        /// </summary>
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
            nameof(State),
            typeof(bool?),
            typeof(ToggleLabel),
            new PropertyMetadata(null, OnStateChanged)
        );

        public bool? State
        {
            get => (bool?)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ToggleLabel)d;
            control.UpdateVisualState();
        }

        /// <summary>
        /// 状态为 True 时显示的文本
        /// </summary>
        public static readonly DependencyProperty TrueTextProperty = DependencyProperty.Register(
            nameof(TrueText),
            typeof(string),
            typeof(ToggleLabel),
            new PropertyMetadata("True")
        );

        public string TrueText
        {
            get => (string)GetValue(TrueTextProperty);
            set => SetValue(TrueTextProperty, value);
        }

        /// <summary>
        /// 状态为 False 时显示的文本
        /// </summary>
        public static readonly DependencyProperty FalseTextProperty = DependencyProperty.Register(
            nameof(FalseText),
            typeof(string),
            typeof(ToggleLabel),
            new PropertyMetadata("False")
        );

        public string FalseText
        {
            get => (string)GetValue(FalseTextProperty);
            set => SetValue(FalseTextProperty, value);
        }

        /// <summary>
        /// 状态为 Null 时显示的文本
        /// </summary>
        public static readonly DependencyProperty NullTextProperty = DependencyProperty.Register(
            nameof(NullText),
            typeof(string),
            typeof(ToggleLabel),
            new PropertyMetadata("Unknown")
        );

        public string NullText
        {
            get => (string)GetValue(NullTextProperty);
            set => SetValue(NullTextProperty, value);
        }

        /// <summary>
        /// 指示灯大小
        /// </summary>
        public static readonly DependencyProperty IndicatorSizeProperty = DependencyProperty.Register(
            nameof(IndicatorSize),
            typeof(double),
            typeof(ToggleLabel),
            new PropertyMetadata(12.0)
        );

        public double IndicatorSize
        {
            get => (double)GetValue(IndicatorSizeProperty);
            set => SetValue(IndicatorSizeProperty, value);
        }

        /// <summary>
        /// 指示灯与文本之间的间距
        /// </summary>
        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
            nameof(Spacing),
            typeof(double),
            typeof(ToggleLabel),
            new PropertyMetadata(6.0)
        );

        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        /// <summary>
        /// 当前显示的文本（只读，根据 State 自动计算）
        /// </summary>
        private static readonly DependencyPropertyKey CurrentTextPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(CurrentText),
            typeof(string),
            typeof(ToggleLabel),
            new PropertyMetadata(string.Empty)
        );

        public static readonly DependencyProperty CurrentTextProperty = CurrentTextPropertyKey.DependencyProperty;

        public string CurrentText
        {
            get => (string)GetValue(CurrentTextProperty);
            private set => SetValue(CurrentTextPropertyKey, value);
        }

        /// <summary>
        /// 当前指示灯的画刷（只读，根据 State 自动计算）
        /// </summary>
        private static readonly DependencyPropertyKey IndicatorBrushPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IndicatorBrush),
            typeof(Brush),
            typeof(ToggleLabel),
            new PropertyMetadata(Brushes.Gray)
        );

        public static readonly DependencyProperty IndicatorBrushProperty = IndicatorBrushPropertyKey.DependencyProperty;

        public Brush IndicatorBrush
        {
            get => (Brush)GetValue(IndicatorBrushProperty);
            private set => SetValue(IndicatorBrushPropertyKey, value);
        }

        #endregion

        #region Private Methods

        private void UpdateVisualState()
        {
            if (State == true)
            {
                CurrentText = TrueText;
                IndicatorBrush = TryFindResource("SuccessBrush") as Brush ?? Brushes.Green;
            }
            else if (State == false)
            {
                CurrentText = FalseText;
                IndicatorBrush = TryFindResource("DangerBrush") as Brush ?? Brushes.Red;
            }
            else
            {
                CurrentText = NullText;
                IndicatorBrush = TryFindResource("ThirdlyTextBrush") as Brush ?? Brushes.Gray;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateVisualState();
        }

        #endregion
    }
}
