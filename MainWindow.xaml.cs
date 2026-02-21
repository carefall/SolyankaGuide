using SolyankaGuide.Internals;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SolyankaGuide
{
    public partial class MainWindow : Window
    {

        public static event Action? SetupUI;
        public static event Action? ChangeFocus;
        private bool locked = false;

        public MainWindow()
        {
            InitializeComponent();
            BGImage.Source = ImageLoader.LoadImage("UI/background.jpg");
            OverrideImage.Source = BGImage.Source;
            Title = Locale.Get("title");
            UpdateYes.Text = Locale.Get("yes");
            UpdateNo.Text = Locale.Get("no");
            UpdateText.Text = Locale.Get("update_question");
            UpdateGrid.Visibility = Visibility.Visible;
            TextGen.MaximizeImage += MaximizeImage;
        }

        private void MaximizeImage(BitmapImage image)
        {
            MaxImg.Source = image;
            MaximizedImage.Visibility = Visibility.Visible;
            MaximizedImage.Focus();
        }

        private void NoUpdate(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (locked) return;
            Logger.Log("Updater", "Update check cancelled.");
            UpdateGrid.Visibility = Visibility.Hidden;
            OverrideImage.Visibility = Visibility.Hidden;
            SetupUI?.Invoke();
        }

        private async void Update(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (locked) return;
            locked = true;
            UpdateStatus.Visibility = Visibility.Visible;
            UpdateSpinner.Visibility = Visibility.Visible;
            int updateStatus = await GitHubAutoUpdate.Update(UpdateStatus);
            locked = false;
            if (updateStatus == 1)
            {
                Logger.Log("Updater", "Update installed.");
                MessageBox.Show(Locale.Get("update_installed"), Locale.Get("update"), MessageBoxButton.OK);
            }
            else if (updateStatus == 0)
            {
                MessageBox.Show(Locale.Get("updates_not_found"), Locale.Get("update"), MessageBoxButton.OK);
            }
            UpdateGrid.Visibility = Visibility.Hidden;
            OverrideImage.Visibility = Visibility.Hidden;
            SetupUI?.Invoke();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MaximizedImage.Visibility = Visibility.Hidden;
        }

        private void MaximizedImage_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            if (MaximizedImage.Visibility == Visibility.Visible)
            {
                MaximizedImage.Visibility = Visibility.Hidden;
                ChangeFocus?.Invoke();
            }
        }
    }
}