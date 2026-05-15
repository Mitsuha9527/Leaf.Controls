using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Leaf.Controls.Utilities;
using Leaf.Controls.Utilities.Input;

namespace Leaf.Controls.CustomControls
{
    [TemplatePart(Name = PART_DragAreaBorder, Type = typeof(Border))]
    public class TitleBar : ContentControl
    {
        const string PART_DragAreaBorder = "PART_DragAreaBorder";
        private Border? _dragAreaBorder;
        private bool _isDragging = false;
        private Point _startPoint;

        static TitleBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TitleBar),
                new FrameworkPropertyMetadata(typeof(TitleBar))
            );
        }

        #region Dependency Properties

        /// <summary>Identifies the <see cref="IsMaximized"/> dependency property.</summary>
        public static readonly DependencyProperty IsMaximizedProperty = DependencyProperty.Register(
            nameof(IsMaximized),
            typeof(bool),
            typeof(TitleBar),
            new PropertyMetadata(false)
        );

        /// <summary>Identifies the <see cref="ShowMaximize"/> dependency property.</summary>
        public static readonly DependencyProperty ShowMaximizeProperty =
            DependencyProperty.Register(
                nameof(ShowMaximize),
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(true)
            );

        /// <summary>Identifies the <see cref="ShowMinimize"/> dependency property.</summary>
        public static readonly DependencyProperty ShowMinimizeProperty =
            DependencyProperty.Register(
                nameof(ShowMinimize),
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(true)
            );

        /// <summary>Identifies the <see cref="ShowHelp"/> dependency property.</summary>
        public static readonly DependencyProperty ShowHelpProperty = DependencyProperty.Register(
            nameof(ShowHelp),
            typeof(bool),
            typeof(TitleBar),
            new PropertyMetadata(false)
        );

        /// <summary>Identifies the <see cref="ShowClose"/> dependency property.</summary>
        public static readonly DependencyProperty ShowCloseProperty = DependencyProperty.Register(
            nameof(ShowClose),
            typeof(bool),
            typeof(TitleBar),
            new PropertyMetadata(true)
        );

        /// <summary>Identifies the <see cref="CanMaximize"/> dependency property.</summary>
        public static readonly DependencyProperty CanMaximizeProperty = DependencyProperty.Register(
            nameof(CanMaximize),
            typeof(bool),
            typeof(TitleBar),
            new PropertyMetadata(true)
        );

        /// <summary>Identifies the <see cref="CloseClicked"/> routed event.</summary>
        public static readonly RoutedEvent CloseClickedEvent = EventManager.RegisterRoutedEvent(
            nameof(CloseClicked),
            RoutingStrategy.Bubble,
            typeof(RoutedEvent),
            typeof(TitleBar)
        );

        /// <summary>Identifies the <see cref="MaximizeClicked"/> routed event.</summary>
        public static readonly RoutedEvent MaximizeClickedEvent = EventManager.RegisterRoutedEvent(
            nameof(MaximizeClicked),
            RoutingStrategy.Bubble,
            typeof(RoutedEvent),
            typeof(TitleBar)
        );

        /// <summary>Identifies the <see cref="MinimizeClicked"/> routed event.</summary>
        public static readonly RoutedEvent MinimizeClickedEvent = EventManager.RegisterRoutedEvent(
            nameof(MinimizeClicked),
            RoutingStrategy.Bubble,
            typeof(RoutedEvent),
            typeof(TitleBar)
        );

        /// <summary>Identifies the <see cref="HelpClicked"/> routed event.</summary>
        public static readonly RoutedEvent HelpClickedEvent = EventManager.RegisterRoutedEvent(
            nameof(HelpClicked),
            RoutingStrategy.Bubble,
            typeof(RoutedEvent),
            typeof(TitleBar)
        );

        /// <summary>Identifies the <see cref="TemplateButtonCommand"/> dependency property.</summary>
        public static readonly DependencyProperty TemplateButtonCommandProperty =
            DependencyProperty.Register(
                nameof(TemplateButtonCommand),
                typeof(IRelayCommand),
                typeof(TitleBar),
                new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets a value indicating whether the current window is maximized.
        /// </summary>
        public bool IsMaximized
        {
            get => (bool)GetValue(IsMaximizedProperty);
            set => SetValue(IsMaximizedProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the maximize button.
        /// </summary>
        public bool ShowMaximize
        {
            get => (bool)GetValue(ShowMaximizeProperty);
            set => SetValue(ShowMaximizeProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the minimize button.
        /// </summary>
        public bool ShowMinimize
        {
            get => (bool)GetValue(ShowMinimizeProperty);
            set => SetValue(ShowMinimizeProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the help button
        /// </summary>
        public bool ShowHelp
        {
            get => (bool)GetValue(ShowHelpProperty);
            set => SetValue(ShowHelpProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the close button.
        /// </summary>
        public bool ShowClose
        {
            get => (bool)GetValue(ShowCloseProperty);
            set => SetValue(ShowCloseProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the maximize functionality is enabled. If disabled the MaximizeActionOverride action won't be called
        /// </summary>
        public bool CanMaximize
        {
            get => (bool)GetValue(CanMaximizeProperty);
            set => SetValue(CanMaximizeProperty, value);
        }

        /// <summary>
        /// Event triggered after clicking close button.
        /// </summary>
        public event RoutedEventHandler CloseClicked
        {
            add => AddHandler(CloseClickedEvent, value);
            remove => RemoveHandler(CloseClickedEvent, value);
        }

        /// <summary>
        /// Event triggered after clicking maximize or restore button.
        /// </summary>
        public event RoutedEventHandler MaximizeClicked
        {
            add => AddHandler(MaximizeClickedEvent, value);
            remove => RemoveHandler(MaximizeClickedEvent, value);
        }

        /// <summary>
        /// Event triggered after clicking minimize button.
        /// </summary>
        public event RoutedEventHandler MinimizeClicked
        {
            add => AddHandler(MinimizeClickedEvent, value);
            remove => RemoveHandler(MinimizeClickedEvent, value);
        }

        /// <summary>
        /// Event triggered after clicking help button
        /// </summary>
        public event RoutedEventHandler HelpClicked
        {
            add => AddHandler(HelpClickedEvent, value);
            remove => RemoveHandler(HelpClickedEvent, value);
        }

        /// <summary>
        /// Gets the command triggered when clicking the titlebar button.
        /// </summary>
        public IRelayCommand TemplateButtonCommand =>
            (IRelayCommand)GetValue(TemplateButtonCommandProperty);

        #endregion

        public TitleBar()
        {
            SetValue(
                TemplateButtonCommandProperty,
                new RelayCommand<TitleBarButtonType>(OnTemplateButtonClick)
            );
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private Window _currentWindow = null!;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _dragAreaBorder = GetTemplateChild(PART_DragAreaBorder) as Border;
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignerHelper.IsInDesignMode)
            {
                return;
            }
            _currentWindow =
                System.Windows.Window.GetWindow(this) ?? throw new InvalidOperationException(
                    "Window is null"
                );
            _currentWindow.StateChanged += OnParentWindowStateChanged;

            if (_dragAreaBorder is not null)
            {
                _dragAreaBorder.MouseLeftButtonDown += OnMouseLeftButtonDown;
                _dragAreaBorder.MouseLeftButtonUp += OnMouseLeftButtonUp;
                _dragAreaBorder.MouseMove += OnMouseMove;
            }
            if (IsMaximized)
            {
                var thickness =
                    (SystemParameters.MaximizedPrimaryScreenWidth - SystemParameters.WorkArea.Width)
                    / 4;
                _currentWindow.SetCurrentValue(
                    PaddingProperty,
                    IsMaximized ? new Thickness(thickness) : default
                );
            }
        }

        private void OnMouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                // 检查是否是双击
                if (e.ClickCount == 2)
                {
                    MaximizeWindow();
                    return;
                }

                // 记录开始位置，准备检测拖动
                _startPoint = e.GetPosition(_dragAreaBorder);
                _isDragging = false;
                _dragAreaBorder?.CaptureMouse();
            }
        }

        private void OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_dragAreaBorder?.IsMouseCaptured == true)
            {
                _dragAreaBorder.ReleaseMouseCapture();
            }
            _isDragging = false;
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (
                e.LeftButton == System.Windows.Input.MouseButtonState.Pressed
                && _dragAreaBorder?.IsMouseCaptured == true
            )
            {
                Point currentPoint = e.GetPosition(_dragAreaBorder);

                // 计算鼠标移动距离
                double deltaX = Math.Abs(currentPoint.X - _startPoint.X);
                double deltaY = Math.Abs(currentPoint.Y - _startPoint.Y);

                // 只有当鼠标移动超过一定阈值时才开始拖动
                if (
                    !_isDragging
                    && (
                        deltaX > SystemParameters.MinimumHorizontalDragDistance
                        || deltaY > SystemParameters.MinimumVerticalDragDistance
                    )
                )
                {
                    _isDragging = true;

                    // 如果窗口是最大化的，先将其恢复为普通状态并移动到鼠标位置
                    if (IsMaximized)
                    {
                        // 获取鼠标相对于标题栏的位置
                        var mousePoint = e.GetPosition(_dragAreaBorder);
                        MaximizeWindow();
                        // 标题栏在还原后的宽度
                        double titleBarWidth = _dragAreaBorder.ActualWidth;
                        // 计算窗口左侧位置，使标题栏中心与鼠标对齐
                        _currentWindow.Left = mousePoint.X - (titleBarWidth / 2);
                        // 确保鼠标在标题栏上的相对位置保持不变
                        _currentWindow.Top = mousePoint.Y - mousePoint.Y;
                    }

                    // 释放鼠标捕获并开始拖动
                    _dragAreaBorder.ReleaseMouseCapture();
                    _currentWindow.DragMove();
                }
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
        }

        private void CloseWindow()
        {
            _currentWindow?.Close();
        }

        private void MinimizeWindow()
        {
            _currentWindow.SetCurrentValue(Window.WindowStateProperty, WindowState.Minimized);
        }

        private void MaximizeWindow()
        {
            if (!CanMaximize)
            {
                return;
            }
            if (_currentWindow.WindowState == WindowState.Normal)
            {
                SetCurrentValue(IsMaximizedProperty, true);
                //_currentWindow.SetCurrentValue(Window.PaddingProperty, new Thickness(10));
                _currentWindow.SetCurrentValue(Window.WindowStateProperty, WindowState.Maximized);
            }
            else
            {
                SetCurrentValue(IsMaximizedProperty, false);
                // _currentWindow.SetCurrentValue(Window.PaddingProperty, new Thickness(0));
                _currentWindow.SetCurrentValue(Window.WindowStateProperty, WindowState.Normal);
            }
        }

        private void OnParentWindowStateChanged(object? sender, EventArgs e)
        {
            if (IsMaximized != (_currentWindow.WindowState == WindowState.Maximized))
            {
                SetCurrentValue(
                    IsMaximizedProperty,
                    _currentWindow.WindowState == WindowState.Maximized
                );
            }
            var thickness =
                (SystemParameters.MaximizedPrimaryScreenWidth - SystemParameters.WorkArea.Width)
                / 4;
            _currentWindow.SetCurrentValue(
                PaddingProperty,
                IsMaximized ? new Thickness(thickness) : default
            );
        }

        private void OnTemplateButtonClick(TitleBarButtonType buttonType)
        {
            switch (buttonType)
            {
                case TitleBarButtonType.Maximize or TitleBarButtonType.Restore:
                    RaiseEvent(new RoutedEventArgs(MaximizeClickedEvent, this));
                    MaximizeWindow();
                    break;

                case TitleBarButtonType.Close:
                    RaiseEvent(new RoutedEventArgs(CloseClickedEvent, this));
                    CloseWindow();
                    break;

                case TitleBarButtonType.Minimize:
                    RaiseEvent(new RoutedEventArgs(MinimizeClickedEvent, this));
                    MinimizeWindow();
                    break;

                case TitleBarButtonType.Help:
                    RaiseEvent(new RoutedEventArgs(HelpClickedEvent, this));
                    break;
            }
        }
    }
}
