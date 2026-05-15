using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Leaf.Controls.CustomControls
{
    /// <summary>
    /// 分页控件
    /// </summary>
    [TemplatePart(Name = ElementButtonLeft, Type = typeof(Button))]
    [TemplatePart(Name = ElementButtonRight, Type = typeof(Button))]
    [TemplatePart(Name = ElementButtonFirst, Type = typeof(RadioButton))]
    [TemplatePart(Name = ElementMoreLeft, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ElementPanelMain, Type = typeof(Panel))]
    [TemplatePart(Name = ElementMoreRight, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ElementButtonLast, Type = typeof(RadioButton))]
    [TemplatePart(Name = ElementJump, Type = typeof(NumberBox))]
    public class Pagination : Control
    {
        #region Commands

        /// <summary>
        /// 上一页命令
        /// </summary>
        public static RoutedCommand PrevCommand { get; } = new(nameof(PrevCommand), typeof(Pagination));

        /// <summary>
        /// 下一页命令
        /// </summary>
        public static RoutedCommand NextCommand { get; } = new(nameof(NextCommand), typeof(Pagination));

        /// <summary>
        /// 选中页命令
        /// </summary>
        public static RoutedCommand SelectedCommand { get; } = new(nameof(SelectedCommand), typeof(Pagination));

        /// <summary>
        /// 跳转页命令
        /// </summary>
        public static RoutedCommand JumpCommand { get; } = new(nameof(JumpCommand), typeof(Pagination));

        #endregion Commands

        #region Constants

        private const string ElementButtonLeft = "PART_ButtonLeft";
        private const string ElementButtonRight = "PART_ButtonRight";
        private const string ElementButtonFirst = "PART_ButtonFirst";
        private const string ElementMoreLeft = "PART_MoreLeft";
        private const string ElementPanelMain = "PART_PanelMain";
        private const string ElementMoreRight = "PART_MoreRight";
        private const string ElementButtonLast = "PART_ButtonLast";
        private const string ElementJump = "PART_Jump";

        #endregion Constants

        #region Data

        private Button? _buttonLeft;
        private Button? _buttonRight;
        private RadioButton? _buttonFirst;
        private FrameworkElement? _moreLeft;
        private Panel? _panelMain;
        private FrameworkElement? _moreRight;
        private RadioButton? _buttonLast;
        private NumberBox? _jumpNumberBox;

        private bool _appliedTemplate;

        #endregion Data

        #region Public Events

        /// <summary>
        /// 页面更新事件
        /// </summary>
        public static readonly RoutedEvent PageUpdatedEvent =
            EventManager.RegisterRoutedEvent("PageUpdated", RoutingStrategy.Bubble,
                typeof(EventHandler<RoutedEventArgs>), typeof(Pagination));

        /// <summary>
        /// 页面更新事件
        /// </summary>
        public event EventHandler<RoutedEventArgs> PageUpdated
        {
            add => AddHandler(PageUpdatedEvent, value);
            remove => RemoveHandler(PageUpdatedEvent, value);
        }

        #endregion Public Events

        static Pagination()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Pagination), new FrameworkPropertyMetadata(typeof(Pagination)));
        }

        public Pagination()
        {
            CommandBindings.Add(new CommandBinding(PrevCommand, ButtonPrev_OnClick));
            CommandBindings.Add(new CommandBinding(NextCommand, ButtonNext_OnClick));
            CommandBindings.Add(new CommandBinding(SelectedCommand, ToggleButton_OnChecked));
            CommandBindings.Add(new CommandBinding(JumpCommand, (s, e) => {
                if (_jumpNumberBox != null)
                    PageIndex = (int)_jumpNumberBox.Value;
            }));
            Loaded += (s, e) => Update();
        }

        #region Public Properties

        #region MaxPageCount

        /// <summary>
        /// 最大页数
        /// </summary>
        public static readonly DependencyProperty MaxPageCountProperty = DependencyProperty.Register(
            nameof(MaxPageCount), typeof(int), typeof(Pagination), 
            new PropertyMetadata(1, OnMaxPageCountChanged, CoerceMaxPageCount));

        private static object CoerceMaxPageCount(DependencyObject d, object basevalue)
        {
            var intValue = (int)basevalue;
            return intValue < 1 ? 1 : intValue;
        }

        private static void OnMaxPageCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Pagination pagination)
            {
                if (pagination.PageIndex > pagination.MaxPageCount)
                {
                    pagination.PageIndex = pagination.MaxPageCount;
                }

                pagination.CoerceValue(PageIndexProperty);
                pagination.OnAutoHidingChanged(pagination.AutoHiding);
                pagination.Update();
            }
        }

        /// <summary>
        /// 最大页数
        /// </summary>
        public int MaxPageCount
        {
            get => (int)GetValue(MaxPageCountProperty);
            set => SetValue(MaxPageCountProperty, value);
        }

        #endregion MaxPageCount

        #region DataCountPerPage

        /// <summary>
        /// 每页的数据量
        /// </summary>
        public static readonly DependencyProperty DataCountPerPageProperty = DependencyProperty.Register(
            nameof(DataCountPerPage), typeof(int), typeof(Pagination), 
            new PropertyMetadata(20, OnDataCountPerPageChanged));

        private static void OnDataCountPerPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Pagination pagination)
            {
                pagination.Update();
            }
        }

        /// <summary>
        /// 每页的数据量
        /// </summary>
        public int DataCountPerPage
        {
            get => (int)GetValue(DataCountPerPageProperty);
            set => SetValue(DataCountPerPageProperty, value);
        }

        #endregion

        #region PageIndex

        /// <summary>
        /// 当前页
        /// </summary>
        public static readonly DependencyProperty PageIndexProperty = DependencyProperty.Register(
            nameof(PageIndex), typeof(int), typeof(Pagination), 
            new PropertyMetadata(1, OnPageIndexChanged, CoercePageIndex));

        private static object CoercePageIndex(DependencyObject d, object basevalue)
        {
            if (d is not Pagination pagination) return 1;

            var intValue = (int)basevalue;
            return intValue < 1
                ? 1
                : intValue > pagination.MaxPageCount
                    ? pagination.MaxPageCount
                    : intValue;
        }

        private static void OnPageIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Pagination pagination && e.NewValue is int value)
            {
                pagination.Update();
                pagination.RaiseEvent(new RoutedEventArgs(PageUpdatedEvent, pagination));
            }
        }

        /// <summary>
        /// 当前页
        /// </summary>
        public int PageIndex
        {
            get => (int)GetValue(PageIndexProperty);
            set => SetValue(PageIndexProperty, value);
        }

        #endregion PageIndex

        #region MaxPageInterval

        /// <summary>
        /// 表示当前选中的按钮距离左右两个方向按钮的最大间隔（4表示间隔4个按钮，如果超过则用省略号表示）
        /// </summary>
        public static readonly DependencyProperty MaxPageIntervalProperty = DependencyProperty.Register(
            nameof(MaxPageInterval), typeof(int), typeof(Pagination), 
            new PropertyMetadata(3, OnMaxPageIntervalChanged));

        private static void OnMaxPageIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Pagination pagination)
            {
                pagination.Update();
            }
        }

        /// <summary>
        /// 表示当前选中的按钮距离左右两个方向按钮的最大间隔（4表示间隔4个按钮，如果超过则用省略号表示）
        /// </summary>
        public int MaxPageInterval
        {
            get => (int)GetValue(MaxPageIntervalProperty);
            set => SetValue(MaxPageIntervalProperty, value);
        }

        #endregion MaxPageInterval

        #region IsJumpEnabled

        public static readonly DependencyProperty IsJumpEnabledProperty = DependencyProperty.Register(
            nameof(IsJumpEnabled), typeof(bool), typeof(Pagination), 
            new PropertyMetadata(false));

        public bool IsJumpEnabled
        {
            get => (bool)GetValue(IsJumpEnabledProperty);
            set => SetValue(IsJumpEnabledProperty, value);
        }

        #endregion

        #region AutoHiding

        public static readonly DependencyProperty AutoHidingProperty = DependencyProperty.Register(
            nameof(AutoHiding), typeof(bool), typeof(Pagination), 
            new PropertyMetadata(true, OnAutoHidingChanged));

        private static void OnAutoHidingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Pagination pagination)
            {
                pagination.OnAutoHidingChanged((bool)e.NewValue);
            }
        }

        private void OnAutoHidingChanged(bool newValue) => this.Visibility = (!newValue || MaxPageCount > 1) ? Visibility.Visible : Visibility.Collapsed;

        public bool AutoHiding
        {
            get => (bool)GetValue(AutoHidingProperty);
            set => SetValue(AutoHidingProperty, value);
        }

        #endregion

        public static readonly DependencyProperty PaginationButtonStyleProperty = DependencyProperty.Register(
            nameof(PaginationButtonStyle), typeof(Style), typeof(Pagination), 
            new PropertyMetadata(default(Style)));

        public Style PaginationButtonStyle
        {
            get => (Style)GetValue(PaginationButtonStyleProperty);
            set => SetValue(PaginationButtonStyleProperty, value);
        }

        #endregion

        #region Public Methods

        public override void OnApplyTemplate()
        {
            _appliedTemplate = false;
            base.OnApplyTemplate();

            _buttonLeft = GetTemplateChild(ElementButtonLeft) as Button;
            _buttonRight = GetTemplateChild(ElementButtonRight) as Button;
            _buttonFirst = GetTemplateChild(ElementButtonFirst) as RadioButton;
            _moreLeft = GetTemplateChild(ElementMoreLeft) as FrameworkElement;
            _panelMain = GetTemplateChild(ElementPanelMain) as Panel;
            _moreRight = GetTemplateChild(ElementMoreRight) as FrameworkElement;
            _buttonLast = GetTemplateChild(ElementButtonLast) as RadioButton;
            _jumpNumberBox = GetTemplateChild(ElementJump) as NumberBox;

            // 为 NumberBox 添加事件处理
            if (_jumpNumberBox != null)
            {
                _jumpNumberBox.KeyDown += JumpNumberBox_KeyDown;
                _jumpNumberBox.LostFocus += JumpNumberBox_LostFocus;
            }

            CheckNull();

            _appliedTemplate = true;
            Update();
        }

        #endregion Public Methods

        #region Private Methods

        private void CheckNull()
        {
            if (_buttonLeft == null || _buttonRight == null || _buttonFirst == null ||
                _moreLeft == null || _panelMain == null || _moreRight == null ||
                _buttonLast == null) throw new Exception();
        }        /// <summary>
        /// 更新
        /// </summary>
        private void Update()
        {
            if (!_appliedTemplate || _buttonLeft == null || _buttonRight == null || 
                _buttonFirst == null || _buttonLast == null || _moreLeft == null || 
                _moreRight == null || _panelMain == null) return;

            // 使用 BeginUpdate/EndUpdate 模式减少重绘
            try
            {
                // 禁用布局更新以减少闪烁
                _panelMain.IsEnabled = false;
                
                // 更新导航按钮状态（只在状态改变时更新）
                var leftEnabled = PageIndex > 1;
                var rightEnabled = PageIndex < MaxPageCount;
                
                if (_buttonLeft.IsEnabled != leftEnabled)
                    _buttonLeft.IsEnabled = leftEnabled;
                if (_buttonRight.IsEnabled != rightEnabled)
                    _buttonRight.IsEnabled = rightEnabled;

                // 特殊情况：MaxPageInterval为0时只显示当前页
                if (MaxPageInterval == 0)
                {
                    UpdateVisibilityIfNeeded(_buttonFirst, false);
                    UpdateVisibilityIfNeeded(_buttonLast, false);
                    UpdateVisibilityIfNeeded(_moreLeft, false);
                    UpdateVisibilityIfNeeded(_moreRight, false);
                    
                    if (_panelMain.Children.Count != 1 || 
                        (_panelMain.Children[0] as RadioButton)?.Content?.ToString() != PageIndex.ToString())
                    {
                        _panelMain.Children.Clear();
                        var selectButton = CreateButton(PageIndex);
                        _panelMain.Children.Add(selectButton);
                        selectButton.IsChecked = true;
                    }
                    return;
                }

                // 预计算所有需要的状态
                var needFirstButton = true;
                var needLastButton = MaxPageCount > 1;
                var right = MaxPageCount - PageIndex;
                var left = PageIndex - 1;
                var needMoreLeft = left > MaxPageInterval;
                var needMoreRight = right > MaxPageInterval;

                // 批量更新可见性（只在需要时更新）
                UpdateVisibilityIfNeeded(_buttonFirst, needFirstButton);
                UpdateVisibilityIfNeeded(_buttonLast, needLastButton);
                UpdateVisibilityIfNeeded(_moreLeft, needMoreLeft);
                UpdateVisibilityIfNeeded(_moreRight, needMoreRight);

                // 更新最后一页内容
                if (needLastButton && _buttonLast.Content?.ToString() != MaxPageCount.ToString())
                {
                    _buttonLast.Content = MaxPageCount.ToString();
                }

                // 构建新的中间按钮列表
                var newButtons = new List<RadioButton>();
                
                // 添加左侧按钮
                var sub = PageIndex;
                for (var i = 0; i < MaxPageInterval - 1; i++)
                {
                    if (--sub > 1)
                    {
                        newButtons.Insert(0, CreateButton(sub));
                    }
                    else
                    {
                        break;
                    }
                }

                // 添加当前页按钮（如果在中间）
                if (PageIndex > 1 && PageIndex < MaxPageCount)
                {
                    var currentButton = CreateButton(PageIndex);
                    currentButton.IsChecked = true;
                    newButtons.Add(currentButton);
                }

                // 添加右侧按钮
                var add = PageIndex;
                for (var i = 0; i < MaxPageInterval - 1; i++)
                {
                    if (++add < MaxPageCount)
                    {
                        newButtons.Add(CreateButton(add));
                    }
                    else
                    {
                        break;
                    }
                }

                // 智能更新中间面板（减少不必要的清空和重建）
                UpdateMiddlePanelButtons(newButtons);

                // 更新选中状态
                UpdateCheckedStates();
            }
            finally
            {
                // 重新启用布局更新
                _panelMain.IsEnabled = true;
            }
        }
        private void UpdateVisibilityIfNeeded(FrameworkElement element, bool shouldBeVisible)
        {
            var targetVisibility = shouldBeVisible ? Visibility.Visible : Visibility.Collapsed;
            if (element.Visibility != targetVisibility)
            {
                element.Visibility = targetVisibility;
            }
        }        private void UpdateMiddlePanelButtons(List<RadioButton> newButtons)
        {
            if (_panelMain == null || newButtons == null) return;
            
            // 检查是否需要更新
            if (_panelMain.Children.Count == newButtons.Count)
            {
                bool needUpdate = false;
                for (int i = 0; i < newButtons.Count; i++)
                {
                    if (_panelMain.Children[i] is RadioButton existingButton &&
                        existingButton.Content?.ToString() != newButtons[i].Content?.ToString())
                    {
                        needUpdate = true;
                        break;
                    }
                }
                
                if (!needUpdate) return;
            }

            // 需要更新时才清空并重建
            _panelMain.Children.Clear();
            foreach (var button in newButtons)
            {
                _panelMain.Children.Add(button);
            }
        }

        private void UpdateCheckedStates()
        {
            // 只在需要时更新选中状态
            var shouldFirstBeChecked = PageIndex == 1;
            var shouldLastBeChecked = PageIndex == MaxPageCount && MaxPageCount > 1;

            if (_buttonFirst != null && _buttonFirst.IsChecked != shouldFirstBeChecked)
                _buttonFirst.IsChecked = shouldFirstBeChecked;
                
            if (_buttonLast != null && _buttonLast.IsChecked != shouldLastBeChecked)
                _buttonLast.IsChecked = shouldLastBeChecked;
        }

        private void ButtonPrev_OnClick(object sender, RoutedEventArgs e) => PageIndex--;

        private void ButtonNext_OnClick(object sender, RoutedEventArgs e) => PageIndex++;

        private RadioButton CreateButton(int page)
        {
            return new()
            {
                Style = FindResource("PaginationRadioButtonStyle") as Style,
                Content = page.ToString(),
                Command = SelectedCommand,
                GroupName = this.Name
            };
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is not RadioButton button) return;
            if (button.IsChecked == false) return;
            var content = button.Content?.ToString();
            if (content != null && int.TryParse(content, out int pageIndex))
            {
                PageIndex = pageIndex;
            }
        }

        private void JumpNumberBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is NumberBox numberBox)
            {
                PageIndex = (int)numberBox.Value;
            }
        }

        private void JumpNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is NumberBox numberBox)
            {
                PageIndex = (int)numberBox.Value;
            }
        }

        #endregion Private Methods
    }
}
