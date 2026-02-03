using SolyankaGuide.Internals;
using System.Windows;

namespace SolyankaGuide
{
    public partial class MainWindow : Window
    {

        public static Action? RefreshUI;
        private bool locked = false;

        public MainWindow()
        {
            InitializeComponent();
            BGImage.Source = ImageLoader.LoadImage("background.jpg");
            OverrideImage.Source = BGImage.Source;
            Title = Locale.Get("title");
            UpdateYes.Text = Locale.Get("yes");
            UpdateNo.Text = Locale.Get("no");
            UpdateText.Text = Locale.Get("update_question");
            UpdateGrid.Visibility = Visibility.Visible;
        }

        private void NoUpdate(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (locked) return;
            UpdateGrid.Visibility = Visibility.Hidden;
            OverrideImage.Visibility = Visibility.Hidden;
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
                Logger.Log("Startup", "Update installed");
                RefreshUI?.Invoke();
            }
            else if (updateStatus == 0)
            {
                MessageBox.Show(Locale.Get("updates_not_found"), Locale.Get("update"), MessageBoxButton.OK);
            }
            UpdateGrid.Visibility = Visibility.Hidden;
            OverrideImage.Visibility = Visibility.Hidden;
        }
    }
}