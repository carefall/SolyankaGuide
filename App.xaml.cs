using System.Windows;

namespace SolyankaGuide
{
    public partial class App : Application {
        public App() : base()
        {
            Application.Current.DispatcherUnhandledException += (s, e) =>
            {
                MessageBox.Show(e.Exception.ToString(), "Unhandled UI Exception");
                e.Handled = true; // или false, чтобы ошибка продолжала всплывать
            };
        }
    }
}
