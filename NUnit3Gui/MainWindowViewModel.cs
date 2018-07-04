using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit3Gui.Interfaces;
using ReactiveUI;

namespace NUnit3Gui
{
    public class MainWindowViewModel : ReactiveObject
    {
        public MainWindowViewModel()
        {
            BrowseAssembliesCommand = ReactiveCommand.CreateFromTask(() => OpenAssemblies());
            LoadedAssemblies = new ReactiveList<IFileItem>();
            FileLoaderManager = AppRoot.Current.CompositionManager.ExportProvider.GetExportedValue<IFileLoaderManager>();
        }

        public ReactiveCommand<Unit, Unit> BrowseAssembliesCommand { get; }

        public IFileLoaderManager FileLoaderManager { get; private set; }

        public ReactiveList<IFileItem> LoadedAssemblies { get; }

        private async Task<Unit> OpenAssemblies()
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Dll files|*.dll", Multiselect = true };
            ofd.ShowDialog();

            if (ofd.FileNames.Length > 0)
            {
                foreach (string fileName in ofd.FileNames)
                {
                    foreach (IFileItem fileItem in await FileLoaderManager.LoadFile(fileName))
                    {
                        LoadedAssemblies.Add(fileItem);
                    }
                }
            }

            return default(Unit);
        }
    }
}