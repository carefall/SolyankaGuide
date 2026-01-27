using SolyankaGuide.Internals;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SolyankaGuide
{
    public partial class CatergoriesControl : UserControl
    {

        private ToggleButton? currentButton = null;
        internal static event Action<Category?>? SwitchSidePanel;

        public CatergoriesControl()
        {
            InitializeComponent();
            App.RefreshUI += RefreshCategories;
            RefreshCategories();
        }

        private void RefreshCategories()
        {
            App.Current.MainWindow.Topmost = true;
            Category[]? categories = JsonLoader.FillCategories();
            if (categories == null)
            {
                Application.Current.Shutdown();
                return;
            }
            SetupPanel(categories);
        }

        private void OpenCategory(Category category)
        {
            if (currentButton == category.RelatedButton)
            {
                currentButton!.IsChecked = false;
                SwitchSidePanel?.Invoke(null);
                currentButton = null;
                return;
            }
            if (currentButton != null)
            {
                currentButton.IsChecked = false;
                currentButton = category.RelatedButton;
            }
            else
            {
                currentButton = category.RelatedButton;
            }
            SwitchSidePanel?.Invoke(category);
        }

        private void SetupPanel(Category[] categories)
        {
            Panel.Children.Clear();
            for (int i = 0; i < categories?.Length; i++)
            {
                Category category = categories[i];
                ToggleButton tb = new()
                {
                    Content = category.Name,
                    Style = (Style)FindResource("CategoriesStyle")
                };
                category.RelatedButton = tb;
                tb.Click += (s, e) => OpenCategory(category);
                Panel.Children.Add(tb);
                if (i != categories.Length - 1)
                {
                    Border b = new()
                    {
                        Width = 1,
                        Height = 20,
                        Background = (SolidColorBrush?)new BrushConverter().ConvertFromString("#66AAAAAA"),
                        Margin = new Thickness(5)
                    };
                    Panel.Children.Add(b);
                }
            }
        }
    }
}
