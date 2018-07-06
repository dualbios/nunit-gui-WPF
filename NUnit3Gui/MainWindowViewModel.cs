using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework.Interfaces;
using NUnit3Gui.Interfaces;
using ReactiveUI;
using ITest = NUnit3Gui.Interfaces.ITest;

namespace NUnit3Gui
{
    public class MainWindowViewModel : ReactiveObject
    {
        private int _loadingProgress;
        private int _ranTestsCount;
        private IFileItem _selectedAssembly;
        private ITest _selectedTest;
        private IObservable<bool> hasTests;
        private ObservableAsPropertyHelper<bool> isAllTestRunning;
        private IObservable<bool> selectedAssembly;
        private IObservable<bool> sss;

        public MainWindowViewModel()
        {
            hasTests = this.WhenAny(vm => vm.Tests, p => p.Value != null && p.Value.Any());
            selectedAssembly = this.WhenAny(vm => vm.SelectedAssembly, p => p.Value != null);

            LoadedAssemblies = new ReactiveList<IFileItem>() { ChangeTrackingEnabled = true };

            BrowseAssembliesCommand = ReactiveCommand
                .CreateFromObservable(() => Observable
                    .StartAsync(ct => OpenAssemblies(ct))
                    .TakeUntil(CancelBrowseCommand)
                , this.WhenAny(vm => vm.IsAllTestRunning, p => p.Value == false));

            LoadedAssemblies.ItemChanged
                .Subscribe(x =>
                {
                    this.RaisePropertyChanged(nameof(Tests));
                    this.RaisePropertyChanged(nameof(AssembliesCount));
                    this.RaisePropertyChanged(nameof(TestCount));
                });

            CancelBrowseCommand = ReactiveCommand.Create(() => { }, BrowseAssembliesCommand.IsExecuting);

            RunAllTestCommand = ReactiveCommand
                    .CreateFromObservable(() => Observable.StartAsync(ct => RunAllTestCommandExecute(ct))
                    .TakeUntil(this.CancelRunTestCommand)
                , Observable.CombineLatest(
                        BrowseAssembliesCommand.IsExecuting
                        , this.WhenAny(vm => vm.IsAllTestRunning, p => p.Value)
                        , hasTests
                        , (a, b, c) => !a && !b && c));

            RemoveAssembliesCommand = ReactiveCommand.CreateFromTask(() => RemoveSelecteddAssemblies()
                , Observable.CombineLatest(
                    BrowseAssembliesCommand.IsExecuting.Select(_ => !_)
                    , selectedAssembly
                    , RunAllTestCommand.IsExecuting
                    , (a, b, c) => a && b && !c));

            FileLoaderManager = AppRoot.Current.CompositionManager.ExportProvider.GetExportedValue<IFileLoaderManager>();
            RunTestManager = AppRoot.Current.CompositionManager.ExportProvider.GetExportedValue<IRunTestManager>();
            CancelRunTestCommand = ReactiveCommand.Create(() => { }, RunAllTestCommand.IsExecuting);

            isAllTestRunning = RunAllTestCommand.IsExecuting.ToProperty(this, x => x.IsAllTestRunning);
        }

        public int AssembliesCount => LoadedAssemblies.Count();

        public ReactiveCommand<Unit, Unit> BrowseAssembliesCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelBrowseCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelRunTestCommand { get; }

        public IFileLoaderManager FileLoaderManager { get; private set; }

        public bool IsAllTestRunning => isAllTestRunning?.Value ?? false;

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

        public int RanTestsCount
        {
            get => _ranTestsCount;
            private set => this.RaiseAndSetIfChanged(ref _ranTestsCount, value);
        }

        public ReactiveCommand<Unit, Unit> RemoveAssembliesCommand { get; }

        public ReactiveCommand<Unit, Unit> RunAllTestCommand { get; }

        public IRunTestManager RunTestManager { get; }

        public IFileItem SelectedAssembly
        {
            get => _selectedAssembly;
            set => this.RaiseAndSetIfChanged(ref _selectedAssembly, value);
        }

        public ITest SelectedTest
        {
            get => _selectedTest;
            set => this.RaiseAndSetIfChanged(ref _selectedTest, value);
        }

        public int TestCount => LoadedAssemblies.Sum(_ => _.Tests?.Count() ?? 0);

        public int TestFailedCount => LoadedAssemblies.SelectMany(_ => _.Tests ?? Enumerable.Empty<ITest>())
            .Where(_ => _.Status == TestStatus.Failed)
            .Count();

        public int TestPassedCount => LoadedAssemblies.SelectMany(_ => _.Tests ?? Enumerable.Empty<ITest>())
            .Where(_ => _.Status == TestStatus.Passed)
            .Count();

        public IEnumerable<ITest> Tests
        {
            get => LoadedAssemblies.Where(_ => _.Tests != null).SelectMany(a => a.Tests);
        }

        private async Task<Unit> OpenAssemblies(CancellationToken ct)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Dll files|*.dll", Multiselect = true, FileName = @"C:\Repositories\nunit-gui-WPF\NUnit3Gui\bin\Debug\NUnit3Gui.UnitTest.dll" };
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
                        this.RaisePropertyChanged(nameof(TestCount));

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
                this.RaisePropertyChanged(nameof(TestCount));
            }

            return Task.FromResult(default(Unit));
        }

        private async Task<Unit> RunAllTestCommandExecute(CancellationToken ct)
        {
            this.RaisePropertyChanged(nameof(BrowseAssembliesCommand));
            RanTestsCount = 0;
            int index = 1;
            int testCount = Tests.Count();
            foreach (ITest test in Tests)
            {
                await RunTestManager.RunTestAsync(test, ct);
                RanTestsCount = (int)(((double)index) / ((double)testCount) * 100D);

                this.RaisePropertyChanged(nameof(TestFailedCount));
                this.RaisePropertyChanged(nameof(TestPassedCount));

                if (ct.IsCancellationRequested)
                    break;
                index++;
            }
            RanTestsCount = 100;
            return Unit.Default;
        }
    }
}