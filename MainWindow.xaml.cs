using SolyankaGuide.Internals;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace SolyankaGuide
{
    public partial class MainWindow : Window
    {

        private ToggleButton? _activeTopButton;
        private Dictionary<int, List<Description>> _sidePanelItems = new Dictionary<int, List<Description>>();
        private static MainWindow? instance;

        public static MainWindow? GetInstance()
        {
            return instance; 
        }

        public MainWindow()
        {
            InitializeComponent();
            instance = this;
            var data = JsonLoader.FillFromJson();
            if (data == null)
            {
                Close();
            }
            foreach (var key in data.Keys)
            {
                _sidePanelItems.Add(int.Parse(key), data[key]);
            }
            DescriptionScrollViewer.SizeChanged += (s, e) =>
            {
                if (DescriptionScrollViewer.Content != null)
                    ((TextBlock)DescriptionScrollViewer.Content).Width = e.NewSize.Width;
            };
        }

        private void DiscordLink(object sender, RoutedEventArgs e)
        {
            UrlOpener.OpenUrl("https://discord.gg/wd3tpW2fxH");
        }

        private void YoutubeLink(object sender, RoutedEventArgs e)
        {
            UrlOpener.OpenUrl("https://www.youtube.com/@nsogsr2023gid/videos");
        }

        #region TitleBar Buttons
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && WindowState != WindowState.Minimized)
                DragMove();
        }

        private void ResizeButton(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void MinimizeButton(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из программы?", "Выход", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Close();
            }
        }
        #endregion

        #region Top Navigation Buttons
        private void Helper(object sender, RoutedEventArgs e) => ShowSidePanel(sender, 0);
        private void Characters(object sender, RoutedEventArgs e) => ShowSidePanel(sender, 1);
        private void Items(object sender, RoutedEventArgs e) => ShowSidePanel(sender, 2);
        private void Quests(object sender, RoutedEventArgs e) => ShowSidePanel(sender, 3);
        private void FAQ(object sender, RoutedEventArgs e) => ShowSidePanel(sender, 4);
        #endregion

        private void ShowSidePanel(object sender, int index)
        {
            if (!(sender is ToggleButton clicked)) return;
            if (_activeTopButton == clicked && clicked.IsChecked == false)
            {
                HideSidePanel();
                _activeTopButton = null;
                return;
            }
            if (_activeTopButton != null && _activeTopButton != clicked)
            {
                _activeTopButton.IsChecked = false;
            }
            _activeTopButton = clicked;
            if (clicked.IsChecked == true)
            {
                Topic.Visibility = Visibility.Hidden;
                SubList.Visibility = Visibility.Hidden;
                SidePanelButtonsPanel.Children.Clear();
                if (!_sidePanelItems.TryGetValue(index, out var items)) return;
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var btn = new RadioButton
                    {
                        Content = item.buttonName,
                        Style = (Style)Resources["SideNavRadioButtonStyle"],
                        GroupName = "SideNav",
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };
                    btn.Click += (s, e) =>
                    {
                        foreach (RadioButton child in SidePanelButtonsPanel.Children.OfType<RadioButton>())
                            child.IsChecked = false;
                        btn.IsChecked = true;
                        if (item.subButtons != null)
                        {
                            SubGrid.Children.Clear();
                            foreach (SubButton sb in item.subButtons)
                            {
                                SubGrid.Children.Add(sb.BuildSubButtonUI());
                            }
                            SubList.Visibility = Visibility.Visible;
                            Topic.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            SubList.Visibility = Visibility.Hidden;
                            var bitmap = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/" + item.imagePath, UriKind.Absolute));
                            DescriptionImage.Source = bitmap;
                            DescriptionImage.Width = bitmap.PixelWidth;
                            DescriptionImage.Height = bitmap.PixelHeight;
                            DescriptionImage.Stretch = Stretch.Uniform;
                            DescriptionHeader.Text = item.header;
                            var textBlock = Description.GetText(item.text, item.center, DescriptionScrollViewer.ActualWidth);
                            DescriptionScrollViewer.Content = textBlock;
                            Topic.Visibility = Visibility.Visible;
                        }
                    };
                    SidePanelButtonsPanel.Children.Add(btn);
                    if (i < items.Count - 1)
                    {
                        var separator = new Border
                        {
                            Height = 1,
                            Background = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
                            Margin = new Thickness(0, 4, 0, 4)
                        };
                        SidePanelButtonsPanel.Children.Add(separator);
                    }
                }
                ((Storyboard)Resources["ShowSidePanel"]).Begin();
            }
        }

        private void HideSidePanel()
        {
            ((Storyboard)Resources["HideSidePanel"]).Begin();
            Topic.Visibility = Visibility.Hidden;
            SubList.Visibility = Visibility.Hidden;
        }

        public void Switch(SubButton relatedButton)
        {
            SubList.Visibility = Visibility.Hidden;
            var bitmap = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/" + relatedButton.imagePath, UriKind.Absolute));
            DescriptionImage.Source = bitmap;
            DescriptionImage.Width = bitmap.PixelWidth;
            DescriptionImage.Height = bitmap.PixelHeight;
            DescriptionImage.Stretch = Stretch.Uniform;
            DescriptionHeader.Text = relatedButton.header;
            var textBlock = Description.GetText(relatedButton.text, false, DescriptionScrollViewer.ActualWidth);
            DescriptionScrollViewer.Content = textBlock;
            Topic.Visibility = Visibility.Visible;
        }
    }
}