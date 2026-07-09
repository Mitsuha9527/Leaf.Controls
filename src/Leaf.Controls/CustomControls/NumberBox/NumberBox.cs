using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Leaf.Controls.Utilities;

namespace Leaf.Controls.CustomControls
{
    [TemplatePart(Name = ElementTextBox, Type = typeof(TextBox))]
    public class NumberBox : Control
    {
        private const string ElementTextBox = "PART_TextBox";
        private TextBox _textBox = null!;
        private bool _isInternalChanged;

        public RoutedCommand IncreaseCommand { get; } =
            new RoutedCommand(nameof(IncreaseCommand), typeof(NumberBox));
        public RoutedCommand DecreaseCommand { get; } =
            new RoutedCommand(nameof(DecreaseCommand), typeof(NumberBox));

        public NumberBox()
        {
            SetResourceReference(StyleProperty, typeof(NumberBox));
            CommandBindings.Add(
                new CommandBinding(
                    IncreaseCommand,
                    (s, e) =>
                    {
                        if (IsReadOnly)
                            return;
                        SetCurrentValue(ValueProperty, Value + Increment);
                    }
                )
            );
            CommandBindings.Add(
                new CommandBinding(
                    DecreaseCommand,
                    (s, e) =>
                    {
                        if (IsReadOnly)
                            return;
                        SetCurrentValue(ValueProperty, Value - Increment);
                    }
                )
            );
        }

        public override void OnApplyTemplate()
        {
            if (_textBox != null)
            {
                _textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;
                _textBox.TextChanged -= TextBox_TextChanged;
                _textBox.LostFocus -= TextBox_LostFocus;
            }

            base.OnApplyTemplate();

            _textBox = (GetTemplateChild(ElementTextBox) as TextBox)!;

            if (_textBox != null)
            {
                _textBox.SetBinding(
                    SelectionBrushProperty,
                    new Binding(SelectionBrushProperty.Name) { Source = this }
                );
                _textBox.SetBinding(
                    SelectionOpacityProperty,
                    new Binding(SelectionOpacityProperty.Name) { Source = this }
                );
                _textBox.SetBinding(
                    CaretBrushProperty,
                    new Binding(CaretBrushProperty.Name) { Source = this }
                );
                _textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                _textBox.TextChanged += TextBox_TextChanged;
                _textBox.LostFocus += TextBox_LostFocus;
                _textBox.Text = CurrentText;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_textBox.Text))
            {
                SetCurrentValue(ValueProperty, ValueBoxes.Double0Box);
            }
            else if (double.TryParse(_textBox.Text, out double value))
            {
                SetCurrentValue(ValueProperty, value);
            }
            else
            {
                SetText(true);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInternalChanged)
                return;

            if (double.TryParse(_textBox.Text, out double value))
            {
                if (value >= Minimum && value <= Maximum)
                {
                    try
                    {
                        _isInternalChanged = true;
                        SetCurrentValue(ValueProperty, value);
                    }
                    finally
                    {
                        _isInternalChanged = false;
                    }
                }
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsReadOnly)
                return;

            if (e.Key == Key.Up)
            {
                Value += Increment;
                SetText(true);
            }
            else if (e.Key == Key.Down)
            {
                Value -= Increment;
                SetText(true);
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (_textBox.IsFocused && !IsReadOnly)
            {
                Value += e.Delta > 0 ? Increment : -Increment;
                SetText(true);
                e.Handled = true;
            }
        }

        private string CurrentText =>
            string.IsNullOrWhiteSpace(ValueFormat)
                ? DecimalPlaces.HasValue
                    ? Value.ToString($"#0.{new string('0', DecimalPlaces.Value)}")
                    : Value.ToString()
                : Value.ToString(ValueFormat);

        private void SetText(bool force = false)
        {
            if (_textBox is null)
                return;
            // 仅在“当前正在由文本同步回 Value”时，避免立即反向覆盖用户输入。
            // 外部绑定更新、DataContext 切换导致的 Value 变化，仍应正常刷新到界面。
            if (!force && _isInternalChanged)
                return;
            var text = CurrentText;
            if (_textBox.Text != text)
            {
                _textBox.Text = text;
            }
            if (force || !_textBox.IsKeyboardFocused)
            {
                _textBox.Select(_textBox.Text.Length, 0);
            }
        }

        private static bool IsInRangeOfDouble(object value)
        {
            var v = (double)value;
            return !(double.IsNaN(v) || double.IsInfinity(v));
        }

        #region Dependency Properties

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(NumberBox),
            new FrameworkPropertyMetadata(
                ValueBoxes.Double0Box,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged,
                CoerceValueChanged
            ),
            IsInRangeOfDouble
        );

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>),
            typeof(NumberBox)
        );

        public event RoutedPropertyChangedEventHandler<double> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        private static object CoerceValueChanged(DependencyObject d, object baseValue)
        {
            var ctl = (NumberBox)d;
            var minimum = ctl.Minimum;
            var num = (double)baseValue;
            if (num < minimum)
            {
                ctl.Value = minimum;
                return minimum;
            }
            var maximum = ctl.Maximum;
            if (num > maximum)
            {
                ctl.Value = maximum;
            }
            ctl.SetText();
            return num > maximum ? maximum : num;
        }

        /// <summary>
        ///     This method is invoked when the Value property changes.
        /// </summary>
        /// <param name="oldValue">The old value of the Value property.</param>
        /// <param name="newValue">The new value of the Value property.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
            RoutedPropertyChangedEventArgs<double> args =
                new RoutedPropertyChangedEventArgs<double>(oldValue, newValue)
                {
                    RoutedEvent = ValueChangedEvent,
                };
            RaiseEvent(args);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (NumberBox)d;
            var v = (double)e.NewValue;
            ctl.SetText();

            ctl.OnValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(NumberBox),
            new PropertyMetadata(double.MaxValue, OnMaximumChanged, CoerceMaximum),
            IsInRangeOfDouble
        );

        private static void OnMaximumChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            var ctl = (NumberBox)d;
            ctl.CoerceValue(MinimumProperty);
            ctl.CoerceValue(ValueProperty);
        }

        private static object CoerceMaximum(DependencyObject d, object basevalue)
        {
            var minimum = ((NumberBox)d).Minimum;
            return (double)basevalue < minimum ? minimum : basevalue;
        }

        /// <summary>
        ///     最大值
        /// </summary>
        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        /// <summary>
        ///     最小值
        /// </summary>
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(NumberBox),
            new PropertyMetadata(double.MinValue, OnMinimumChanged, CoerceMinimum),
            IsInRangeOfDouble
        );

        private static void OnMinimumChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            var ctl = (NumberBox)d;
            ctl.CoerceValue(MaximumProperty);
            ctl.CoerceValue(ValueProperty);
        }

        private static object CoerceMinimum(DependencyObject d, object basevalue)
        {
            var maximum = ((NumberBox)d).Maximum;
            return (double)basevalue > maximum ? maximum : basevalue;
        }

        /// <summary>
        ///     最小值
        /// </summary>
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        static NumberBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NumberBox),
                new FrameworkPropertyMetadata(typeof(NumberBox))
            );
        }

        /// <summary>
        ///     指示每单击一下按钮时增加或减少的数量
        /// </summary>
        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
            nameof(Increment),
            typeof(double),
            typeof(NumberBox),
            new PropertyMetadata(ValueBoxes.Double1Box)
        );

        /// <summary>
        ///     指示每单击一下按钮时增加或减少的数量
        /// </summary>
        public double Increment
        {
            get => (double)GetValue(IncrementProperty);
            set => SetValue(IncrementProperty, value);
        }

        /// <summary>
        ///     指示要显示的小数位数
        /// </summary>
        public static readonly DependencyProperty DecimalPlacesProperty =
            DependencyProperty.Register(
                nameof(DecimalPlaces),
                typeof(int?),
                typeof(NumberBox),
                new PropertyMetadata(default(int?))
            );

        /// <summary>
        ///     指示要显示的小数位数
        /// </summary>
        public int? DecimalPlaces
        {
            get => (int?)GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, value);
        }

        /// <summary>
        ///     指示要显示的数字的格式
        /// </summary>
        public static readonly DependencyProperty ValueFormatProperty = DependencyProperty.Register(
            nameof(ValueFormat),
            typeof(string),
            typeof(NumberBox),
            new PropertyMetadata(default(string))
        );

        /// <summary>
        ///     指示要显示的数字的格式，这将会覆盖 <see cref="DecimalPlaces"/> 属性
        /// </summary>
        public string ValueFormat
        {
            get => (string)GetValue(ValueFormatProperty);
            set => SetValue(ValueFormatProperty, value);
        }

        /// <summary>
        ///     是否显示上下调值按钮
        /// </summary>
        internal static readonly DependencyProperty ShowUpDownButtonProperty =
            DependencyProperty.Register(
                nameof(ShowUpDownButton),
                typeof(bool),
                typeof(NumberBox),
                new PropertyMetadata(ValueBoxes.TrueBox)
            );

        /// <summary>
        ///     是否显示上下调值按钮
        /// </summary>
        public bool ShowUpDownButton
        {
            get => (bool)GetValue(ShowUpDownButtonProperty);
            set => SetValue(ShowUpDownButtonProperty, ValueBoxes.BooleanBox(value));
        }

        /// <summary>
        ///     标识 IsReadOnly 依赖属性。
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(NumberBox),
            new PropertyMetadata(ValueBoxes.FalseBox)
        );

        /// <summary>
        ///     获取或设置一个值，该值指示NumericUpDown是否只读。
        /// </summary>
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, ValueBoxes.BooleanBox(value));
        }

        public static readonly DependencyProperty SelectionBrushProperty =
            TextBoxBase.SelectionBrushProperty.AddOwner(typeof(NumberBox));

        public Brush SelectionBrush
        {
            get => (Brush)GetValue(SelectionBrushProperty);
            set => SetValue(SelectionBrushProperty, value);
        }

        public static readonly DependencyProperty SelectionOpacityProperty =
            TextBoxBase.SelectionOpacityProperty.AddOwner(typeof(NumberBox));

        public double SelectionOpacity
        {
            get => (double)GetValue(SelectionOpacityProperty);
            set => SetValue(SelectionOpacityProperty, value);
        }

        public static readonly DependencyProperty CaretBrushProperty =
            TextBoxBase.CaretBrushProperty.AddOwner(typeof(NumberBox));

        public Brush CaretBrush
        {
            get => (Brush)GetValue(CaretBrushProperty);
            set => SetValue(CaretBrushProperty, value);
        }

        #endregion
    }
}
