using SolyankaGuide.Internals;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SolyankaGuide
{
    public partial class GuideControl : UserControl
    {

        internal static event Action<Element>? ShowGrid;
        internal static event Action<Element>? ShowDescription;

        public GuideControl()
        {
            InitializeComponent();
            CatergoriesControl.SwitchSidePanel += SwitchSidePanel;
        }

        private void VKLink(object sender, RoutedEventArgs e)
        {
            UrlOpener.OpenUrl("https://vk.com/narodnaya_solyanka");
        }

        private void DiscordLink(object sender, RoutedEventArgs e)
        {
            UrlOpener.OpenUrl("https://discord.gg/wd3tpW2fxH");
        }

        private void YoutubeLink(object sender, RoutedEventArgs e)
        {
            UrlOpener.OpenUrl("https://www.youtube.com/@nsogsr2023gid/videos");
        }

        private void SwitchSidePanel(Category? category)
        {
            if (category == null)
            {
                ((Storyboard)Resources["HideSidePanel"]).Begin();
                DescControl.Visibility = Visibility.Hidden;
                DescGridControl.Visibility = Visibility.Hidden;
                return;
            }
            Element[]? elements = JsonLoader.FillElements(category);
            if (elements == null)
            {
                Application.Current.Shutdown();
                return;
            }
            DescControl.Visibility = Visibility.Hidden;
            DescGridControl.Visibility = Visibility.Hidden;
            Elements.Children.Clear();
            for (int i = 0; i < elements.Length; i++)
            {
                Element element = elements[i]; ;
                RadioButton rb = new()
                {
                    Content = element.Name,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Style = (Style)FindResource("ElementsStyle")
                };
                rb.Click += (s, e) => OpenElement(element);
                Elements.Children.Add(rb);
                if (i != elements.Length - 1)
                {
                    Border b = new()
                    {
                        Height = 1,
                        Background = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
                        Margin = new Thickness(0, 4, 0, 4)
                    };
                    Elements.Children.Add(b);
                }
            }
            ((Storyboard)Resources["ShowSidePanel"]).Begin();
        }

        private void OpenElement(Element element)
        {
            if (element.Descriptions != null)
            {
                ShowGrid?.Invoke(element);
                DescControl.Visibility = Visibility.Hidden;
                DescGridControl.Visibility = Visibility.Visible;
                return;
            }
            ShowDescription?.Invoke(element);
            DescControl.Visibility = Visibility.Visible;
            DescGridControl.Visibility = Visibility.Hidden;
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            TopFade.Visibility = e.VerticalOffset > 0? Visibility.Visible : Visibility.Collapsed;
            BottomFade.Visibility = e.VerticalOffset + e.ViewportHeight < e.ExtentHeight? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
