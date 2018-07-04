using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit3Gui.Interfaces;
using ReactiveUI;

namespace NUnit3Gui
{
    public class MainWindowViewModel : ReactiveObject
    {
        private int _loadingProgress;

        public MainWindowViewModel()
        {
            BrowseAssembliesCommand = ReactiveCommand.CreateFromTask(() => OpenAssemblies());
            LoadedAssemblies = new ReactiveList<IFileItem>();
            FileLoaderManager = AppRoot.Current.CompositionManager.ExportProvider.GetExportedValue<IFileLoaderManager>();
        }

        public ReactiveCommand<Unit, Unit> BrowseAssembliesCommand { get; }

        public IFileLoaderManager FileLoaderManager { get; private set; }

        public ReactiveList<IFileItem> LoadedAssemblies { get; }

        public int LoadingProgress
        {
            get => _loadingProgress;
            private set
            {
                _loadingProgress = value; 
               this.RaisePropertyChanged();
            }
        }

        private async Task<Unit> OpenAssemblies()
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Dll files|*.dll", Multiselect = true };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadingProgress = 0;
                if (ofd.FileNames.Length > 0)
                {
                    int index = 0;
                    foreach (string fileName in ofd.FileNames)
                    {
                        foreach (IFileItem fileItem in await FileLoaderManager.LoadFile(fileName))
                        {
                            LoadedAssemblies.Add(fileItem);
                        }

                        LoadingProgress = (int)(((double)index) / ((double)ofd.FileNames.Length) * 100D);
                        await Task.Delay(25);
                        index++;
                    }
                }
            }

            return default(Unit);
        }
    }
}