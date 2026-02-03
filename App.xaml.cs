using SolyankaGuide.Internals;
using System.Windows;

namespace SolyankaGuide
{
    public partial class App : Application {

        public App() : base() 
        {
            Application.Current.DispatcherUnhandledException += (s, e) =>
            {
                MessageBox.Show(e.Exception.ToString(), "Unhandled UI Exception");
                e.Handled = true;
            };
            ImageLoader.SetupPlaceholder();
            Logger.Setup();
            Logger.Log("Startup", "App started!");
            Locale.Init();
        }
    }
}
