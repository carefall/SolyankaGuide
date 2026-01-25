using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SolyankaGuide
{
    public partial class TitleControl : UserControl
    {

        private readonly Window window;

        public TitleControl()
        {
            InitializeComponent();
            window = App.Current.MainWindow;
        }

        public void CloseButton(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        public void MinimizeButton(object sender, RoutedEventArgs e)
        {
            window.WindowState = WindowState.Minimized;
        }

        public void ResizeButton(object sender, RoutedEventArgs e)
        {
            window.WindowState = App.Current.MainWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Drag(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && window.WindowState != WindowState.Minimized) window.DragMove();
        }
    }
}
