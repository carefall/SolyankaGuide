using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SolyankaGuide.Internals
{
    internal static class ImageLoader
    {

        private static readonly BitmapImage placeholder = new();

        public static void SetupPlaceholder()
        {
            placeholder.BeginInit();
            placeholder.CacheOption = BitmapCacheOption.OnLoad;
            placeholder.UriSource = new Uri("pack://application:,,,/Internals/Images/placeholder.png", UriKind.Absolute);
            placeholder.EndInit();
            placeholder.Freeze();
        }

        public static BitmapImage LoadImage(string? path)
        {
            BitmapImage bitmap = new();
            if (path == null || path.Trim().Length == 0)
            {
                Logger.Log("ImageLoader", "No photo path provided.");
                return placeholder.Clone();
            }
            try
            {
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + $"/Assets/Images/{path}")) 
                {
                    Logger.Log("ImageLoader", "No photo on provided path");
                    return placeholder.Clone();
                }
                using FileStream stream = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + $"/Assets/Images/{path}");
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex)
            {
                Logger.Log("ImageLoader", ex.ToString());
                return placeholder.Clone();
            }
        }
    }
}
