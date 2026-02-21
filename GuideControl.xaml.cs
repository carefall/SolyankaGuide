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
        internal static event Action<Element>? ShowElement;
        internal static event Action<Description>? ShowDescription;
        internal static event Action<Category, Element, Description?>? OpenElementWithHyper;

        public GuideControl()
        {
            InitializeComponent();
            CatergoriesControl.SwitchSidePanel += SwitchSidePanel;
            CatergoriesControl.SwitchSidePanelWithHyper += SwitchSidePanelWithHyper;
            TextGen.SwitchDescription += SwitchDescription;
        }

        private void SwitchDescription(string category, string listName, int elementIndex, int descriptionIndex)
        {
            Category? cat = CatergoriesControl.GetCategory(category);
            if (cat == null)
            {
                Logger.Log("Guide", $"Hyperlink was directing to unknown category {category}");
                MessageBox.Show(Locale.Get("category_not_found"), Locale.Get("guide"), MessageBoxButton.OK);
                return;
            }
            var list = cat.Lists!.FirstOrDefault(e => e!.Name.Split("/").Last() == listName, null);
            if (list == null)
            {
                Logger.Log("Guide", $"Hyperlink was directing to non-existing element list {listName} in category {category}");
                MessageBox.Show(Locale.Get("category_path_not_found"), Locale.Get("guide"), MessageBoxButton.OK);
                return;
            }
            if (elementIndex >= list.Elements!.Length)
            {
                Logger.Log("Guide", $"Hyperlink was directing to non-existing element with index {elementIndex} in list {list.Name} in category {category}");
                MessageBox.Show(Locale.Get("path_element_not_found"), Locale.Get("guide"), MessageBoxButton.OK);
                return;
            }
            Element element = list.Elements![elementIndex];
            if (element.Descriptions == null)
            {
                OpenElementWithHyper?.Invoke(cat, element, null);
            }
            else
            {
                if (descriptionIndex >= element.Descriptions.Length)
                {
                    Logger.Log("Guide", $"Hyperlink was directing to non-existing description of element {element.Name} in list {list.Name} in category {category}");
                    MessageBox.Show(Locale.Get("element_description_not_found"), Locale.Get("guide"), MessageBoxButton.OK);
                    return;
                }
                if (descriptionIndex == -1)
                {
                    OpenElementWithHyper?.Invoke(cat, element, null);
                    return;
                }
                Description desc = element.Descriptions[descriptionIndex];
                OpenElementWithHyper?.Invoke(cat, element, desc);
            }
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

        private Element[] JoinElements(ElementList[] lists)
        {
            int totalLength = lists.Sum(arr => arr.Elements!.Length);
            Element[] combinedArray = new Element[totalLength];
            int position = 0;
            foreach (var array in lists)
            {
                Array.Copy(array.Elements!, 0, combinedArray, position, array.Elements!.Length);
                position += array.Elements!.Length;
            }
            return combinedArray;
        }


        private void SwitchSidePanelWithHyper(Category category, Element element, Description? description)
        {
            Element[] elements = JoinElements(category.Lists!);
            DescControl.Visibility = Visibility.Hidden;
            DescGridControl.Visibility = Visibility.Hidden;
            Elements.Children.Clear();
            Element? target = null;
            for (int i = 0; i < elements.Length; i++)
            {
                Element elem = elements[i];
                RadioButton rb = new()
                {
                    Content = elem.Name,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Style = (Style)FindResource("ElementsStyle")
                };
                rb.Click += (s, e) => OpenElement(elem);
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
                if (elem.Name == element.Name)
                {
                    target = elem;
                    rb.IsChecked = true;
                }
            }
            if (description == null)
            {
                if (target!.Descriptions == null || target.Descriptions.Length == 0)
                {
                    ShowElement?.Invoke(target);
                    DescControl.Visibility = Visibility.Visible;
                    DescControl.Focus();
                    DescGridControl.Visibility = Visibility.Hidden;
                } else
                {
                    ShowGrid?.Invoke(target);
                    DescControl.Visibility = Visibility.Hidden;
                    DescGridControl.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ShowDescription?.Invoke(description);
            }
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
            Element[] elements = JoinElements(category.Lists!);
            DescControl.Visibility = Visibility.Hidden;
            DescGridControl.Visibility = Visibility.Hidden;
            Elements.Children.Clear();
            for (int i = 0; i < elements.Length; i++)
            {
                Element element = elements[i];
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
            ShowElement?.Invoke(element);
            DescControl.Visibility = Visibility.Visible;
            DescControl.Focus();
            DescGridControl.Visibility = Visibility.Hidden;
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            TopFade.Visibility = e.VerticalOffset > 0 ? Visibility.Visible : Visibility.Collapsed;
            BottomFade.Visibility = e.VerticalOffset + e.ViewportHeight < e.ExtentHeight ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
