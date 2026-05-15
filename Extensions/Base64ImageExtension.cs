using System.IO;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Leaf.Controls.Extensions
{
    public class Base64ImageExtension : MarkupExtension
    {
        public string? Base64 { get; set; }

        public Base64ImageExtension() { }

        public Base64ImageExtension(string base64)
        {
            Base64 = base64;
        }
        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Base64))
                return null;

            byte[] imageBytes = Convert.FromBase64String(Base64);
            using (var ms = new MemoryStream(imageBytes))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // 使BitmapImage可跨线程访问
                return bitmapImage;
            }
        }
    }
}
