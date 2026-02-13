using SolyankaGuide.Internals;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SolyankaGuide
{
    public partial class DescriptionControl : UserControl
    {

        public static event Action ShowGrid;

        public DescriptionControl()
        {
            InitializeComponent();
            GuideControl.ShowDescription += ShowDescription;
            GuideControl.ShowElement += ShowDescription;
            DescriptionGridControl.ShowDescription += ShowDescription;
            DescScrollView.SizeChanged += (s, e) =>
            {
                if (DescScrollView.Content != null) ((TextBlock)DescScrollView.Content).Width = e.NewSize.Width;
            };
        }

        private void ShowDescription(Element element)
        {
            BackButton.Visibility = Visibility.Hidden;
            BitmapImage? bitmap = ImageLoader.LoadImage(element.ImagePath);
            if (bitmap != null)
            {
                DescImage.Source = bitmap;
                DescImage.Stretch = Stretch.UniformToFill;
            }
            DescHeader.Text = element.Header;
            var textBlock = TextGen.GetText(element.Text!, element.Centered, DescScrollView.ActualWidth);
            DescScrollView.Content = textBlock;
            DescScrollView.ScrollToTop();
        }

        private void ShowDescription(Description desc)
        {
            BackButton.Visibility = Visibility.Visible;
            BitmapImage? bitmap = ImageLoader.LoadImage(desc.ImagePath);
            DescImage.Source = bitmap;
            DescImage.Stretch = Stretch.UniformToFill;
            DescHeader.Text = desc.Header;
            var textBlock = TextGen.GetText(desc.Text!, desc.Centered, DescScrollView.ActualWidth);
            DescScrollView.Content = textBlock;
            DescScrollView.ScrollToTop();
            Visibility = Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
            ShowGrid?.Invoke();
        }

        private void Image_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool over = (bool)e.NewValue;
            ((Image)sender).OpacityMask = over? Brushes.Yellow : Brushes.White;

        }
    }
}
