using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CvCommon;
using Leaf.Controls.Utilities;

namespace Leaf.Controls.CustomControls
{
    /// <summary>
    /// 提供图像查看与交互功能的自定义控件，支持缩放、平移和像素
    /// 以及在图像上绘制和交互各种形状
    /// </summary>
    [TemplatePart(Name = ElementImage, Type = typeof(Image))]
    [TemplatePart(Name = ElementPanel, Type = typeof(Panel))]
    public class ImageViewer : Control, IDisposable
    {
        #region Contants
        private const string ElementImage = "PART_Image";
        private const string ElementPanel = "PART_Panel";

        /// <summary>
        /// 缩放比间隔
        /// </summary>
        private const double ScaleInternal = 0.2;

        #endregion
        static ImageViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ImageViewer),
                new FrameworkPropertyMetadata(typeof(ImageViewer))
            );
        }

        #region 私有字段

        private bool _isDisposed = false;

        /// <summary>
        /// 根LayoutPanel
        /// </summary>
        private Panel _mainPanel = null!;

        /// <summary>
        /// 图像显示控件
        /// </summary>
        private Image _mainImage = null!;

        /// <summary>
        /// 图片原始宽度
        /// </summary>
        private double _oriImageWidth;

        /// <summary>
        /// 图片原始高度
        /// </summary>
        private double _oriImageHeight;

        /// <summary>
        /// 图片实际位置
        /// </summary>
        private Thickness _imgActualMargin;

        /// <summary>
        /// 图片实际旋缩放比
        /// </summary>
        private double _imgActualScale = 1;

        /// <summary>
        /// 在图片上鼠标移动时的即时位置
        /// </summary>
        private Point _imgCurrentPoint;

        /// <summary>
        /// 鼠标是否在图片上按下
        /// </summary>
        private bool _imgIsMouseDown;

        /// <summary>
        /// 在图片上按下时图片的位置
        /// </summary>
        private Thickness _imgMouseDownMargin;

        /// <summary>
        /// 在图片上按下时鼠标的位置
        /// </summary>
        private Point _imgMouseDownPoint;

        // --- ROI 绘制相关字段 ---
        private InteractionMode _modeBeforePan;
        private bool _isDrawing = false;
        private bool _isDraggingRoi = false;
        private Point _roiStartPoint;
        private Point _roiDragStartPoint;
        private ROI? _previewRoi;
        private ROI? _selectedRoi;
        // -----------------------

        #endregion

        #region 构造
        public ImageViewer()
        {
            // 注册加载事件
            Loaded += ImageView_Loaded;
            ROIs = [];
            AddHandler(
                Thumb.DragDeltaEvent,
                new DragDeltaEventHandler(ResizeThumb_DragDelta),
                true
            );
            Focusable = true;
        }

        /// <summary>
        /// 带一个图片Uri的构造函数
        /// </summary>
        /// <param name="uri"></param>
        public ImageViewer(Uri uri)
            : this()
        {
            Uri = uri;
        }

        /// <summary>
        /// 带一个图片路径的构造函数
        /// </summary>
        /// <param name="path"></param>
        public ImageViewer(string path)
            : this(new Uri(path)) { }
        #endregion

        #region 依赖属性
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            nameof(ImageSource),
            typeof(BitmapFrame),
            typeof(ImageViewer),
            new PropertyMetadata(default(BitmapFrame), OnImageSourceChanged)
        );
        public static readonly DependencyProperty UriProperty = DependencyProperty.Register(
            nameof(Uri),
            typeof(Uri),
            typeof(ImageViewer),
            new PropertyMetadata(default(Uri), OnUriChanged)
        );

        public static readonly DependencyProperty ROIsProperty = DependencyProperty.Register(
            nameof(ROIs),
            typeof(ObservableCollection<ROI>),
            typeof(ImageViewer),
            new PropertyMetadata(default)
        );

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
            nameof(Mode),
            typeof(InteractionMode),
            typeof(ImageViewer),
            new PropertyMetadata(InteractionMode.Pan, OnModeChanged)
        );

        internal static readonly DependencyProperty ImagePathProperty = DependencyProperty.Register(
            nameof(ImagePath),
            typeof(string),
            typeof(ImageViewer),
            new PropertyMetadata(default(string))
        );
        internal static readonly DependencyProperty ImgSizeProperty = DependencyProperty.Register(
            nameof(ImgSize),
            typeof(long),
            typeof(ImageViewer),
            new PropertyMetadata(-1L)
        );

        internal static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register(
                nameof(ImageWidth),
                typeof(double),
                typeof(ImageViewer),
                new PropertyMetadata(0.0)
            );
        internal static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register(
                nameof(ImageHeight),
                typeof(double),
                typeof(ImageViewer),
                new PropertyMetadata(0.0)
            );
        internal static readonly DependencyProperty ImageMarginProperty =
            DependencyProperty.Register(
                nameof(ImageMargin),
                typeof(Thickness),
                typeof(ImageViewer),
                new PropertyMetadata(new Thickness(0))
            );
        public static readonly DependencyProperty ImageScaleProperty = DependencyProperty.Register(
            nameof(ImageScale),
            typeof(double),
            typeof(ImageViewer),
            new PropertyMetadata(ValueBoxes.Double1Box, OnImageScaleChanged)
        );
        public BitmapFrame? ImageSource
        {
            get { return (BitmapFrame)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }
        public Uri Uri
        {
            get { return (Uri)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        public ObservableCollection<ROI> ROIs
        {
            get { return (ObservableCollection<ROI>)GetValue(ROIsProperty); }
            set { SetValue(ROIsProperty, value); }
        }
        public InteractionMode Mode
        {
            get { return (InteractionMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }
        internal string ImagePath
        {
            get { return (string)GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }
        internal long ImgSize
        {
            get { return (long)GetValue(ImgSizeProperty); }
            set { SetValue(ImgSizeProperty, value); }
        }
        internal double ImageWidth
        {
            get { return (double)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }
        internal double ImageHeight
        {
            get { return (double)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }
        internal Thickness ImageMargin
        {
            get { return (Thickness)GetValue(ImageMarginProperty); }
            set { SetValue(ImageMarginProperty, value); }
        }
        public double ImageScale
        {
            get { return (double)GetValue(ImageScaleProperty); }
            set { SetValue(ImageScaleProperty, value); }
        }
        #endregion

        #region 模板与事件

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            // 获取模板中的元素
            _mainImage = (GetTemplateChild(ElementImage) as Image)!;
            _mainPanel = (GetTemplateChild(ElementPanel) as Panel)!;
        }

        private void ImageView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ImageSource is not null)
                Init();
        }

        #endregion

        #region 依赖属性回调

        private static void OnImageSourceChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            ((ImageViewer)d).OnImageSourceChanged();
        }

        private void OnImageSourceChanged()
        {
            Init();
        }

        private static void OnImageScaleChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            if (d is ImageViewer imageViewer && e.NewValue is double newValue)
            {
                imageViewer.ImageWidth = imageViewer._oriImageWidth * newValue;
                imageViewer.ImageHeight = imageViewer._oriImageHeight * newValue;
                imageViewer.UpdateROIs(newValue);
            }
        }

        private static void OnUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ImageViewer)d).OnUriChanged((Uri)e.NewValue);
        }

        private void OnUriChanged(Uri newValue)
        {
            ImageSource = newValue is not null ? GetBitmapFrame(newValue) : null;
            if (ImageSource is not null && newValue!.IsAbsoluteUri)
            {
                ImagePath = newValue.AbsolutePath;
                if (File.Exists(ImagePath))
                {
                    ImgSize = new FileInfo(ImagePath).Length;
                }
            }
            else
            {
                ImagePath = string.Empty;
                ImgSize = 0;
            }

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
            var viewer = d as ImageViewer;
            if (viewer == null)
                return;
            if (e.NewValue is InteractionMode newMode)
            {
                if (newMode != InteractionMode.Edit)
                    if (viewer._selectedRoi != null)
                    {
                        viewer._selectedRoi.IsEditing = false;
                        viewer._selectedRoi = null;
                    }
            }
        }

        #endregion

        #region 鼠标键盘事件处理

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            FitImageToView();
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_selectedRoi == null || _imgActualScale == 0)
                return;

            if (e.OriginalSource is not Thumb thumb)
                return;
            double dx = e.HorizontalChange;
            double dy = e.VerticalChange;
            var rect = _selectedRoi.Rect;
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
            // 防止矩形翻转或尺寸为负
            if (rect.Width > 0 && rect.Height > 0)
            {
                _selectedRoi.Rect = rect;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Delete)
            {
                if (_selectedRoi is not null)
                {
                    ROIs.Remove(_selectedRoi);
                    _selectedRoi = null;
                }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e) => ScaleImage(e.Delta > 0);

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
                        _imgCurrentPoint = e.GetPosition(this); // 使用 this 作为参考，因为 Margin 是相对于父级的
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
                    if (_isDraggingRoi && _selectedRoi != null)
                    {
                        Point currentPoint = e.GetPosition(_mainPanel);

                        double dx = currentPoint.X - _roiDragStartPoint.X;
                        double dy = currentPoint.Y - _roiDragStartPoint.Y;

                        var currentRect = _selectedRoi.Rect;
                        _selectedRoi.Rect = new CvRect(
                            currentRect.X + dx,
                            currentRect.Y + dy,
                            currentRect.Width,
                            currentRect.Height
                        );
                        // 更新拖动起始点，为下一次MouseMove做准备
                        _roiDragStartPoint = currentPoint;
                    }
                    break;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            // 检测鼠标中键点击
            if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 2)
            {
                // 执行自适应调整
                FitImageToView();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            this.CaptureMouse();
            switch (Mode)
            {
                case InteractionMode.DrawROI:
                    Point startPoint = e.GetPosition(_mainPanel);
                    _roiStartPoint = startPoint;
                    _previewRoi = new ROI { Rect = new CvRect(startPoint.X, startPoint.Y, 0, 0) };
                    ROIs.Add(_previewRoi);
                    _isDrawing = true;
                    break;
                case InteractionMode.Edit:
                    this.Focus(); // 获取焦点以接收键盘事件
                    Point clickPoint = e.GetPosition(_mainPanel);
                    // 从最上层的ROI开始查找（后添加的在集合末尾）
                    var clickedRoi = ROIs.LastOrDefault(roi =>
                        roi.Rect.Contains(clickPoint.X, clickPoint.Y)
                    );
                    // 如果点击在已选中的ROI上，则准备拖动
                    if (clickedRoi != null)
                    {
                        if (_selectedRoi != clickedRoi)
                        {
                            if (_selectedRoi != null)
                                _selectedRoi.IsEditing = false;
                            _selectedRoi = clickedRoi;
                            _selectedRoi.IsEditing = true;
                        }
                        _isDraggingRoi = true;
                        _roiDragStartPoint = clickPoint; // 记录拖动起始点（像素坐标）
                    }
                    else // 如果点击在空白处，则取消选择
                    {
                        if (_selectedRoi != null)
                        {
                            _selectedRoi.IsEditing = false;
                            _selectedRoi = null;
                        }
                    }
                    break;
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            switch (Mode)
            {
                case InteractionMode.DrawROI:
                    if (_isDrawing && _previewRoi != null)
                    {
                        // 可选：如果绘制的矩形太小，可以将其移除
                        if (
                            _previewRoi.Rect.Width / _imgActualScale < 15
                            || _previewRoi.Rect.Height / _imgActualScale < 15
                        )
                        {
                            ROIs.Remove(_previewRoi);
                        }
                    }
                    _isDrawing = false;
                    _previewRoi = null;
                    break;
                case InteractionMode.Edit:
                    _isDraggingRoi = false; // 停止拖动
                    break;
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            this.CaptureMouse();
            _modeBeforePan = Mode;
            Mode = InteractionMode.Pan;
            _imgMouseDownPoint = e.GetPosition(this); // 使用 this 作为参考
            _imgMouseDownMargin = ImageMargin;
            _imgIsMouseDown = true;
        }

        protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
        {
            this.CaptureMouse();
            _imgIsMouseDown = false;
            Mode = _modeBeforePan;
        }

        #endregion

        #region 坐标转换方法

        private (int, int) TranslatePanelPointToPixel(CvPoint panelPoint)
        {
            int pixelX = (int)(panelPoint.X / _imgActualScale);
            int pixelY = (int)(panelPoint.Y / _imgActualScale);
            return (pixelX, pixelY);
        }

        #endregion


        private void Init()
        {
            // 首次初始化使用新方法
            UpdateImageWithCurrentScale();
        }

        /// <summary>
        /// 在保留当前缩放比例的情况下更新图像
        /// </summary>
        private void UpdateImageWithCurrentScale()
        {
            if (ActualHeight == 0 || ActualWidth == 0 || ImageSource is null)
                return;

            // 保存之前的缩放比例，如果是首次加载则使用默认值
            double previousScale = _imgActualScale;
            bool isFirstLoad = _oriImageWidth == 0 || _oriImageHeight == 0;

            // 保存当前图像在视图中的相对位置
            double relativePositionX = 0.5; // 默认居中
            double relativePositionY = 0.5; // 默认居中

            // 如果不是首次加载，计算图像中心相对于视图的位置
            if (!isFirstLoad && ImageWidth > 0 && ImageHeight > 0)
            {
                // 计算显示区域中心点
                double viewCenterX = ActualWidth / 2;
                double viewCenterY = ActualHeight / 2;

                // 计算图像中心点
                double imageCenterX = _imgActualMargin.Left + ImageWidth / 2;
                double imageCenterY = _imgActualMargin.Top + ImageHeight / 2;

                // 计算相对位置（作为比例）
                relativePositionX = (viewCenterX - imageCenterX) / ImageWidth + 0.5;
                relativePositionY = (viewCenterY - imageCenterY) / ImageHeight + 0.5;

                // 确保值在有效范围内
                relativePositionX = Math.Max(0, Math.Min(1, relativePositionX));
                relativePositionY = Math.Max(0, Math.Min(1, relativePositionY));
            }

            // 更新图像原始尺寸
            var width = ImageSource.PixelWidth;
            var height = ImageSource.PixelHeight;
            _oriImageWidth = width;
            _oriImageHeight = height;
            if (Math.Abs(height - 0) < 0.001 || Math.Abs(width - 0) < 0.001)
            {
                MessageBox.Show("Error ImageSize");
                return;
            }

            // 如果是首次加载，则计算适合视图的缩放比例
            if (isFirstLoad)
            {
                var imgWidHeiScale = width / height;
                var winWidHeiScale = ActualWidth / ActualHeight;
                ImageScale = 1;

                if (imgWidHeiScale > winWidHeiScale)
                {
                    if (width > ActualWidth)
                    {
                        ImageScale = ActualWidth / width;
                    }
                }
                else if (height > ActualHeight)
                {
                    ImageScale = ActualHeight / height;
                }
            }
            else
            {
                // 保持之前的缩放比例
                ImageScale = previousScale;
            }

            // 计算新的图像边距，保持相对位置不变
            double newLeft;
            double newTop;

            if (isFirstLoad)
            {
                // 首次加载，居中显示
                newLeft = (ActualWidth - ImageWidth) / 2;
                newTop = (ActualHeight - ImageHeight) / 2;
            }
            else
            {
                // 使用相对位置计算新的边距
                newLeft = ActualWidth / 2 - (relativePositionX * ImageWidth);
                newTop = ActualHeight / 2 - (relativePositionY * ImageHeight);

                // 确保图像不会完全移出视图
                newLeft = Math.Min(ActualWidth - 50, Math.Max(-ImageWidth + 50, newLeft));
                newTop = Math.Min(ActualHeight - 50, Math.Max(-ImageHeight + 50, newTop));
            }

            ImageMargin = new Thickness(newLeft, newTop, 0, 0);
            _imgActualScale = ImageScale;
            _imgActualMargin = ImageMargin;
        }

        /// <summary>
        /// 自动调整图像大小以适应视图
        /// </summary>
        private void FitImageToView()
        {
            if (ImageSource is null || ActualWidth <= 0 || ActualHeight <= 0)
                return;
            // 计算适合视图的缩放比例
            double scaleX = ActualWidth / _oriImageWidth;
            double scaleY = ActualHeight / _oriImageHeight;

            // 选择较小的值以确保整个图像都可见
            ImageScale = Math.Min(scaleX, scaleY);

            // 居中显示
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

            // 缩放比例限制
            if (Math.Abs(tempScale) < 0.01 || Math.Abs(tempScale) > 20)
                return;

            var posCanvas = Mouse.GetPosition(_mainPanel);
            // 根坐标系下，鼠标相对于图片左上角的位置
            var posImg = new CvPoint(posCanvas.X - ImageMargin.Left, posCanvas.Y - ImageMargin.Top);

            // 计算缩放后图片相对于鼠标位置的偏移
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
            if (ROIs is null)
                return;
            foreach (var roi in ROIs)
            {
                var rect = roi.Rect;
                roi.Rect = new CvRect(
                    rect.X / _imgActualScale * scale,
                    rect.Y / _imgActualScale * scale,
                    rect.Width / _imgActualScale * scale,
                    rect.Height / _imgActualScale * scale
                );
            }
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    ImageSource = null;
                    _mainImage.Source = null;
                    _mainImage.UpdateLayout();
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
