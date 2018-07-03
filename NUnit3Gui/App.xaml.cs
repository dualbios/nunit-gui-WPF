using System.Windows;

namespace NUnit3Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppRoot.Current.OnStartup(e);
            this.MainWindow = AppRoot.Current.MainWindow;
            this.MainWindow.Show();
        }
    }
}