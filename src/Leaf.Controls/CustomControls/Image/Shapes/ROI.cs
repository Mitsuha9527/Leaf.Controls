using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using CvCommon;

namespace Leaf.Controls.CustomControls
{
    public class ROI : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // 私有字段存储原始坐标
        private CvRect _originalRect;
        private CvRect _displayRect;
        private double _scale = 1.0;
        private bool _isUpdatingRect = false; // 防止循环更新

        /// <summary>
        /// 显示坐标的矩形（用于UI绑定，已缩放）
        /// </summary>
        public CvRect Rect
        {
            get => _displayRect;
            set
            {
                if (_isUpdatingRect)
                    return; // 防止循环更新

                if (SetField(ref _displayRect, value))
                {
                    _isUpdatingRect = true;
                    try
                    {
                        // 更新原始坐标
                        _originalRect = new CvRect(
                            value.X / _scale,
                            value.Y / _scale,
                            value.Width / _scale,
                            value.Height / _scale
                        );
                        OnPropertyChanged(nameof(OriginalRect));
                    }
                    finally
                    {
                        _isUpdatingRect = false;
                    }
                }
            }
        }

        /// <summary>
        /// 原始图像坐标的矩形（未缩放，用于业务逻辑）
        /// </summary>
        public CvRect OriginalRect
        {
            get => _originalRect;
            set
            {
                if (_isUpdatingRect)
                    return; // 防止循环更新

                if (SetField(ref _originalRect, value))
                {
                    _isUpdatingRect = true;
                    try
                    {
                        // 更新显示坐标
                        _displayRect = new CvRect(
                            value.X * _scale,
                            value.Y * _scale,
                            value.Width * _scale,
                            value.Height * _scale
                        );
                        OnPropertyChanged(nameof(Rect));
                    }
                    finally
                    {
                        _isUpdatingRect = false;
                    }
                }
            }
        }

        /// <summary>
        /// 当前的缩放比例
        /// </summary>
        public double Scale
        {
            get => _scale;
            set
            {
                if (SetField(ref _scale, value))
                {
                    _isUpdatingRect = true;
                    try
                    {
                        // 根据新的缩放比例重新计算显示坐标
                        _displayRect = new CvRect(
                            _originalRect.X * value,
                            _originalRect.Y * value,
                            _originalRect.Width * value,
                            _originalRect.Height * value
                        );
                        OnPropertyChanged(nameof(Rect));
                    }
                    finally
                    {
                        _isUpdatingRect = false;
                    }
                }
            }
        }

        private bool _isEditing;

        /// <summary>
        /// 是否处于编辑状态
        /// </summary>
        public bool IsEditing
        {
            get => _isEditing;
            set => SetField(ref _isEditing, value);
        }

        private bool _isSelected;

        /// <summary>
        /// 是否被选中
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        private bool? _isOK;
        public bool? IsOK
        {
            get => _isOK;
            set => SetField(ref _isOK, value);
        }

        private bool _showIndicator;

        /// <summary>
        /// 是否显示圆形指示物
        /// </summary>
        public bool ShowIndicator
        {
            get => _showIndicator;
            set => SetField(ref _showIndicator, value);
        }

        public string Code
        {
            get => field;
            set => SetField(ref field, value);
        } = string.Empty;

        /// <summary>
        /// 使用原始坐标创建ROI
        /// </summary>
        public static ROI FromOriginalRect(CvRect originalRect, double scale = 1.0)
        {
            var roi = new ROI();
            roi._scale = scale;
            roi._originalRect = originalRect;
            roi._displayRect = new CvRect(
                originalRect.X * scale,
                originalRect.Y * scale,
                originalRect.Width * scale,
                originalRect.Height * scale
            );
            return roi;
        }

        /// <summary>
        /// 使用显示坐标创建ROI
        /// </summary>
        public static ROI FromDisplayRect(CvRect displayRect, double scale = 1.0)
        {
            var roi = new ROI();
            roi._scale = scale;
            roi._displayRect = displayRect;
            roi._originalRect = new CvRect(
                displayRect.X / scale,
                displayRect.Y / scale,
                displayRect.Width / scale,
                displayRect.Height / scale
            );
            return roi;
        }

        /// <summary>
        /// 内部更新缩放比例的方法（用于批量更新时避免重复计算）
        /// </summary>
        internal void UpdateScale(double newScale)
        {
            if (Math.Abs(_scale - newScale) < 0.0001)
                return;

            _scale = newScale;
            _isUpdatingRect = true;
            try
            {
                _displayRect = new CvRect(
                    _originalRect.X * newScale,
                    _originalRect.Y * newScale,
                    _originalRect.Width * newScale,
                    _originalRect.Height * newScale
                );
                OnPropertyChanged(nameof(Scale));
                OnPropertyChanged(nameof(Rect));
            }
            finally
            {
                _isUpdatingRect = false;
            }
        }

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
    }
}
