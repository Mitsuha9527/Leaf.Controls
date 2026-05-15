using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Leaf.Controls.CustomControls
{
    /// <summary>
    /// Toast 全局窗口
    /// </summary>
    public class LeafToastWindow : Window
    {
        #region Fields

        private static readonly List<LeafToastWindow> _windowList = new List<LeafToastWindow>();
        private StackPanel _toastContainer;

        #endregion

        #region Properties

        /// <summary>
        /// Toast 容器
        /// </summary>
        public Panel ToastContainer => _toastContainer;

        #endregion

        #region Constructor

        public LeafToastWindow()
        {
            InitializeWindow();
            InitializeContainer();
            _windowList.Add(this);
        }

        #endregion

        #region Private Methods

        private void InitializeWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            Topmost = true;
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;

            // 设置窗口位置
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - 400; // 距离右边距 400 像素
            Top = workArea.Top + 50;     // 距离顶部 50 像素
        }

        [MemberNotNull(nameof(_toastContainer))]
        private void InitializeContainer()
        {
            _toastContainer = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                MinWidth = 300,
                MaxWidth = 400
            };

            // 初始化 Toast 容器
            LeafToast.SetShowMode(_toastContainer, ToastShowMode.Prepend);
            LeafToast.InitToastPanel(_toastContainer);

            Content = _toastContainer;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 显示 Toast
        /// </summary>
        public void ShowToast(LeafToastInfo info)
        {
            var toast = new LeafToast
            {
                ToastInfo = info,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 0, 10)
            };

            toast.Closed += OnToastClosed;
            
            if (LeafToast.GetShowMode(_toastContainer) == ToastShowMode.Prepend)
            {
                _toastContainer.Children.Insert(0, toast);
            }
            else
            {
                _toastContainer.Children.Add(toast);
            }

            if (!IsVisible)
            {
                Show();
                UpdateWindowPosition();
            }
        }

        private void OnToastClosed(object? sender, EventArgs e)
        {
            if (sender is LeafToast toast)
            {
                toast.Closed -= OnToastClosed;
                _toastContainer.Children.Remove(toast);
            }

            // 如果没有 Toast 了，隐藏窗口
            if (_toastContainer.Children.Count == 0)
            {
                Hide();
            }
            else
            {
                UpdateWindowPosition();
            }
        }

        private void UpdateWindowPosition()
        {
            UpdateLayout();

            // 更新窗口大小
            SizeToContent = SizeToContent.WidthAndHeight;

            // 确保窗口在屏幕范围内
            var workArea = SystemParameters.WorkArea;
            if (Left + ActualWidth > workArea.Right)
                Left = workArea.Right - ActualWidth;
            if (Top + ActualHeight > workArea.Bottom)
                Top = workArea.Bottom - ActualHeight;
        }

        protected override void OnClosed(EventArgs e)
        {
            _windowList.Remove(this);
            base.OnClosed(e);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// 获取或创建全局 Toast 窗口
        /// </summary>
        public static LeafToastWindow GetOrCreateGlobalWindow()
        {
            var window = _windowList.FirstOrDefault();
            if (window == null)
            {
                window = new LeafToastWindow();
            }
            return window;
        }

        /// <summary>
        /// 关闭所有全局 Toast 窗口
        /// </summary>
        public static void CloseAllGlobalWindows()
        {
            var windows = _windowList.ToList();
            foreach (var window in windows)
            {
                window.Close();
            }
        }

        #endregion
    }
}
