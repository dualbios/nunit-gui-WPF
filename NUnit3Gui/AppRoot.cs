using System.Threading.Tasks;
using System.Windows;
using NUnit3Gui.Instanses;
using NUnit3Gui.Interfaces;
using NUnit3Gui.ViewModels;
using NUnit3Gui.Views;

namespace NUnit3Gui
{
    public sealed class AppRoot
    {
        public AppRoot()
        {
            CompositionManager = new VSMefCompositionManager();
        }

        public static AppRoot Current { get; } = new AppRoot();

        public ICompositionManager CompositionManager { get; private set; }

        public Window MainWindow { get; private set; }

        public async Task OnStartup(StartupEventArgs e)
        {
            await CompositionManager.Compose(this);
            IMainViewModel mainViewModel = Current.CompositionManager.ExportProvider.GetExportedValue<IMainViewModel>();
            MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }
    }
}