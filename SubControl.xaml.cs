using SolyankaGuide.Internals;
using System.Windows;
using System.Windows.Controls;

namespace SolyankaGuide
{
    public partial class SubControl : UserControl
    {
        public SubControl()
        {
            InitializeComponent();
        }

        public SubButton relatedButton { get; set; }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as FrameworkElement;
            if (source == null) return;
            if (source != this && source != SubImage) return;
            if (MainWindow.GetInstance()  == null) MessageBox.Show("Критический баг. Обратитесь к разработчику гида.");
            MainWindow.GetInstance().Switch(relatedButton);
        }
    }
}
