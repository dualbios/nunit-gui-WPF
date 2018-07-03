using System.Threading.Tasks;
using System.Windows;
using NUnit3Gui.Instanses;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui
{
    public sealed class AppRoot
    {
        public AppRoot()
        {
            CompositionManager = new VSMefCompositionManager();
        }

        public static AppRoot Current { get; } = new AppRoot();
        public Window MainWindow { get; private set; }

        public ICompositionManager CompositionManager { get; private set; }

        public async Task OnStartup(StartupEventArgs e)
        {
            await CompositionManager.Compose(this);
            MainWindow = new MainWindow {DataContext = new MainWindowViewModel()};
        }
    }
}