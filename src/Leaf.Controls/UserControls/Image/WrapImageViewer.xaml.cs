using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using CvCommon;
using Leaf.Controls.CustomControls;
using Leaf.Controls.UserControls.Image;
using Leaf.Controls.UserControls.Image.Shape;

namespace Leaf.Controls.UserControls
{
    /// <summary>
    /// 增强版图像查看器，包含集成的工具栏和状态栏
    /// </summary>
    public partial class WrapImageViewer : UserControl, INotifyPropertyChanged, IDisposable
    {
        #region 私有字段
        private bool _isDisposed = false;
        private Panel _mainPanel = null!;
        private System.Windows.Controls.Image _mainImage = null!;

        // 图像相关字段
        private double _oriImageWidth;
        private double _oriImageHeight;
        private Thickness _imgActualMargin;
        private double _imgActualScale = 1;

        // 鼠标交互字段
        private Point _imgCurrentPoint;
        private bool _imgIsMouseDown;
        private Thickness _imgMouseDownMargin;
        private Point _imgMouseDownPoint;

        // ROI 绘制相关字段
        private InteractionMode _modeBeforePan;
        private bool _isDrawing = false;
        private bool _isDraggingRoi = false;
        private Point _roiStartPoint;
        private Point _roiDragStartPoint;
        private ROI? _previewRoi;
        private Key _pressKey;
        #endregion

        #region 构造函数
        public WrapImageViewer()
        {
            InitializeComponent();

            ROIs = [];

            // 注册事件
            Loaded += WrapImageViewer_Loaded;

            // 注册Thumb拖拽事件
            AddHandler(
                Thumb.DragDeltaEvent,
                new DragDeltaEventHandler(ResizeThumb_DragDelta),
                true
            );

            // 设置焦点
            Focusable = true;

            // 更新按钮状态
            UpdateModeButtons();
        }

        /// <summary>
        /// 带图片URI的构造函数
        /// </summary>
        public WrapImageViewer(Uri uri)
            : this()
        {
            Uri = uri;
        }

        /// <summary>
        /// 带图片路径的构造函数
        /// </summary>
        public WrapImageViewer(string path)
            : this(new Uri(path)) { }
        #endregion



        #region 依赖属性
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            nameof(ImageSource),
            typeof(BitmapFrame),
            typeof(WrapImageViewer),
            new PropertyMetadata(default(BitmapFrame), OnImageSourceChanged)
        );

        public static readonly DependencyProperty UriProperty = DependencyProperty.Register(
            nameof(Uri),
            typeof(Uri),
            typeof(WrapImageViewer),
            new PropertyMetadata(default(Uri), OnUriChanged)
        );

        public static readonly DependencyProperty ROIsProperty = DependencyProperty.Register(
            nameof(ROIs),
            typeof(ObservableCollection<ROI>),
            typeof(WrapImageViewer),
            new PropertyMetadata(default(ObservableCollection<ROI>), OnROIsChanged)
        );

        public static readonly DependencyProperty SelectedROIProperty = DependencyProperty.Register(
            nameof(SelectedROI),
            typeof(ROI),
            typeof(WrapImageViewer),
            new PropertyMetadata(default)
        );

        public static readonly DependencyProperty ROIManipulationCommandProperty =
            DependencyProperty.Register(
                nameof(ROIManipulationCommand),
                typeof(ICommand),
                typeof(WrapImageViewer),
                new PropertyMetadata(null)
            );

        public static readonly DependencyProperty ROIAdjustCommandProperty =
            DependencyProperty.Register(
                nameof(ROIAdjustCommand),
                typeof(ICommand),
                typeof(WrapImageViewer),
                new PropertyMetadata(null)
            );

        public static readonly DependencyProperty ShowToolbarProperty = DependencyProperty.Register(
            nameof(ShowToolbar),
            typeof(bool),
            typeof(WrapImageViewer),
            new PropertyMetadata(true)
        );

        public static readonly DependencyProperty CodeTextBrushProperty =
            DependencyProperty.Register(
                nameof(CodeTextBrush),
                typeof(Brush),
                typeof(WrapImageViewer),
                new PropertyMetadata(Brushes.Yellow)
            );

        public static readonly DependencyProperty CodeTextFontSizeProperty =
            DependencyProperty.Register(
                nameof(CodeTextFontSize),
                typeof(double),
                typeof(WrapImageViewer),
                new PropertyMetadata(12.0)
            );

        public static readonly DependencyProperty IsCodeCenteredInRoiProperty =
            DependencyProperty.Register(
                nameof(IsCodeCenteredInRoi),
                typeof(bool),
                typeof(WrapImageViewer),
                new PropertyMetadata(false)
            );

        public static readonly DependencyProperty CodeCenterFillRatioProperty =
            DependencyProperty.Register(
                nameof(CodeCenterFillRatio),
                typeof(double),
                typeof(WrapImageViewer),
                new PropertyMetadata(0.92)
            );

        public static readonly DependencyProperty ImageScaleProperty = DependencyProperty.Register(
            nameof(ImageScale),
            typeof(double),
            typeof(WrapImageViewer),
            new PropertyMetadata(0.0, OnImageScaleChanged)
        );

        internal static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register(
                nameof(ImageWidth),
                typeof(double),
                typeof(WrapImageViewer),
                new PropertyMetadata(0.0)
            );

        internal static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register(
                nameof(ImageHeight),
                typeof(double),
                typeof(WrapImageViewer),
                new PropertyMetadata(0.0)
            );

        internal static readonly DependencyProperty ImageMarginProperty =
            DependencyProperty.Register(
                nameof(ImageMargin),
                typeof(Thickness),
                typeof(WrapImageViewer),
                new PropertyMetadata(new Thickness(0))
            );
        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
            nameof(Mode),
            typeof(InteractionMode),
            typeof(WrapImageViewer),
            new PropertyMetadata(InteractionMode.Pan, OnModeChanged)
        );
        internal static readonly DependencyProperty HasSelectedROIProperty =
            DependencyProperty.Register(
                nameof(HasSelectedROI),
                typeof(bool),
                typeof(WrapImageViewer),
                new PropertyMetadata(false)
            );

        public static readonly DependencyProperty SingleModeOnlyProperty =
            DependencyProperty.Register(
                nameof(SingleModeOnly),
                typeof(bool),
                typeof(WrapImageViewer),
                new PropertyMetadata(false, OnSingleModeOnlyChanged)
            );

        private static void OnSingleModeOnlyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            throw new NotImplementedException();
        }

        // 属性访问器
        public BitmapFrame? ImageSource
        {
            get => (BitmapFrame)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public Uri? Uri
        {
            get => (Uri)GetValue(UriProperty);
            set => SetValue(UriProperty, value);
        }

        public ObservableCollection<ROI> ROIs
        {
            get => (ObservableCollection<ROI>)GetValue(ROIsProperty);
            set => SetValue(ROIsProperty, value);
        }
        public ROI? SelectedROI
        {
            get => (ROI?)GetValue(SelectedROIProperty);
            set => SetValue(SelectedROIProperty, value);
        }
        public ICommand? ROIManipulationCommand
        {
            get { return (ICommand?)GetValue(ROIManipulationCommandProperty); }
            set { SetValue(ROIManipulationCommandProperty, value); }
        }

        public ICommand? ROIAdjustCommand
        {
            get { return (ICommand?)GetValue(ROIAdjustCommandProperty); }
            set { SetValue(ROIAdjustCommandProperty, value); }
        }

        public bool ShowToolbar
        {
            get => (bool)GetValue(ShowToolbarProperty);
            set => SetValue(ShowToolbarProperty, value);
        }

        public Brush CodeTextBrush
        {
            get => (Brush)GetValue(CodeTextBrushProperty);
            set => SetValue(CodeTextBrushProperty, value);
        }

        public double CodeTextFontSize
        {
            get => (double)GetValue(CodeTextFontSizeProperty);
            set => SetValue(CodeTextFontSizeProperty, value);
        }

        public bool IsCodeCenteredInRoi
        {
            get => (bool)GetValue(IsCodeCenteredInRoiProperty);
            set => SetValue(IsCodeCenteredInRoiProperty, value);
        }

        public double CodeCenterFillRatio
        {
            get => (double)GetValue(CodeCenterFillRatioProperty);
            set => SetValue(CodeCenterFillRatioProperty, value);
        }

        public double ImageScale
        {
            get => (double)GetValue(ImageScaleProperty);
            set => SetValue(ImageScaleProperty, value);
        }
        internal double ImageWidth
        {
            get => (double)GetValue(ImageWidthProperty);
            set => SetValue(ImageWidthProperty, value);
        }

        internal double ImageHeight
        {
            get => (double)GetValue(ImageHeightProperty);
            set => SetValue(ImageHeightProperty, value);
        }

        internal Thickness ImageMargin
        {
            get => (Thickness)GetValue(ImageMarginProperty);
            set => SetValue(ImageMarginProperty, value);
        }
        public InteractionMode Mode
        {
            get => (InteractionMode)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        internal bool HasSelectedROI
        {
            get => (bool)GetValue(HasSelectedROIProperty);
            set => SetValue(HasSelectedROIProperty, value);
        }
        public bool SingleModeOnly
        {
            get { return (bool)GetValue(SingleModeOnlyProperty); }
            set { SetValue(SingleModeOnlyProperty, value); }
        }

        #endregion

        #region 事件处理
        private void WrapImageViewer_Loaded(object sender, RoutedEventArgs e)
        {
            _mainPanel = PART_Panel;
            _mainImage = PART_Image;

            if (ImageSource != null)
                Init();
        }

        #region 工具栏按钮事件
        private void PanModeButton_Click(object sender, RoutedEventArgs e)
        {
            Mode = InteractionMode.Pan;
        }

        private void DrawROIModeButton_Click(object sender, RoutedEventArgs e)
        {
            Mode = InteractionMode.DrawROI;
        }

        private void EditModeButton_Click(object sender, RoutedEventArgs e)
        {
            Mode = InteractionMode.Edit;
        }

        private void ClickModeButton_Click(object sender, RoutedEventArgs e)
        {
            Mode = InteractionMode.Select;
        }

        private void FitToViewButton_Click(object sender, RoutedEventArgs e)
        {
            FitImageToView();
        }

        private void ClearROIsButton_Click(object sender, RoutedEventArgs e)
        {
            if (LeafNotificationBox.ShowYesNo("确定要清除所有ROI吗？") is true)
            {
                ROIs.Clear();
                SelectedROI = null;
                HasSelectedROI = false;
            }
        }

        private void DeleteSelectedROIButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedROI != null)
            {
                ROIs.Remove(SelectedROI);
                SelectedROI = null;
                HasSelectedROI = false;
            }
        }

        private void AdjustROIPosition_Click(object sender, RoutedEventArgs e)
        {
            var adjustWindow = new ROIAdjustWindow
            {
                MoveUp = (value) => MoveROis(-value, 0),
                MoveLeft = (value) => MoveROis(0, -value),
                MoveRight = (value) => MoveROis(0, value),
                MoveDown = (value) => MoveROis(value, 0),
            };
            adjustWindow.ShowDialog();
            ROIAdjustCommand?.Execute(null);
        }

        private void MoveROis(double updown, double leftright)
        {
            foreach (var roi in ROIs)
            {
                var oldRect = roi.OriginalRect;
                roi.OriginalRect = new CvRect(
                    oldRect.X + leftright,
                    oldRect.Y + updown,
                    oldRect.Width,
                    oldRect.Height
                );
            }
        }

        #endregion

        #endregion

        #region 依赖属性回调
        private static void OnImageSourceChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            ((WrapImageViewer)d).OnImageSourceChanged();
        }

        private void OnImageSourceChanged()
        {
            Init();
        }

        private static void OnUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WrapImageViewer)d).OnUriChanged((Uri)e.NewValue);
        }

        private void OnUriChanged(Uri? newValue)
        {
            ImageSource = newValue != null ? GetBitmapFrame(newValue) : null;

            static BitmapFrame? GetBitmapFrame(Uri source)
            {
                try
                {
                    return BitmapFrame.Create(source);
                }
                catch
                {
                    return null;
                }
            }
        }

        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (WrapImageViewer)d;
            viewer.OnModeChanged((InteractionMode)e.NewValue);
        }

        private void OnModeChanged(InteractionMode newMode)
        {
            // 取消之前模式的状态
            if (newMode != InteractionMode.Pan && ROIs is not null)
            {
                foreach (var roi in ROIs)
                {
                    roi.IsEditing = false;
                    roi.IsSelected = false;
                }
            }

            if (newMode != InteractionMode.Edit && SelectedROI != null)
            {
                SelectedROI.IsEditing = false;
            }
            if (newMode != InteractionMode.Select && SelectedROI != null)
            {
                SelectedROI.IsSelected = false;
            }
            if (newMode != InteractionMode.Edit && newMode != InteractionMode.Select)
            {
                SelectedROI = null;
                HasSelectedROI = false;
            }

            UpdateModeButtons();
        }

        private static void OnImageScaleChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            if (d is WrapImageViewer viewer && e.NewValue is double newValue)
            {
                if (double.IsNaN(newValue) || double.IsInfinity(newValue) || newValue <= 0)
                    return;

                viewer.ImageWidth = viewer._oriImageWidth * newValue;
                viewer.ImageHeight = viewer._oriImageHeight * newValue;
                viewer.UpdateROIs(newValue);
            }
        }

        private static void OnROIsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (WrapImageViewer)d;

            if (e.OldValue is ObservableCollection<ROI> oldCollection)
            {
                oldCollection.CollectionChanged -= viewer.ROIs_CollectionChanged;
            }

            if (e.NewValue is ObservableCollection<ROI> newCollection)
            {
                newCollection.CollectionChanged += viewer.ROIs_CollectionChanged;
            }
        }

        private void ROIs_CollectionChanged(
            object? sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e
        ) { }
        #endregion

        #region 鼠标和键盘事件
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (ImageSource != null)
                FitImageToView();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            ScaleImage(e.Delta > 0);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (
                e.LeftButton != MouseButtonState.Pressed
                && e.RightButton != MouseButtonState.Pressed
            )
                return;

            switch (Mode)
            {
                case InteractionMode.Pan:
                    if (_imgIsMouseDown)
                    {
                        _imgCurrentPoint = e.GetPosition(this);
                        var dx = _imgCurrentPoint.X - _imgMouseDownPoint.X;
                        var dy = _imgCurrentPoint.Y - _imgMouseDownPoint.Y;
                        ImageMargin = new Thickness(
                            _imgMouseDownMargin.Left + dx,
                            _imgMouseDownMargin.Top + dy,
                            0,
                            0
                        );
                        _imgActualMargin = ImageMargin;
                    }
                    break;

                case InteractionMode.DrawROI:
                    if (_isDrawing && _previewRoi != null)
                    {
                        Point currentPoint = e.GetPosition(_mainPanel);
                        var newRect = new CvRect(
                            new CvPoint(_roiStartPoint.X, _roiStartPoint.Y),
                            new CvPoint(currentPoint.X, currentPoint.Y)
                        );
                        _previewRoi.Rect = newRect;
                    }
                    break;

                case InteractionMode.Edit:
                    if (_isDraggingRoi && SelectedROI != null)
                    {
                        Point currentPoint = e.GetPosition(_mainPanel);
                        double dx = currentPoint.X - _roiDragStartPoint.X;
                        double dy = currentPoint.Y - _roiDragStartPoint.Y;

                        var currentRect = SelectedROI.Rect;
                        SelectedROI.Rect = new CvRect(
                            currentRect.X + dx,
                            currentRect.Y + dy,
                            currentRect.Width,
                            currentRect.Height
                        );
                        _roiDragStartPoint = currentPoint;
                    }
                    break;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 2)
            {
                FitImageToView();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            this.CaptureMouse();

            if (_pressKey == Key.LeftCtrl)
            {
                var pos = e.GetPosition(_mainPanel);
                var clickedRoi = ROIs.LastOrDefault(roi => roi.Rect.Contains(pos.X, pos.Y));
                if (clickedRoi != null)
                {
                    var copyRoi = ROI.FromOriginalRect(clickedRoi.OriginalRect, _imgActualScale);
                    clickedRoi.IsSelected = false;
                    clickedRoi.IsEditing = false;
                    copyRoi.Code = $"{ROIs.Count + 1}";
                    _previewRoi = copyRoi;
                    ROIs.Add(_previewRoi);
                    SelectedROI = _previewRoi;
                    SelectedROI.IsEditing = true;
                    Mode = InteractionMode.Edit;
                    ROIManipulationCommand?.Execute(SelectedROI);
                }
                else
                    Mode = InteractionMode.DrawROI;
            }

            switch (Mode)
            {
                case InteractionMode.Pan:
                    _imgMouseDownPoint = e.GetPosition(this);
                    _imgMouseDownMargin = ImageMargin;
                    _imgIsMouseDown = true;
                    break;
                case InteractionMode.DrawROI:
                    Point startPoint = e.GetPosition(_mainPanel);
                    _roiStartPoint = startPoint;
                    // 使用FromDisplayRect创建ROI
                    _previewRoi = ROI.FromDisplayRect(
                        new CvRect(startPoint.X, startPoint.Y, 0, 0),
                        _imgActualScale
                    );
                    _previewRoi.Code = $"{ROIs.Count + 1}";
                    ROIs.Add(_previewRoi);
                    _isDrawing = true;
                    break;

                case InteractionMode.Edit:
                    this.Focus();
                    Point clickPoint = e.GetPosition(_mainPanel);
                    var clickedRoi = ROIs.LastOrDefault(roi =>
                        roi.Rect.Contains(clickPoint.X, clickPoint.Y)
                    );

                    if (clickedRoi != null)
                    {
                        if (SelectedROI != clickedRoi)
                        {
                            if (SelectedROI != null)
                                SelectedROI.IsEditing = false;
                            SelectedROI = clickedRoi;
                            SelectedROI.IsEditing = true;
                            HasSelectedROI = true;
                        }
                        _isDraggingRoi = true;
                        _roiDragStartPoint = clickPoint;
                        ROIManipulationCommand?.Execute(SelectedROI);
                    }
                    else
                    {
                        if (SelectedROI != null)
                        {
                            SelectedROI.IsEditing = false;
                            SelectedROI = null;
                            HasSelectedROI = false;
                        }
                    }
                    break;
                case InteractionMode.Select:
                    clickPoint = e.GetPosition(_mainPanel);
                    clickedRoi = ROIs.LastOrDefault(roi =>
                        roi.Rect.Contains(clickPoint.X, clickPoint.Y)
                    );
                    if (clickedRoi is not null)
                    {
                        if (SelectedROI != clickedRoi)
                        {
                            if (SelectedROI != null)
                                SelectedROI.IsSelected = false;
                            SelectedROI = clickedRoi;
                            SelectedROI.IsSelected = true;
                            HasSelectedROI = true;
                        }
                    }
                    else
                    {
                        if (SelectedROI != null)
                        {
                            SelectedROI.IsSelected = false;
                            SelectedROI = null;
                            HasSelectedROI = false;
                        }
                    }
                    ROIManipulationCommand?.Execute(SelectedROI);
                    break;
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();

            switch (Mode)
            {
                case InteractionMode.Pan:
                    _imgIsMouseDown = false;
                    break;

                case InteractionMode.DrawROI:
                    if (_isDrawing && _previewRoi != null)
                    {
                        if (
                            _previewRoi.Rect.Width / _imgActualScale < 15
                            || _previewRoi.Rect.Height / _imgActualScale < 15
                        )
                        {
                            ROIs.Remove(_previewRoi);
                        }
                        else //绘制结束就切换成选中模式
                        {
                            SelectedROI = _previewRoi;
                            Mode = InteractionMode.Edit;
                            SelectedROI.IsEditing = true;   
                            ROIManipulationCommand?.Execute(SelectedROI);
                        }
                    }
                    _isDrawing = false;
                    _previewRoi = null;
                    break;

                case InteractionMode.Edit:
                    ROIManipulationCommand?.Execute(SelectedROI);
                    _isDraggingRoi = false;
                    break;
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            this.CaptureMouse();
            _modeBeforePan = Mode;
            Mode = InteractionMode.Pan;
            _imgMouseDownPoint = e.GetPosition(this);
            _imgMouseDownMargin = ImageMargin;
            _imgIsMouseDown = true;
        }

        protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            _imgIsMouseDown = false;
            Mode = _modeBeforePan;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Delete && SelectedROI != null)
            {
                ROIs.Remove(SelectedROI);
                SelectedROI = null;
                HasSelectedROI = false;
            }
            _pressKey = e.Key;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _pressKey = Key.None;
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (SelectedROI == null || _imgActualScale == 0)
                return;

            if (e.OriginalSource is not Thumb thumb)
                return;

            double dx = e.HorizontalChange;
            double dy = e.VerticalChange;
            var rect = SelectedROI.Rect;

            switch (thumb.Tag as string)
            {
                case "TopLeft":
                    rect.X += dx;
                    rect.Y += dy;
                    rect.Width -= dx;
                    rect.Height -= dy;
                    break;
                case "TopCenter":
                    rect.Y += dy;
                    rect.Height -= dy;
                    break;
                case "TopRight":
                    rect.Y += dy;
                    rect.Width += dx;
                    rect.Height -= dy;
                    break;
                case "MiddleLeft":
                    rect.X += dx;
                    rect.Width -= dx;
                    break;
                case "MiddleRight":
                    rect.Width += dx;
                    break;
                case "BottomLeft":
                    rect.X += dx;
                    rect.Width -= dx;
                    rect.Height += dy;
                    break;
                case "BottomCenter":
                    rect.Height += dy;
                    break;
                case "BottomRight":
                    rect.Width += dx;
                    rect.Height += dy;
                    break;
            }

            if (rect.Width > 0 && rect.Height > 0)
            {
                SelectedROI.Rect = rect;
            }
        }
        #endregion

        #region 私有方法
        private void Init()
        {
            UpdateImageWithCurrentScale();
        }

        private void UpdateImageWithCurrentScale()
        {
            if (ActualHeight == 0 || ActualWidth == 0 || ImageSource == null)
                return;

            double previousScale = _imgActualScale;
            bool isFirstLoad = _oriImageWidth == 0 || _oriImageHeight == 0;

            double relativePositionX = 0.5;
            double relativePositionY = 0.5;

            if (!isFirstLoad && ImageWidth > 0 && ImageHeight > 0)
            {
                double viewCenterX = ActualWidth / 2;
                double viewCenterY = ActualHeight / 2;
                double imageCenterX = _imgActualMargin.Left + ImageWidth / 2;
                double imageCenterY = _imgActualMargin.Top + ImageHeight / 2;

                relativePositionX = (viewCenterX - imageCenterX) / ImageWidth + 0.5;
                relativePositionY = (viewCenterY - imageCenterY) / ImageHeight + 0.5;

                relativePositionX = Math.Max(0, Math.Min(1, relativePositionX));
                relativePositionY = Math.Max(0, Math.Min(1, relativePositionY));
            }

            var width = ImageSource.PixelWidth;
            var height = ImageSource.PixelHeight;
            bool isImageSizeChanged =
                Math.Abs(width - _oriImageWidth) > 0.001
                || Math.Abs(height - _oriImageHeight) > 0.001;
            _oriImageWidth = width;
            _oriImageHeight = height;

            if (Math.Abs(height - 0) < 0.001 || Math.Abs(width - 0) < 0.001)
            {
                MessageBox.Show("Error ImageSize");
                return;
            }

            if (isFirstLoad || isImageSizeChanged)
            {
                var imgWidHeiScale = width / height;
                var winWidHeiScale = ActualWidth / ActualHeight;
                ImageScale = 1;

                if (imgWidHeiScale > winWidHeiScale)
                {
                    if (width > ActualWidth)
                        ImageScale = ActualWidth / width;
                }
                else if (height > ActualHeight)
                {
                    ImageScale = ActualHeight / height;
                }
            }
            else
            {
                ImageScale = previousScale;
            }

            double newLeft,
                newTop;

            if (isFirstLoad)
            {
                newLeft = (ActualWidth - ImageWidth) / 2;
                newTop = (ActualHeight - ImageHeight) / 2;
            }
            else
            {
                newLeft = ActualWidth / 2 - (relativePositionX * ImageWidth);
                newTop = ActualHeight / 2 - (relativePositionY * ImageHeight);

                newLeft = Math.Min(ActualWidth - 50, Math.Max(-ImageWidth + 50, newLeft));
                newTop = Math.Min(ActualHeight - 50, Math.Max(-ImageHeight + 50, newTop));
            }

            ImageMargin = new Thickness(newLeft, newTop, 0, 0);
            _imgActualScale = ImageScale;
            _imgActualMargin = ImageMargin;
        }

        private void FitImageToView()
        {
            if (
                ImageSource == null
                || ActualWidth <= 0
                || ActualHeight <= 0
                || _oriImageWidth <= 0
                || _oriImageHeight <= 0
            )
                return;

            double scaleX = ActualWidth / _oriImageWidth;
            double scaleY = ActualHeight / _oriImageHeight;

            if (
                double.IsNaN(scaleX)
                || double.IsInfinity(scaleX)
                || double.IsNaN(scaleY)
                || double.IsInfinity(scaleY)
            )
                return;

            ImageScale = Math.Min(scaleX, scaleY);

            ImageMargin = new Thickness(
                (ActualWidth - ImageWidth) / 2,
                (ActualHeight - ImageHeight) / 2,
                0,
                0
            );
            _imgActualMargin = ImageMargin;
            _imgActualScale = ImageScale;
        }

        private void ScaleImage(bool isEnlarge)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                return;

            var tempScale = isEnlarge ? _imgActualScale * 1.1 : _imgActualScale * 0.9;

            if (Math.Abs(tempScale) < 0.01 || Math.Abs(tempScale) > 20)
                return;

            var posCanvas = Mouse.GetPosition(_mainPanel);
            var posImg = new CvPoint(posCanvas.X - ImageMargin.Left, posCanvas.Y - ImageMargin.Top);

            var posAfterScaleImg = new CvPoint(
                posCanvas.X / _imgActualScale * tempScale - posCanvas.X,
                posCanvas.Y / _imgActualScale * tempScale - posCanvas.Y
            );

            ImageScale = tempScale;

            var thickness = new Thickness(
                ImageMargin.Left - posAfterScaleImg.X,
                ImageMargin.Top - posAfterScaleImg.Y,
                0,
                0
            );
            ImageMargin = thickness;
            _imgActualMargin = ImageMargin;
            _imgActualScale = ImageScale;
        }

        private void UpdateROIs(double scale)
        {
            if (ROIs == null || double.IsInfinity(scale) || double.IsNaN(scale) || scale <= 0)
                return;

            // 使用内部方法批量更新，避免重复计算
            foreach (var roi in ROIs)
            {
                roi.UpdateScale(scale);
            }
        }

        private void UpdateModeButtons()
        {
            if (!IsLoaded)
                return;

            PanModeButton.IsChecked = Mode == InteractionMode.Pan;
            DrawROIModeButton.IsChecked = Mode == InteractionMode.DrawROI;
            EditModeButton.IsChecked = Mode == InteractionMode.Edit;
            ClickModeButton.IsChecked = Mode == InteractionMode.Select;
        }

        #endregion

        #region INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(
            ref T field,
            T value,
            [CallerMemberName] string? propertyName = null
        )
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        #region IDisposable 实现
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    ImageSource = null;
                    if (_mainImage != null)
                    {
                        _mainImage.Source = null;
                        _mainImage.UpdateLayout();
                    }
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
