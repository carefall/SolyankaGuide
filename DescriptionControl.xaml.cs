using SolyankaGuide.Internals;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

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
            var bitmap = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/" + element.ImagePath, UriKind.Absolute));
            DescImage.Source = bitmap;
            DescImage.Width = bitmap.PixelWidth;
            DescImage.Height = bitmap.PixelHeight;
            DescImage.Stretch = Stretch.Uniform;
            DescHeader.Text = element.Header;
            var textBlock = TextGen.GetText(element.Text!, element.Centered, DescScrollView.ActualWidth);
            DescScrollView.Content = textBlock;
            DescScrollView.ScrollToTop();
        }

        private void ShowDescription(Description desc)
        {
            var bitmap = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/" + desc.ImagePath, UriKind.Absolute));
            DescImage.Source = bitmap;
            DescImage.Width = bitmap.PixelWidth;
            DescImage.Height = bitmap.PixelHeight;
            DescImage.Stretch = Stretch.Uniform;
            DescHeader.Text = desc.Header;
            var textBlock = TextGen.GetText(desc.Text!, desc.Centered, DescScrollView.ActualWidth);
            DescScrollView.Content = textBlock;
            DescScrollView.ScrollToTop();
            Visibility = Visibility.Visible;
        }


    }
}
