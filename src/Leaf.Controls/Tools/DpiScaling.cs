using System.Windows;
using System.Windows.Media;

namespace Leaf.Controls.Tools
{
    public static class DpiScaling
    {
        /// <summary>
        /// 获取主屏幕的分辨率
        /// </summary>
        /// <returns></returns>
        public static (double Width, double Height) GetPrimaryScreenResolution()
        {
            return (SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
        }

        /// <summary>
        /// 获取当前窗口的DPI缩放比例
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        public static double GetDpiScale(Visual visual)
        {
            var source = PresentationSource.FromVisual(visual);
            return source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
        }

        /// <summary>
        /// 创建反向缩放变换
        /// </summary>
        /// <param name="visual">窗体</param>
        /// <returns></returns>
        public static ScaleTransform CreateReverseScaleTransform(Visual visual)
        {
            var dpiScale = GetDpiScale(visual);

            //获取当前屏幕分辨率和1920*1080的比例
            var (width, height) = GetPrimaryScreenResolution();
            var scale = Math.Min(1920 / width/ dpiScale, 1080 / height/ dpiScale);
            scale *= dpiScale;
            return new ScaleTransform(1 / scale, 1 / scale);
        }

        /// <summary>
        /// Transforms the visual element to match the physical dimensions provided.
        /// </summary>
        /// <param name="visual">The visual element to be transformed.</param>
        /// <param name="physicalWidth">The physical width to scale to.</param>
        /// <param name="physicalHeight">The physical height to scale to.</param>
        /// <param name="rootVisual">The root visual element to apply the transformation to.</param>
        public static void TransformVisual(Visual visual, double physicalWidth, double physicalHeight, FrameworkElement rootVisual)
        {
            var scaleTransform = CreateReverseScaleTransform(visual);
            rootVisual.LayoutTransform = scaleTransform;
            if (visual is Window w)
            {
                w.Width = physicalWidth * scaleTransform.ScaleX;
                w.Height = physicalHeight * scaleTransform.ScaleY;
                w.Left = (SystemParameters.PrimaryScreenWidth - w.Width) / 2;
                w.Top = (SystemParameters.PrimaryScreenHeight - w.Height) / 2;
            }
        }
    }
}
