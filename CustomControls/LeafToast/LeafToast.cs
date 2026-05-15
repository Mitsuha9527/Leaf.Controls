using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Leaf.Controls.CustomControls
{
    /// <summary>
    /// Toast 通知控件
    /// </summary>
    [TemplatePart(Name = ElementGridMain, Type = typeof(Grid))]
    [TemplatePart(Name = ElementButtonClose, Type = typeof(Button))]
    [TemplatePart(Name = ElementButtonPanel, Type = typeof(Panel))]
    [TemplatePart(Name = ElementButtonConfirm, Type = typeof(Button))]
    [TemplatePart(Name = ElementButtonCancel, Type = typeof(Button))]
    public class LeafToast : Control
    {
        #region Constants

        private const string ElementGridMain = "PART_GridMain";
        private const string ElementButtonClose = "PART_ButtonClose";
        private const string ElementButtonPanel = "PART_ButtonPanel";
        private const string ElementButtonConfirm = "PART_ButtonConfirm";
        private const string ElementButtonCancel = "PART_ButtonCancel";
        private const int MinWaitTime = 1000;

        #endregion

        #region Fields

        private Grid? _gridMain;
        private Button? _buttonClose;
        private Panel? _buttonPanel;
        private Button? _buttonConfirm;
        private Button? _buttonCancel;
        private DispatcherTimer? _closeTimer;
        private int _tickCount;
        private bool _showCloseButton = true;
        private bool _staysOpen;
        private int _waitTime = 4000;

        private static readonly Dictionary<string, Panel> _tokenDictionary =
            new Dictionary<string, Panel>();
        private static LeafToastWindow? _globalWindow;

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Toast 信息
        /// </summary>
        public static readonly DependencyProperty ToastInfoProperty = DependencyProperty.Register(
            nameof(ToastInfo),
            typeof(LeafToastInfo),
            typeof(LeafToast),
            new PropertyMetadata(null, OnToastInfoChanged)
        );

        /// <summary>
        /// 消息内容
        /// </summary>
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(LeafToast),
            new PropertyMetadata(string.Empty)
        );

        /// <summary>
        /// 消息类型
        /// </summary>
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            nameof(Type),
            typeof(ToastType),
            typeof(LeafToast),
            new PropertyMetadata(ToastType.Info)
        );

        /// <summary>
        /// 图标
        /// </summary>
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(string),
            typeof(LeafToast),
            new PropertyMetadata(string.Empty)
        );

        /// <summary>
        /// 图标画刷
        /// </summary>
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(
            nameof(IconBrush),
            typeof(Brush),
            typeof(LeafToast),
            new PropertyMetadata(Brushes.Gray)
        );

        /// <summary>
        /// 是否显示关闭按钮
        /// </summary>
        public static readonly DependencyProperty ShowCloseButtonProperty =
            DependencyProperty.Register(
                nameof(ShowCloseButton),
                typeof(bool),
                typeof(LeafToast),
                new PropertyMetadata(true)
            );

        /// <summary>
        /// 是否显示确认和取消按钮
        /// </summary>
        public static readonly DependencyProperty ShowButtonsProperty = DependencyProperty.Register(
            nameof(ShowButtons),
            typeof(bool),
            typeof(LeafToast),
            new PropertyMetadata(false)
        );

        /// <summary>
        /// 确认按钮文本
        /// </summary>
        public static readonly DependencyProperty ConfirmStrProperty = DependencyProperty.Register(
            nameof(ConfirmStr),
            typeof(string),
            typeof(LeafToast),
            new PropertyMetadata("确认")
        );

        /// <summary>
        /// 取消按钮文本
        /// </summary>
        public static readonly DependencyProperty CancelStrProperty = DependencyProperty.Register(
            nameof(CancelStr),
            typeof(string),
            typeof(LeafToast),
            new PropertyMetadata("取消")
        );

        /// <summary>
        /// 显示时间
        /// </summary>
        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
            nameof(Time),
            typeof(DateTime),
            typeof(LeafToast),
            new PropertyMetadata(DateTime.Now)
        );

        /// <summary>
        /// 是否显示时间
        /// </summary>
        public static readonly DependencyProperty ShowDateTimeProperty =
            DependencyProperty.Register(
                nameof(ShowDateTime),
                typeof(bool),
                typeof(LeafToast),
                new PropertyMetadata(true)
            );

        #endregion

        #region Attached Properties

        /// <summary>
        /// Toast 容器父级
        /// </summary>
        public static readonly DependencyProperty ToastParentProperty =
            DependencyProperty.RegisterAttached(
                "ToastParent",
                typeof(bool),
                typeof(LeafToast),
                new PropertyMetadata(false, OnToastParentChanged)
            );

        /// <summary>
        /// Token 标识
        /// </summary>
        public static readonly DependencyProperty TokenProperty =
            DependencyProperty.RegisterAttached(
                "Token",
                typeof(string),
                typeof(LeafToast),
                new PropertyMetadata(string.Empty, OnTokenChanged)
            );

        /// <summary>
        /// 显示模式
        /// </summary>
        public static readonly DependencyProperty ShowModeProperty =
            DependencyProperty.RegisterAttached(
                "ShowMode",
                typeof(ToastShowMode),
                typeof(LeafToast),
                new FrameworkPropertyMetadata(
                    ToastShowMode.Append,
                    FrameworkPropertyMetadataOptions.Inherits
                )
            );

        private static readonly DependencyProperty IsCreatedAutomaticallyProperty =
            DependencyProperty.RegisterAttached(
                "IsCreatedAutomatically",
                typeof(bool),
                typeof(LeafToast),
                new PropertyMetadata(false)
            );

        #endregion

        #region Properties

        /// <summary>
        /// Toast 信息
        /// </summary>
        public LeafToastInfo ToastInfo
        {
            get => (LeafToastInfo)GetValue(ToastInfoProperty);
            set => SetValue(ToastInfoProperty, value);
        }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        /// <summary>
        /// 消息类型
        /// </summary>
        public ToastType Type
        {
            get => (ToastType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>
        /// 图标画刷
        /// </summary>
        public Brush IconBrush
        {
            get => (Brush)GetValue(IconBrushProperty);
            set => SetValue(IconBrushProperty, value);
        }

        /// <summary>
        /// 是否显示关闭按钮
        /// </summary>
        public bool ShowCloseButton
        {
            get => (bool)GetValue(ShowCloseButtonProperty);
            set => SetValue(ShowCloseButtonProperty, value);
        }

        /// <summary>
        /// 是否显示确认和取消按钮
        /// </summary>
        public bool ShowButtons
        {
            get => (bool)GetValue(ShowButtonsProperty);
            set => SetValue(ShowButtonsProperty, value);
        }

        /// <summary>
        /// 确认按钮文本
        /// </summary>
        public string ConfirmStr
        {
            get => (string)GetValue(ConfirmStrProperty);
            set => SetValue(ConfirmStrProperty, value);
        }

        /// <summary>
        /// 取消按钮文本
        /// </summary>
        public string CancelStr
        {
            get => (string)GetValue(CancelStrProperty);
            set => SetValue(CancelStrProperty, value);
        }

        /// <summary>
        /// 显示时间
        /// </summary>
        public DateTime Time
        {
            get => (DateTime)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        /// <summary>
        /// 是否显示时间
        /// </summary>
        public bool ShowDateTime
        {
            get => (bool)GetValue(ShowDateTimeProperty);
            set => SetValue(ShowDateTimeProperty, value);
        }

        /// <summary>
        /// Toast 容器
        /// </summary>
        public static Panel? ToastPanel { get; set; }

        /// <summary>
        /// 关闭前执行的操作
        /// </summary>
        private Func<bool, bool>? ActionBeforeClose { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// 关闭事件
        /// </summary>
        public event EventHandler? Closed;

        /// <summary>
        /// 确认事件
        /// </summary>
        public event EventHandler? Confirmed;

        /// <summary>
        /// 取消事件
        /// </summary>
        public event EventHandler? Cancelled;

        #endregion

        #region Constructor

        static LeafToast()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(LeafToast),
                new FrameworkPropertyMetadata(typeof(LeafToast))
            );
        }

        public LeafToast() { }

        #endregion

        #region Override Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // 移除旧事件处理
            if (_buttonConfirm != null)
            {
                _buttonConfirm.Click -= OnConfirmButtonClick;
            }

            if (_buttonCancel != null)
            {
                _buttonCancel.Click -= OnCancelButtonClick;
            }

            // 获取模板部件
            _gridMain = GetTemplateChild(ElementGridMain) as Grid;
            _buttonClose = GetTemplateChild(ElementButtonClose) as Button;
            _buttonPanel = GetTemplateChild(ElementButtonPanel) as Panel;
            _buttonConfirm = GetTemplateChild(ElementButtonConfirm) as Button;
            _buttonCancel = GetTemplateChild(ElementButtonCancel) as Button;

            if (_gridMain == null || _buttonClose == null || _buttonPanel == null)
            {
                throw new InvalidOperationException("模板部件未找到");
            }

            // 添加事件处理
            if (_buttonConfirm != null)
            {
                _buttonConfirm.Click += OnConfirmButtonClick;
            }

            if (_buttonCancel != null)
            {
                _buttonCancel.Click += OnCancelButtonClick;
            }
            if (_buttonClose != null)
            {
                _buttonClose.Click += OnCloseButtonClick;
            }

            Update();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (_buttonClose != null)
            {
                _buttonClose.Visibility = _showCloseButton
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (_buttonClose != null)
            {
                _buttonClose.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Event Handlers

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void OnConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            ToastInfo?.ConfirmAction?.Invoke();
            ToastInfo?.ConfirmCommand?.Execute(null);
            Confirmed?.Invoke(this, EventArgs.Empty);
            Close(true);
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            ToastInfo?.CancelAction?.Invoke();
            ToastInfo?.CancelCommand?.Execute(null);
            Cancelled?.Invoke(this, EventArgs.Empty);
            Close(false);
        }

        private static void OnToastInfoChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            if (d is LeafToast toast && e.NewValue is LeafToastInfo info)
            {
                toast.UpdateFromToastInfo(info);
            }
        }

        private static void OnTokenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Panel panel)
            {
                if (e.NewValue == null)
                {
                    Unregister(panel);
                }
                else
                {
                    Register(e.NewValue.ToString(), panel);
                }
            }
        }

        private static void OnToastParentChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            if ((bool)e.NewValue && d is Panel panel)
            {
                SetToastPanel(panel);
            }
        }

        #endregion

        #region Private Methods

        private void UpdateFromToastInfo(LeafToastInfo info)
        {
            Message = info.Message;
            Type = info.Type;
            Icon = info.Icon;
            ShowCloseButton = info.ShowCloseButton;
            ShowButtons = info.Type == ToastType.Ask; // 询问类型时显示按钮
            ConfirmStr = info.ConfirmStr;
            CancelStr = info.CancelStr;
            Time = DateTime.Now;
            _showCloseButton = info.ShowCloseButton;
            _staysOpen =
                info.Type == ToastType.Fatal || info.Type == ToastType.Ask || info.WaitTime == 0;
            _waitTime = Math.Max(info.WaitTime, MinWaitTime);

            ActionBeforeClose =
                info.Type == ToastType.Ask
                    ? (
                        result =>
                        {
                            if (result)
                            {
                                info.ConfirmAction?.Invoke();
                                return true;
                            }
                            else
                            {
                                info.CancelAction?.Invoke();
                                return true;
                            }
                        }
                    )
                    : null;

            // 根据类型设置图标画刷
            if (!string.IsNullOrEmpty(info.IconBrushKey))
            {
                var brush = TryFindResource(info.IconBrushKey) as Brush;
                if (brush != null)
                {
                    IconBrush = brush;
                }
            }
        }

        private void StartCloseTimer()
        {
            if (_staysOpen)
                return;

            _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
            _closeTimer.Tick += delegate
            {
                if (IsMouseOver)
                {
                    _tickCount = 0;
                    return;
                }

                _tickCount += 1000;
                if (_tickCount >= _waitTime)
                {
                    Close(true);
                }
            };
            _closeTimer.Start();
        }

        private void Update()
        {
            // 更新显示状态
            UpdateButtonsVisibility();

            // 播放动画和启动定时器
            PlayShowAnimation();
            if (!_staysOpen)
                StartCloseTimer();
        }

        /// <summary>
        /// 更新确认取消按钮的可见性
        /// </summary>
        private void UpdateButtonsVisibility()
        {
            if (_buttonPanel != null)
            {
                _buttonPanel.Visibility = ShowButtons ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void PlayShowAnimation()
        {
            if (_gridMain != null)
            {
                var transform = new TranslateTransform(0, -ActualHeight);
                _gridMain.RenderTransform = transform;

                var animation = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                };

                transform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
        }

        private void PlayHideAnimation()
        {
            if (_gridMain != null)
            {
                var transform =
                    _gridMain.RenderTransform as TranslateTransform ?? new TranslateTransform();
                _gridMain.RenderTransform = transform;

                var animation = new DoubleAnimation
                {
                    To = -ActualHeight,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn },
                };

                animation.Completed += (s, e) => RemoveFromParent();
                transform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
            else
            {
                RemoveFromParent();
            }
        }

        private void RemoveFromParent()
        {
            if (Parent is Panel parent)
            {
                parent.Children.Remove(this);

                // 关闭全局窗口（如果没有 Toast 了）
                if (_globalWindow != null)
                {
                    if (_globalWindow.ToastContainer.Children.Count == 0)
                    {
                        _globalWindow.Hide();
                    }
                }
                else if (
                    ToastPanel != null
                    && ToastPanel.Children.Count == 0
                    && GetIsCreatedAutomatically(ToastPanel)
                )
                {
                    // 移除自动创建的面板
                    RemoveDefaultPanel(ToastPanel);
                    ToastPanel = null;
                }
            }

            Closed?.Invoke(this, EventArgs.Empty);
        }

        private static void SetToastPanel(Panel panel)
        {
            ToastPanel = panel;
            InitToastPanel(panel);
        }

        public static void InitToastPanel(Panel panel)
        {
            if (panel == null)
                return;

            // 添加上下文菜单以便清除所有 Toast
            var menuItem = new MenuItem { Header = "清除全部" };
            menuItem.Click += (s, e) =>
            {
                foreach (var item in panel.Children.OfType<LeafToast>())
                {
                    item.Close();
                }
            };

            panel.ContextMenu = new ContextMenu { Items = { menuItem } };
        }

        private static void ShowInternal(Panel? panel, UIElement toast)
        {
            if (panel == null)
                return;

            var showMode = GetShowMode(panel);
            if (showMode == ToastShowMode.Prepend)
            {
                panel.Children.Insert(0, toast);
            }
            else
            {
                panel.Children.Add(toast);
            }
        }

        private static Panel? CreateDefaultPanel()
        {
            // 优先使用当前活动窗口,如果没有则使用主窗口
            FrameworkElement element = GetActiveWindow() ?? Application.Current.MainWindow;

            if (ToastPanel is not null&&ToastPanel.DataContext==element.DataContext)
            {
                return ToastPanel;
            }
            

            if (element == null)
                return null;
            var decorator = FindChild<AdornerDecorator>(element);

            if (decorator != null)
            {
                var layer = decorator.AdornerLayer;
                if (layer != null)
                {
                    var panel = new StackPanel
                    {
                        IsHitTestVisible = true,
                        VerticalAlignment = VerticalAlignment.Top,
                    };

                    InitToastPanel(panel);
                    SetIsCreatedAutomatically(panel, true);

                    var scrollViewer = new ScrollViewer
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                        IsEnabled = true,
                        IsHitTestVisible = true,
                        Content = panel,
                    };

                    var container = new AdornerContainer(layer)
                    {
                        IsHitTestVisible = false,
                        Child = scrollViewer,
                    };

                    layer.Add(container);

                    return panel;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取当前活动窗口
        /// </summary>
        private static Window? GetActiveWindow()
        {
            // 方法1: 获取当前激活的窗口
            foreach (Window window in Application.Current.Windows)
            {
                if (window.IsActive)
                {
                    return window;
                }
            }

            // 方法2: 获取焦点所在的窗口
            var focusedElement = Keyboard.FocusedElement as DependencyObject;
            if (focusedElement != null)
            {
                var window = Window.GetWindow(focusedElement);
                if (window != null)
                {
                    return window;
                }
            }

            // 方法3: 获取最后显示的窗口
            return Application.Current.Windows.OfType<Window>().LastOrDefault(w => w.IsVisible);
        }

        private static void RemoveDefaultPanel(Panel panel)
        {
            // 从面板查找其所属的窗口
            var window = Window.GetWindow(panel);
            if (window == null)
                return;

            var decorator = FindChild<AdornerDecorator>(window);

            if (decorator != null)
            {
                var layer = decorator.AdornerLayer;
                // 查找包含面板的 Adorner
                var parent = FindParent<Adorner>(panel);

                if (parent != null)
                {
                    layer?.Remove(parent);
                }
            }
        }

        private static T? FindChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            if (parent == null)
                return null;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found)
                {
                    return found;
                }

                var result = FindChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private static T? FindParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            DependencyObject current = child;
            while (current != null && !(current is T))
            {
                var parent = VisualTreeHelper.GetParent(current);
                current = parent;
            }
            return current as T;
        }

        private static void InitToastInfo(ref LeafToastInfo info, ToastType type)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            info.Type = type;

            switch (type)
            {
                case ToastType.Success:
                    info.Icon = "CheckCircleOutline";
                    info.IconBrushKey = "SuccessBrush";
                    break;
                case ToastType.Info:
                    info.Icon = "InformationOutline";
                    info.IconBrushKey = "InfoBrush";
                    break;
                case ToastType.Warning:
                    info.Icon = "AlertOutline";
                    info.IconBrushKey = "WarningBrush";
                    break;
                case ToastType.Error:
                    info.Icon = "CloseCircleOutline";
                    info.IconBrushKey = "ErrorBrush";
                    break;
                case ToastType.Fatal:
                    info.Icon = "AlertCircleOutline";
                    info.IconBrushKey = "FatalBrush";
                    info.WaitTime = 0; // 致命错误不自动关闭
                    info.ShowCloseButton = false;
                    break;
                case ToastType.Ask:
                    info.Icon = "HelpCircleOutline";
                    info.IconBrushKey = "AskBrush";
                    info.WaitTime = 0; // 询问消息不自动关闭
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 关闭 Toast
        /// </summary>
        public void Close(bool result = false)
        {
            if (ActionBeforeClose?.Invoke(result) == false)
            {
                return;
            }

            _closeTimer?.Stop();
            PlayHideAnimation();
        }

        /// <summary>
        /// 关闭 Toast
        /// </summary>
        public void Close()
        {
            Close(false);
        }

        #endregion

        #region Attached Property Methods

        public static bool GetToastParent(DependencyObject obj)
        {
            return (bool)obj.GetValue(ToastParentProperty);
        }

        public static void SetToastParent(DependencyObject obj, bool value)
        {
            obj.SetValue(ToastParentProperty, value);
        }

        public static string GetToken(DependencyObject obj)
        {
            return (string)obj.GetValue(TokenProperty);
        }

        public static void SetToken(DependencyObject obj, string value)
        {
            obj.SetValue(TokenProperty, value);
        }

        public static ToastShowMode GetShowMode(DependencyObject obj)
        {
            return (ToastShowMode)obj.GetValue(ShowModeProperty);
        }

        public static void SetShowMode(DependencyObject obj, ToastShowMode value)
        {
            obj.SetValue(ShowModeProperty, value);
        }

        private static void SetIsCreatedAutomatically(DependencyObject obj, bool value)
        {
            obj.SetValue(IsCreatedAutomaticallyProperty, value);
        }

        private static bool GetIsCreatedAutomatically(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsCreatedAutomaticallyProperty);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// 注册容器
        /// </summary>
        public static void Register(string? token, Panel panel)
        {
            if (string.IsNullOrEmpty(token) || panel == null)
                return;

            _tokenDictionary[token] = panel;
            InitToastPanel(panel);
        }

        /// <summary>
        /// 注销容器
        /// </summary>
        public static void Unregister(string token)
        {
            if (string.IsNullOrEmpty(token))
                return;

            if (_tokenDictionary.ContainsKey(token))
            {
                var panel = _tokenDictionary[token];
                _tokenDictionary.Remove(token);
                panel.ContextMenu = null;
            }
        }

        /// <summary>
        /// 注销容器
        /// </summary>
        public static void Unregister(Panel panel)
        {
            if (panel == null)
                return;

            var entry = _tokenDictionary.FirstOrDefault(kv => ReferenceEquals(kv.Value, panel));
            if (!string.IsNullOrEmpty(entry.Key))
            {
                _tokenDictionary.Remove(entry.Key);
                panel.ContextMenu = null;
            }
        }

        /// <summary>
        /// 清空指定容器中的所有 Toast
        /// </summary>
        public static void Clear(string? token = null)
        {
            if (!string.IsNullOrEmpty(token))
            {
                if (_tokenDictionary.TryGetValue(token, out var panel))
                {
                    Clear(panel);
                }
            }
            else
            {
                Clear(ToastPanel);
            }
        }

        /// <summary>
        /// 清空指定容器中的所有 Toast
        /// </summary>
        public static void Clear(Panel? panel)
        {
            panel?.Children.Clear();
        }

        /// <summary>
        /// 清空全局 Toast
        /// </summary>
        public static void ClearGlobal()
        {
            if (_globalWindow != null)
            {
                Clear(_globalWindow.ToastContainer);
                _globalWindow.Hide();
            }
        }

        /// <summary>
        /// 显示 Toast
        /// </summary>
        public static void Show(LeafToastInfo info, string? token = null)
        {
            Application.Current.Dispatcher?.Invoke(() =>
            {
                Panel? panel = null;

                if (!string.IsNullOrEmpty(token))
                {
                    _tokenDictionary.TryGetValue(token, out panel);
                }
                else
                {
                    // 使用默认面板
                    ToastPanel = CreateDefaultPanel();
                    panel = ToastPanel;
                }

                if (panel == null)
                {
                    // 如果没有找到容器，创建全局窗口
                    ShowGlobal(info);
                    return;
                }

                var toast = new LeafToast
                {
                    ToastInfo = info,
                    Time = DateTime.Now,
                    _showCloseButton = info.ShowCloseButton,
                    _staysOpen =
                        info.Type == ToastType.Fatal
                        || info.Type == ToastType.Ask
                        || info.WaitTime == 0,
                    _waitTime = Math.Max(info.WaitTime, MinWaitTime),
                    ShowDateTime = true,
                };

                ShowInternal(panel, toast);
            });
        }

        /// <summary>
        /// 显示成功消息
        /// </summary>
        public static void Success(string message, string? token = null)
        {
            var info = new LeafToastInfo { Message = message };
            InitToastInfo(ref info, ToastType.Success);
            Show(info, token);
        }

        /// <summary>
        /// 显示信息消息
        /// </summary>
        public static void Info(string message, string? token = null)
        {
            var info = new LeafToastInfo { Message = message };
            InitToastInfo(ref info, ToastType.Info);
            Show(info, token);
        }

        /// <summary>
        /// 显示警告消息
        /// </summary>
        public static void Warning(string message, string? token = null)
        {
            var info = new LeafToastInfo { Message = message };
            InitToastInfo(ref info, ToastType.Warning);
            Show(info, token);
        }

        /// <summary>
        /// 显示错误消息
        /// </summary>
        public static void Error(string message, string? token = null)
        {
            var info = new LeafToastInfo { Message = message };
            InitToastInfo(ref info, ToastType.Error);
            Show(info, token);
        }

        /// <summary>
        /// 显示致命错误消息
        /// </summary>
        public static void Fatal(string message, string? token = null)
        {
            var info = new LeafToastInfo { Message = message };
            InitToastInfo(ref info, ToastType.Fatal);
            Show(info, token);
        }

        /// <summary>
        /// 显示询问消息
        /// </summary>
        public static void Ask(
            string message,
            Action? confirmAction = null,
            Action? cancelAction = null,
            string? token = null
        )
        {
            var info = new LeafToastInfo
            {
                Message = message,
                ConfirmAction = confirmAction,
                CancelAction = cancelAction,
            };
            InitToastInfo(ref info, ToastType.Ask);
            Show(info, token);
        }

        /// <summary>
        /// 在全局窗口中显示 Toast
        /// </summary>
        public static void ShowGlobal(LeafToastInfo info)
        {
            Application.Current.Dispatcher?.Invoke(() =>
            {
                _globalWindow ??= LeafToastWindow.GetOrCreateGlobalWindow();
                _globalWindow.ShowToast(info);
            });
        }

        /// <summary>
        /// 显示成功消息（全局）
        /// </summary>
        public static void SuccessGlobal(string message)
        {
            var info = new LeafToastInfo { Message = message };
            InitToastInfo(ref info, ToastType.Success);
            ShowGlobal(info);
        }

        /// <summary>
        /// 显示信息消息（全局）
        /// </summary>
        public static void InfoGlobal(string message)
        {
            var info = new LeafToastInfo { Message = message };
            InitToastInfo(ref info, ToastType.Info);
            ShowGlobal(info);
        }

        /// <summary>
        /// 显示警告消息（全局）
        /// </summary>
        public static void WarningGlobal(string message)
        {
            var info = new LeafToastInfo { Message = message };
            InitToastInfo(ref info, ToastType.Warning);
            ShowGlobal(info);
        }

        /// <summary>
        /// 显示错误消息（全局）
        /// </summary>
        public static void ErrorGlobal(string message)
        {
            var info = new LeafToastInfo { Message = message };
            InitToastInfo(ref info, ToastType.Error);
            ShowGlobal(info);
        }

        /// <summary>
        /// 显示致命错误消息（全局）
        /// </summary>
        public static void FatalGlobal(string message)
        {
            var info = new LeafToastInfo { Message = message };
            InitToastInfo(ref info, ToastType.Fatal);
            ShowGlobal(info);
        }

        /// <summary>
        /// 显示询问消息（全局）
        /// </summary>
        public static void AskGlobal(
            string message,
            Action? confirmAction = null,
            Action? cancelAction = null
        )
        {
            var info = new LeafToastInfo
            {
                Message = message,
                ConfirmAction = confirmAction,
                CancelAction = cancelAction,
            };
            InitToastInfo(ref info, ToastType.Ask);
            ShowGlobal(info);
        }

        #endregion
    }

    /// <summary>
    /// Toast 显示模式
    /// </summary>
    public enum ToastShowMode
    {
        /// <summary>
        /// 追加
        /// </summary>
        Append,

        /// <summary>
        /// 前置
        /// </summary>
        Prepend,
    }
}
