using System.Windows.Media.Imaging;

namespace SolyankaGuide.Internals
{
    class IconStorage
    {
        private static Dictionary<int, BitmapImage> icons = new();

        static IconStorage()
        {
            icons[0] = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/icon0.png", UriKind.Absolute));
        }

        public static BitmapImage GetById(int id)
        {
            return icons[id];
        }
            
    }
}
