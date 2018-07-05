using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit3Gui.Interfaces;
using ReactiveUI;
using System;

namespace NUnit3Gui
{
    public class MainWindowViewModel : ReactiveObject
    {
        private int _loadingProgress;
        private IFileItem _selectedAssembly;

        public MainWindowViewModel()
        {
            LoadedAssemblies = new ReactiveList<IFileItem>() { ChangeTrackingEnabled = true };

            BrowseAssembliesCommand = ReactiveCommand
                .CreateFromObservable(() => Observable
                    .StartAsync(ct => OpenAssemblies(ct))
                    .TakeUntil(this.CancelBrowseCommand));

            RemoveAssembliesCommand = ReactiveCommand.CreateFromTask(
                () => RemoveSelecteddAssemblies(),
                     Observable.Merge(
                        BrowseAssembliesCommand.IsExecuting.Select(_ => !_)
                        , (this).WhenAny(vm => vm.SelectedAssembly, p => p.Value != null)
                      )
                    );

            LoadedAssemblies.ItemChanged
                .Subscribe(x => this.RaisePropertyChanged(nameof(Tests)));

            CancelBrowseCommand = ReactiveCommand.Create(() => { }, BrowseAssembliesCommand.IsExecuting);

            FileLoaderManager = AppRoot.Current.CompositionManager.ExportProvider.GetExportedValue<IFileLoaderManager>();
        }

        public ReactiveCommand<Unit, Unit> BrowseAssembliesCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelBrowseCommand { get; }

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

        public IEnumerable<string> Tests
        {
            get => LoadedAssemblies.Where(_ => _.Tests != null).SelectMany(a => a.Tests);
        }

        private async Task<Unit> OpenAssemblies(CancellationToken ct)
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

                    if (ct.IsCancellationRequested)
                    {
                        LoadedAssemblies.Clear();
                        return default(Unit);
                    }

                    int index = 0;
                    foreach (IFileItem item in LoadedAssemblies)
                    {
                        await item.LoadAsync();
                        LoadingProgress = (int)(((double)index) / ((double)ofd.FileNames.Length) * 100D);
                        await Task.Delay(25);
                        index++;

                        if (ct.IsCancellationRequested)
                        {
                            LoadedAssemblies.Clear();
                            return default(Unit);
                        }
                    }
                    LoadingProgress = 100;
                }
            }

            return default(Unit);
        }

        private Task<Unit> RemoveSelecteddAssemblies()
        {
            if (SelectedAssembly != null)
            {
                LoadedAssemblies.Remove(SelectedAssembly);
                this.RaisePropertyChanged(nameof(Tests));
            }

            return Task.FromResult(default(Unit));
        }
    }
}