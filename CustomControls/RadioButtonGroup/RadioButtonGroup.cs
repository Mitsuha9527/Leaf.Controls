using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Leaf.Controls.CustomControls
{
    /// <summary>
    /// RadioButtonGroup控件，提供带有滑动指示器的RadioButton组
    /// </summary>
    public class RadioButtonGroup : ItemsControl
    {
        #region 私有字段

        private Canvas _indicatorCanvas;
        private Rectangle _indicatorLine;
        private RadioButton _selectedRadioButton;

        #endregion

        #region 路由事件

        /// <summary>
        /// 选中项变化事件
        /// </summary>
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(SelectionChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<object>),
            typeof(RadioButtonGroup)
        );

        /// <summary>
        /// 选中项变化事件处理器
        /// </summary>
        public event RoutedPropertyChangedEventHandler<object> SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        /// <summary>
        /// 选中前事件
        /// </summary>
        public static readonly RoutedEvent PreviewSelectionChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(PreviewSelectionChanged),
            RoutingStrategy.Tunnel,
            typeof(RoutedPropertyChangedEventHandler<object>),
            typeof(RadioButtonGroup)
        );

        /// <summary>
        /// 选中前事件处理器
        /// </summary>
        public event RoutedPropertyChangedEventHandler<object> PreviewSelectionChanged
        {
            add { AddHandler(PreviewSelectionChangedEvent, value); }
            remove { RemoveHandler(PreviewSelectionChangedEvent, value); }
        }

        /// <summary>
        /// 选中索引变化事件
        /// </summary>
        public static readonly RoutedEvent SelectedIndexChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(SelectedIndexChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>),
            typeof(RadioButtonGroup)
        );

        /// <summary>
        /// 选中索引变化事件处理器
        /// </summary>
        public event RoutedPropertyChangedEventHandler<int> SelectedIndexChanged
        {
            add { AddHandler(SelectedIndexChangedEvent, value); }
            remove { RemoveHandler(SelectedIndexChangedEvent, value); }
        }

        /// <summary>
        /// 单击选项事件
        /// </summary>
        public static readonly RoutedEvent ItemClickEvent = EventManager.RegisterRoutedEvent(
            nameof(ItemClick),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(RadioButtonGroup)
        );

        /// <summary>
        /// 单击选项事件处理器
        /// </summary>
        public event RoutedEventHandler ItemClick
        {
            add { AddHandler(ItemClickEvent, value); }
            remove { RemoveHandler(ItemClickEvent, value); }
        }

        #endregion

        #region 依赖属性

        /// <summary>
        /// 选中项的索引
        /// </summary>
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(RadioButtonGroup),
                new PropertyMetadata(-1, OnSelectedIndexChanged));

        /// <summary>
        /// 选中项
        /// </summary>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(RadioButtonGroup),
                new PropertyMetadata(null, OnSelectedItemChanged));

        /// <summary>
        /// 指示条动画持续时间
        /// </summary>
        public Duration AnimationDuration
        {
            get { return (Duration)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration", typeof(Duration), typeof(RadioButtonGroup),
                new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(300))));

        /// <summary>
        /// 是否允许取消选择
        /// </summary>
        public bool AllowUnchecked
        {
            get { return (bool)GetValue(AllowUncheckedProperty); }
            set { SetValue(AllowUncheckedProperty, value); }
        }

        public static readonly DependencyProperty AllowUncheckedProperty =
            DependencyProperty.Register(nameof(AllowUnchecked), typeof(bool), typeof(RadioButtonGroup),
                new PropertyMetadata(false));

        #endregion

        #region 构造函数

        static RadioButtonGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RadioButtonGroup),
                new FrameworkPropertyMetadata(typeof(RadioButtonGroup)));
        }

        public RadioButtonGroup()
        {
            Loaded += OnLoaded;
        }

        #endregion

        #region 事件触发方法

        /// <summary>
        /// 触发选中项变化事件
        /// </summary>
        /// <param name="oldValue">旧值</param>
        /// <param name="newValue">新值</param>
        protected virtual void OnSelectionChanged(object oldValue, object newValue)
        {
            // 触发预览事件
            RoutedPropertyChangedEventArgs<object> previewArgs = new RoutedPropertyChangedEventArgs<object>(
                oldValue, newValue, PreviewSelectionChangedEvent);
            RaiseEvent(previewArgs);

            if (previewArgs.Handled)
                return;

            // 触发正式事件
            RoutedPropertyChangedEventArgs<object> args = new RoutedPropertyChangedEventArgs<object>(
                oldValue, newValue, SelectionChangedEvent);
            RaiseEvent(args);
        }

        /// <summary>
        /// 触发选中索引变化事件
        /// </summary>
        /// <param name="oldValue">旧值</param>
        /// <param name="newValue">新值</param>
        protected virtual void OnSelectedIndexChanged(int oldValue, int newValue)
        {
            RoutedPropertyChangedEventArgs<int> args = new RoutedPropertyChangedEventArgs<int>(
                oldValue, newValue, SelectedIndexChangedEvent);
            RaiseEvent(args);
        }

        /// <summary>
        /// 触发单击选项事件
        /// </summary>
        protected virtual void OnItemClick(object item)
        {
            RoutedEventArgs args = new RoutedEventArgs(ItemClickEvent, item);
            RaiseEvent(args);
        }

        #endregion

        #region 方法重写

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _indicatorCanvas = GetTemplateChild("PART_IndicatorCanvas") as Canvas;
            _indicatorLine = GetTemplateChild("PART_IndicatorLine") as Rectangle;

            if (_indicatorLine != null)
            {
                _indicatorLine.Width = 2; // 设置指示线宽度
            }

            SetupRadioButtons();
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            SetupRadioButtons();

            // 如果有选中项，更新指示器
            if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
            {
                UpdateIndicator(GetRadioButtonAtIndex(SelectedIndex), false);
            }
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is RadioButton radioButton)
            {
                radioButton.GroupName = $"RadioButtonGroup_{this.GetHashCode()}";
                radioButton.Checked += RadioButton_Checked;

                // 添加点击事件监听
                radioButton.Click += RadioButton_Click;

                // 如果允许取消选择，需要添加Unchecked事件处理
                if (AllowUnchecked)
                {
                    radioButton.Unchecked += RadioButton_Unchecked;
                }
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is RadioButton radioButton)
            {
                radioButton.Checked -= RadioButton_Checked;
                radioButton.Click -= RadioButton_Click;

                if (AllowUnchecked)
                {
                    radioButton.Unchecked -= RadioButton_Unchecked;
                }
            }

            base.ClearContainerForItemOverride(element, item);
        }

        #endregion

        #region 事件处理

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitializeIndicator();

            // 如果已设置了SelectedIndex，则激活相应项
            if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
            {
                RadioButton radioButton = GetRadioButtonAtIndex(SelectedIndex);
                if (radioButton != null)
                {
                    radioButton.IsChecked = true;
                    UpdateIndicator(radioButton, false);
                }
            }
        }

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadioButtonGroup group = d as RadioButtonGroup;
            int oldIndex = (int)e.OldValue;
            int newIndex = (int)e.NewValue;

            if (group != null)
            {
                // 触发索引变化事件
                group.OnSelectedIndexChanged(oldIndex, newIndex);

                if (group.Items.Count > 0 && newIndex >= 0 && newIndex < group.Items.Count)
                {
                    RadioButton radioButton = group.GetRadioButtonAtIndex(newIndex);
                    if (radioButton != null)
                    {
                        radioButton.IsChecked = true;
                        group.SelectedItem = group.Items[newIndex];
                    }
                }
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadioButtonGroup group = d as RadioButtonGroup;
            object oldItem = e.OldValue;
            object newItem = e.NewValue;

            if (group != null)
            {
                // 触发选中项变化事件
                group.OnSelectionChanged(oldItem, newItem);

                if (newItem != null)
                {
                    int index = group.Items.IndexOf(newItem);
                    if (index != -1 && index != group.SelectedIndex)
                    {
                        group.SelectedIndex = index;
                    }
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                int index = GetRadioButtonIndex(radioButton);
                if (index != -1)
                {
                    object oldItem = SelectedItem;
                    int oldIndex = SelectedIndex;
                    object newItem = Items[index];

                    SelectedIndex = index;
                    SelectedItem = newItem;
                    UpdateIndicator(radioButton, true);
                    _selectedRadioButton = radioButton;
                }
            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                int index = GetRadioButtonIndex(radioButton);
                if (index != -1)
                {
                    // 触发点击事件
                    OnItemClick(Items[index]);
                }
            }
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!AllowUnchecked)
                return;

            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null && radioButton == _selectedRadioButton)
            {
                // 如果当前选中的按钮被取消选中
                object oldItem = SelectedItem;
                int oldIndex = SelectedIndex;

                SelectedIndex = -1;
                SelectedItem = null;
                _selectedRadioButton = null;
            }
        }

        #endregion

        #region 私有方法

        private void SetupRadioButtons()
        {
            // 遍历所有RadioButton并设置事件
            foreach (var item in Items)
            {
                RadioButton radioButton = GetRadioButtonForItem(item);
                if (radioButton != null)
                {
                    radioButton.GroupName = $"RadioButtonGroup_{this.GetHashCode()}";

                    // 移除旧事件以避免重复
                    radioButton.Checked -= RadioButton_Checked;
                    radioButton.Checked += RadioButton_Checked;

                    radioButton.Click -= RadioButton_Click;
                    radioButton.Click += RadioButton_Click;

                    if (AllowUnchecked)
                    {
                        radioButton.Unchecked -= RadioButton_Unchecked;
                        radioButton.Unchecked += RadioButton_Unchecked;
                    }
                }
            }
        }

        private void InitializeIndicator()
        {
            if (_indicatorLine == null || _indicatorCanvas == null) return;

            // 调整指示Canvas大小与控件一致
            _indicatorCanvas.Width = ActualWidth;
            _indicatorCanvas.Height = ActualHeight;

            // 初始将指示线隐藏
            _indicatorLine.Height = 0;
            _indicatorLine.Opacity = 1;
            Canvas.SetLeft(_indicatorLine, 0);
            Canvas.SetTop(_indicatorLine, 0);
        }

        private void UpdateIndicator(RadioButton radioButton, bool animate)
        {
            if (_indicatorLine == null || _indicatorCanvas == null || radioButton == null) return;

            // 获取RadioButton相对于Canvas的位置
            Point relativePoint = radioButton.TranslatePoint(new Point(0, 0), _indicatorCanvas);

            // 调整指示线的位置和尺寸
            double newLeft = 5; // 指示线左边距
            double newTop = relativePoint.Y + radioButton.ActualHeight * 0.25;
            double newHeight = radioButton.ActualHeight * 0.5;

            if (animate)
            {
                // 创建动画
                DoubleAnimation topAnim = new DoubleAnimation(newTop, AnimationDuration);
                DoubleAnimation heightAnim = new DoubleAnimation(newHeight, AnimationDuration);

                topAnim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                heightAnim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

                // 应用动画
                _indicatorLine.BeginAnimation(Canvas.TopProperty, topAnim);
                _indicatorLine.BeginAnimation(Rectangle.HeightProperty, heightAnim);

                // 设置固定位置
                Canvas.SetLeft(_indicatorLine, newLeft);
            }
            else
            {
                // 直接设置位置
                Canvas.SetLeft(_indicatorLine, newLeft);
                Canvas.SetTop(_indicatorLine, newTop);
                _indicatorLine.Height = newHeight;
            }
        }

        private RadioButton GetRadioButtonAtIndex(int index)
        {
            if (index < 0 || index >= Items.Count)
                return null;

            object item = Items[index];
            return GetRadioButtonForItem(item);
        }

        private RadioButton GetRadioButtonForItem(object item)
        {
            if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return ItemContainerGenerator.ContainerFromItem(item) as RadioButton;
        }

        private int GetRadioButtonIndex(RadioButton radioButton)
        {
            if (radioButton == null)
                return -1;

            object item = ItemContainerGenerator.ItemFromContainer(radioButton);
            if (item == DependencyProperty.UnsetValue)
                return -1;

            return Items.IndexOf(item);
        }

        #endregion
    }
}
