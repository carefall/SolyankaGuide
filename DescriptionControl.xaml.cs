using SolyankaGuide.Internals;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SolyankaGuide
{
    public partial class DescriptionControl : UserControl
    {
        public DescriptionControl()
        {
            InitializeComponent();
            GuideControl.ShowDescription += ShowDescription;
            DescriptionGridControl.ShowDescription += ShowDescription;
            DescScrollView.SizeChanged += (s, e) =>
            {
                if (DescScrollView.Content != null) ((TextBlock)DescScrollView.Content).Width = e.NewSize.Width;
            };
        }

        private void ShowDescription(Element element)
        {
            BitmapImage? bitmap = ImageLoader.LoadImage(element.ImagePath);
            if (bitmap != null)
            {
                DescImage.Source = bitmap;
                double h = bitmap.PixelHeight;
                double w = bitmap.PixelWidth;
                if (h != 360)
                {
                    double factor = h / 360;
                    w /= factor;
                    h = 360;
                }
                DescImage.Width = w;
                DescImage.Height = h;
                DescImage.Stretch = Stretch.Uniform;
            }
            DescHeader.Text = element.Header;
            var textBlock = TextGen.GetText(element.Text!, element.Centered, DescScrollView.ActualWidth);
            DescScrollView.Content = textBlock;
            DescScrollView.ScrollToTop();
        }

        private void ShowDescription(Description desc)
        {
            BitmapImage? bitmap = ImageLoader.LoadImage(desc.ImagePath);
            if (bitmap != null)
            {
                DescImage.Source = bitmap;
                DescImage.Width = bitmap.PixelWidth;
                DescImage.Height = bitmap.PixelHeight;
                DescImage.Stretch = Stretch.Uniform;
            }
            DescHeader.Text = desc.Header;
            var textBlock = TextGen.GetText(desc.Text!, desc.Centered, DescScrollView.ActualWidth);
            DescScrollView.Content = textBlock;
            DescScrollView.ScrollToTop();
            Visibility = Visibility.Visible;
        }


    }
}
