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
            Descriptions.SizeChanged += (s, e) => RecalculateCellsSize();
        }

        private void RecalculateCellsSize()
        {
            double totalWidth = Descriptions.ActualWidth;
            double itemWitdh = (totalWidth - 20) / 3;
            var descs = Descriptions.Children.OfType<GriddedDescription>().ToArray();
            for (int i = 0; i < descs.Length; i++)
            {
                GriddedDescription gd = descs[i];
                gd.Width = itemWitdh;
                gd.Height = itemWitdh / 16 * 14;
                gd.Margin = new Thickness(0, 0, ((i + 1) % 3 != 0) ? 10 : 0, 0);
            }
        }

        private void ShowGrid(Element element)
        {
            Descriptions.Children.Clear();
            foreach (Description desc in element.Descriptions!)
            {
                Descriptions.Children.Add(BuildSubButtonUI(desc));
            }
        }

        private GriddedDescription BuildSubButtonUI(Description desc)
        {
            GriddedDescription gd = new();
            BitmapImage? bmi = ImageLoader.LoadImage(desc.GridImagePath);
            if (bmi != null) gd.Image.Source = ImageLoader.LoadImage(desc.GridImagePath);
            gd.TileName.Text = desc.Name;
            gd.MouseDown += (s, e) => OpenDescription(desc);
            return gd;
        }

        private void OpenDescription(Description desc)
        {
            Visibility = Visibility.Hidden;
            ShowDescription?.Invoke(desc);
        }
    }
}
