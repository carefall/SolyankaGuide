using SolyankaGuide.Internals;
using System.Windows;

namespace SolyankaGuide
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BGImage.Source = ImageLoader.LoadImage("background.jpg");
        }
    }
}