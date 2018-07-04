using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit3Gui.Interfaces;
using ReactiveUI;

namespace NUnit3Gui
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool> canDoIt;
        private int _loadingProgress;
        private IFileItem _selectedAssembly;

        public MainWindowViewModel()
        {
            BrowseAssembliesCommand = ReactiveCommand.CreateFromTask(() => OpenAssemblies());
            canDoIt = this.WhenAnyObservable(x => x.BrowseAssembliesCommand.IsExecuting)
                .ToProperty(this, x => x.CanDoIt);
            RemoveAssembliesCommand = ReactiveCommand.CreateFromTask(
                () => RemoveSelecteddAssemblies(),
                Observable.Merge(
                    this.WhenAny(vm => vm.CanDoIt, p => p.Value == false),
                    this.WhenAny(vm => vm.SelectedAssembly, p => p.Value != null)));
            LoadedAssemblies = new ReactiveList<IFileItem>();
            FileLoaderManager = AppRoot.Current.CompositionManager.ExportProvider.GetExportedValue<IFileLoaderManager>();
        }

        public ReactiveCommand<Unit, Unit> BrowseAssembliesCommand { get; }

        public bool CanDoIt
        {
            get { return canDoIt.Value; }
        }

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

        public ReactiveCommand<Unit, Unit> RemoveAssembliesCommand { get; }

        public IFileItem SelectedAssembly
        {
            get => _selectedAssembly;
            set => this.RaiseAndSetIfChanged(ref _selectedAssembly, value);
        }

        private async Task<Unit> OpenAssemblies()
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Dll files|*.dll", Multiselect = true };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadingProgress = 0;
                if (ofd.FileNames.Length > 0)
                {
                    foreach (IFileItem fileItem in FileLoaderManager.LoadFiles(ofd.FileNames))
                    {
                        LoadedAssemblies.Add(fileItem);
                    }

                    int index = 0;
                    foreach (IFileItem item in LoadedAssemblies)
                    {
                        await item.LoadAsync();
                        LoadingProgress = (int)(((double)index) / ((double)ofd.FileNames.Length) * 100D);
                        await Task.Delay(25);
                        index++;
                    }
                    LoadingProgress = 100;
                }
            }

            return default(Unit);
        }

        private Task<Unit> RemoveSelecteddAssemblies()
        {
            if (SelectedAssembly != null)
                LoadedAssemblies.Remove(SelectedAssembly);

            return Task.FromResult(default(Unit));
        }
    }
}