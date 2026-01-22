using System.IO;
using System.Windows.Media.Imaging;

namespace SolyankaGuide.Internals
{
    internal static class ImageLoader
    {
        public static BitmapImage LoadImage(string path)
        {
			BitmapImage bitmap = new BitmapImage();
			using FileStream stream = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + $"/Assets/Images/{path}");
			bitmap.BeginInit();
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.StreamSource = stream;
			bitmap.EndInit();
			bitmap.Freeze();
			return bitmap;
        }
    }
}
