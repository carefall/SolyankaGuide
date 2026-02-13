using SolyankaGuide.Internals;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SolyankaGuide
{
    public partial class DescriptionGridControl : UserControl
    {

        internal static event Action<Description>? ShowDescription;

        public DescriptionGridControl()
        {
            InitializeComponent();
            GuideControl.ShowGrid += ShowGrid;
            DescriptionControl.ShowGrid += () => Visibility = Visibility.Visible;
        }

        private void ShowGrid(Element element)
        {
            Descriptions.Children.Clear();
            int len = element.Descriptions!.Length;
            int lastRowCount = len % 3 == 0 ? 3 : len % 3;
            int firstIndexOfLastRow = len - lastRowCount;
            for (int i = 0; i < len; i++)
            {
                var desc = element.Descriptions[i];
                bool isRight = (i % 3) == 2;
                bool isBottom = i >= firstIndexOfLastRow;
                Descriptions.Children.Add(BuildSubButtonUI(desc, isRight, isBottom));
            }
        }

        private GriddedDescription BuildSubButtonUI(Description desc, bool rightest, bool lowest)
        {
            GriddedDescription gd = new()
            {
                Margin = new Thickness(0, 0, rightest ? 0 : 15, lowest ? 0 : 15),
            };
            BitmapImage bmi = ImageLoader.LoadImage(desc.GridImagePath);
            gd.Image.Source = bmi;
            gd.TileName.Text = desc.Name;
            gd.MouseDown += (s, e) =>
            {
                OpenDescription(desc);
            };
            return gd;
        }

        private void OpenDescription(Description desc)
        {
            Visibility = Visibility.Hidden;
            ShowDescription?.Invoke(desc);
        }
    }
}
