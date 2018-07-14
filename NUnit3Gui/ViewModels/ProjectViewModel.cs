using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit3Gui.Convertres;
using NUnit3Gui.Extensions;
using NUnit3Gui.Interfaces;
using NUnit3Gui.Views;
using ReactiveUI;

namespace NUnit3Gui.ViewModels
{
    [Export(typeof(IProjectViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [TypeConverter(typeof(ViewModelToViewConverter<ProjectViewModel, ProjectView>))]
    public class ProjectViewModel : ReactiveObject, IProjectViewModel
    {
        private readonly IFileLoaderManager _fileLoaderManager;
        private readonly IOpenFileDialog _openFileDialog;
        private readonly string[] propertiesToRefresh = { nameof(AssembliesCount) };
        private readonly IObservable<bool> selectedAssembly;
        private int _loadingProgress;
        private IFileItem _selectedAssembly;
        private IObservable<bool> _isTestRunningObservable;
        private IObservable<bool> isTestRunning;

        public IReactiveList<ITest> Tests { get; } = new ReactiveList<ITest>() { ChangeTrackingEnabled = true };

        public IObservable<bool> HasTests { get; }

        [ImportingConstructor]
        public ProjectViewModel(IFileLoaderManager fileLoaderManager, IOpenFileDialog openFileDialog)
        {
            _fileLoaderManager = fileLoaderManager;
            _openFileDialog = openFileDialog;

            LoadedAssemblies = new ReactiveList<IFileItem>() { ChangeTrackingEnabled = true };

            selectedAssembly = this.WhenAny(vm => vm.SelectedAssembly, p => p.Value != null);

            Tests.Changed.Subscribe(x => this.PropertiesChanged(propertiesToRefresh));

            HasTests = Tests.WhenAny(x => x.Count, p => p.Value > 0);

            LoadedAssemblies.ItemsRemoved
                .Subscribe(x => Tests.RemoveAll(x.Tests.EmptyIfNull()));

            LoadedAssemblies.Changed
                .Where(_ => _.Action == NotifyCollectionChangedAction.Reset)
                .Subscribe(x => Tests.Clear());

            LoadedAssemblies.Changed
                .Subscribe(x => this.PropertiesChanged(propertiesToRefresh));

            LoadedAssemblies.ItemChanged
                .Subscribe(x => this.PropertiesChanged(propertiesToRefresh));

            isTestRunning = this.WhenAnyValue(vm => vm.IsTestRunningObservable)
                .Where(_ => _ != null)
                .SelectMany(_ => _);

            BrowseAssembliesCommand = ReactiveCommand
                .CreateFromObservable(() => Observable
                        .StartAsync(ct => OpenAssemblies(ct))
                        .TakeUntil(CancelBrowseCommand)
                    , isTestRunning.Invert());

            CancelBrowseCommand = ReactiveCommand.Create(() => { }, BrowseAssembliesCommand.IsExecuting);


            RemoveAssembliesCommand = ReactiveCommand.CreateFromTask(() =>
                {
                    if (SelectedAssembly != null)
                    {
                        LoadedAssemblies.Remove(SelectedAssembly);
                    }
                    return Task.FromResult(default(Unit));
                }
                , Observable.CombineLatest(
                    BrowseAssembliesCommand.IsExecuting.Invert()
                    , selectedAssembly
                    , isTestRunning.Invert()
                    , ResultSelector.And3Result));

            RemoveAllAssembliesCommand = ReactiveCommand.CreateFromTask(() =>
                {
                    LoadedAssemblies.Clear();
                    return Task.FromResult(default(Unit));
                }
                , Observable.CombineLatest(
                    HasTests
                    , BrowseAssembliesCommand.IsExecuting.Invert()
                    , isTestRunning.Invert()
                    , ResultSelector.And3Result));
        }

        public IObservable<bool> IsTestRunningObservable
        {
            get => _isTestRunningObservable;
            set => this.RaiseAndSetIfChanged(ref _isTestRunningObservable, value);
        }

        public int AssembliesCount => LoadedAssemblies.Count();

        public ReactiveCommand<Unit, Unit> BrowseAssembliesCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelBrowseCommand { get; }

        public IReactiveList<IFileItem> LoadedAssemblies { get; }

        public int LoadingProgress
        {
            get => _loadingProgress;
            private set
            {
                _loadingProgress = value;
                IReactiveObjectExtensions.RaisePropertyChanged(this);
            }
        }

        public ReactiveCommand<Unit, Unit> RemoveAllAssembliesCommand { get; }

        public ReactiveCommand<Unit, Unit> RemoveAssembliesCommand { get; }

        public IFileItem SelectedAssembly
        {
            get => _selectedAssembly;
            set => this.RaiseAndSetIfChanged(ref _selectedAssembly, value);
        }

        public int TestCount { get; }

        private async Task<Unit> OpenAssemblies(CancellationToken ct)
        {
            void CancelationOpenAssemblies(IEnumerable<IFileItem> addedFiles)
            {
                using (LoadedAssemblies.SuppressChangeNotifications())
                {
                    foreach (IFileItem file in addedFiles)
                    {
                        LoadedAssemblies.Remove(file);
                    }
                }

                LoadingProgress = 100;
            }

            _openFileDialog.Filter = "Dll files|*.dll";
            _openFileDialog.Multiselect = true;
            _openFileDialog.FileName = @"*.test*.dll";
            if (_openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadingProgress = 0;
                if (_openFileDialog.FileNames.Length > 0)
                {
                    IEnumerable<IFileItem> addedFiles = _fileLoaderManager.LoadFiles(_openFileDialog.FileNames).ToList();
                    foreach (IFileItem fileItem in addedFiles)
                    {
                        LoadedAssemblies.Add(fileItem);
                    }

                    if (ct.IsCancellationRequested)
                    {
                        CancelationOpenAssemblies(addedFiles);
                        return default(Unit);
                    }

                    int index = 1;
                    foreach (IFileItem item in addedFiles)
                    {
                        try
                        {
                            await item.LoadAsync();
                            foreach (ITest test in item.Tests)
                            {
                                Tests.Add(test);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        LoadingProgress = (int)(((double)index) / ((double)_openFileDialog.FileNames.Length) * 100D);
                        await Task.Delay(25);
                        index++;

                        if (ct.IsCancellationRequested)
                        {
                            CancelationOpenAssemblies(addedFiles);
                            return default(Unit);
                        }
                    }
                    LoadingProgress = 100;
                }
            }

            return default(Unit);
        }
    }
}