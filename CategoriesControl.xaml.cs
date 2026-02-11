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
        internal static Category[] categories = Array.Empty<Category>();
        internal static event Action<Category, Element, Description?>? SwitchSidePanelWithHyper;

        internal static Category? GetCategory(string internalName)
        {
            foreach (var category in categories)
            {
                if (category.Internal_name == internalName) return category;
            }
            return null;
        }

        public CatergoriesControl()
        {
            InitializeComponent();
            MainWindow.SetupUI += SetupCategories;
            GuideControl.OpenElementWithHyper += OpenCategoryWithHyper;
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

        private void OpenCategoryWithHyper(Category category, Element element, Description? description)
        {
            if (currentButton != category.RelatedButton)
            {
                currentButton!.IsChecked = false;
                currentButton = category.RelatedButton;
                currentButton!.IsChecked = true;
            }
            SwitchSidePanelWithHyper?.Invoke(category, element, description);
        }

        private void SetupCategories()
        {
            var cats = JsonLoader.FillCategories();
            if (cats == null)
            {
                Application.Current.Shutdown();
                return;
            }
            categories = cats;
            SetupPanel(categories);
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
