using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using NUnit3Gui.Enums;
using NUnit3Gui.Extensions;
using NUnit3Gui.Interfaces;
using ReactiveUI;
using ITest = NUnit3Gui.Interfaces.ITest;

namespace NUnit3Gui
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool> isAllTestRunning;
        private readonly IObservable<bool> selectedAssembly;
        private int _loadingProgress;
        private int _ranTestsCount;
        private TimeSpan _runningTime;
        private IFileItem _selectedAssembly;
        private ITest _selectedTest;
        private IObservable<bool> hasTests;
        private IObservable<bool> isTestRunningObservable;

        public MainWindowViewModel()
        {
            LoadedAssemblies = new ReactiveList<IFileItem>() { ChangeTrackingEnabled = true };
            Tests = new ReactiveList<ITest>() { ChangeTrackingEnabled = true };

            hasTests = Tests.WhenAny(x => x.Count, p => p.Value > 0);
            selectedAssembly = this.WhenAny(vm => vm.SelectedAssembly, p => p.Value != null);

            Tests.Changed
                .Subscribe(x =>
                {
                    if (x.Action == NotifyCollectionChangedAction.Add)
                    {
                        foreach (ReactiveObject test in x.NewItems)
                        {
                            test.PropertyChanged += TestOnPropertyChanged;
                        }
                    }
                    else if (x.Action == NotifyCollectionChangedAction.Remove)
                    {
                        foreach (ReactiveObject test in x.OldItems)
                        {
                            test.PropertyChanged -= TestOnPropertyChanged;
                        }
                    }
                    PropertiesChanged(nameof(Tests), nameof(AssembliesCount), nameof(TestCount), nameof(SelectedTests));
                });

            BrowseAssembliesCommand = ReactiveCommand
                .CreateFromObservable(() => Observable
                    .StartAsync(ct => OpenAssemblies(ct))
                    .TakeUntil(CancelBrowseCommand)
                , this.WhenAny(vm => vm.IsAllTestRunning, p => p.Value == false));

            LoadedAssemblies.ItemsRemoved
                .Subscribe(x =>
                {
                    Tests.RemoveAll(x.Tests);
                });

            LoadedAssemblies.Changed
                .Subscribe(x =>
                {
                    if (x.Action == NotifyCollectionChangedAction.Reset)
                    {
                        Tests.Clear();
                    }
                    PropertiesChanged(nameof(Tests), nameof(AssembliesCount), nameof(TestCount), nameof(SelectedTests));
                });
            LoadedAssemblies.ItemChanged
                .Subscribe(x =>
                {
                    PropertiesChanged(nameof(Tests), nameof(AssembliesCount), nameof(TestCount));
                });

            CancelBrowseCommand = ReactiveCommand.Create(() => { }, BrowseAssembliesCommand.IsExecuting);

            RunAllTestCommand = ReactiveCommand
                    .CreateFromObservable(() => Observable.StartAsync(ct => RunAllTestCommandExecute(ct, Tests))
                    .TakeUntil(this.CancelRunTestCommand)
                , Observable.CombineLatest(
                        BrowseAssembliesCommand.IsExecuting.Select(_ => !_)
                        , this.WhenAny(vm => vm.IsAllTestRunning, p => p.Value).Select(_ => !_)
                        , hasTests
                        , ResultSelector.And3Result));

            RunSelectedTestCommand = ReactiveCommand
                .CreateFromObservable(() => Observable.StartAsync(ct => RunAllTestCommandExecute(ct, SelectedTests))
                        .TakeUntil(this.CancelRunTestCommand)
                    , Observable.CombineLatest(
                       this.WhenAny(vm => vm.IsAllTestRunning, p => p.Value).Invert()
                        , BrowseAssembliesCommand.IsExecuting.Invert()
                        , hasTests
                        , this.WhenAny(vm => vm.SelectedTests, p => p.Value != null && p.Value.Any())
                        , ResultSelector.And4Result)
                    );

            isTestRunningObservable = Observable.CombineLatest(
                RunAllTestCommand.IsExecuting
                , RunSelectedTestCommand.IsExecuting
                , (a, b) => a || b);
            isAllTestRunning = isTestRunningObservable.ToProperty(this, x => x.IsAllTestRunning);
            CancelRunTestCommand = ReactiveCommand.Create(() => { }, isTestRunningObservable);

            RemoveAssembliesCommand = ReactiveCommand.CreateFromTask(() => RemoveSelecteddAssemblies()
                , Observable.CombineLatest(
                    BrowseAssembliesCommand.IsExecuting.Invert()
                    , selectedAssembly
                    , isTestRunningObservable.Invert()
                    , ResultSelector.And3Result));

            RemoveAllAssembliesCommand = ReactiveCommand.CreateFromTask(() => RemoveAllAssembliesCommandExecute()
                , Observable.CombineLatest(
                    hasTests
                    , isTestRunningObservable.Invert()
                    , BrowseAssembliesCommand.IsExecuting.Invert()
                    , ResultSelector.And3Result)
            );

            FileLoaderManager = AppRoot.Current.CompositionManager.ExportProvider.GetExportedValue<IFileLoaderManager>();
            RunTestManager = AppRoot.Current.CompositionManager.ExportProvider.GetExportedValue<IRunTestManager>();
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
                IReactiveObjectExtensions.RaisePropertyChanged(this);
            }
        }

        public int RanTestsCount
        {
            get => _ranTestsCount;
            private set => this.RaiseAndSetIfChanged(ref _ranTestsCount, value);
        }

        public ReactiveCommand<Unit, Unit> RemoveAllAssembliesCommand { get; }

        public ReactiveCommand<Unit, Unit> RemoveAssembliesCommand { get; }

        public ReactiveCommand<Unit, Unit> RunAllTestCommand { get; }

        public TimeSpan RunningTime
        {
            get => _runningTime;
            set => this.RaiseAndSetIfChanged(ref _runningTime, value);
        }

        public ReactiveCommand<Unit, Unit> RunSelectedTestCommand { get; }

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

        public IEnumerable<ITest> SelectedTests => LoadedAssemblies.Where(_ => _.Tests != null).SelectMany(a => a.Tests).Where(_ => _.IsSelected).EmptyIfNull();

        public int TestCount => LoadedAssemblies.Sum(_ => _.Tests?.Count() ?? 0);

        public int TestFailedCount => LoadedAssemblies.SelectMany(_ => _.Tests ?? Enumerable.Empty<ITest>())
            .Where(_ => _.Status == TestState.Failed)
            .Count();

        public int TestPassedCount => LoadedAssemblies.SelectMany(_ => _.Tests ?? Enumerable.Empty<ITest>())
            .Where(_ => _.Status == TestState.Passed)
            .Count();

        public ReactiveList<ITest> Tests { get; }

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

            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Dll files|*.dll", Multiselect = true, FileName = @"*.test*.dll" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadingProgress = 0;
                if (ofd.FileNames.Length > 0)
                {
                    IEnumerable<IFileItem> addedFiles = FileLoaderManager.LoadFiles(ofd.FileNames).ToList();
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
                        await item.LoadAsync();
                        foreach (ITest test in item.Tests)
                        {
                            Tests.Add(test);
                        }

                        LoadingProgress = (int)(((double)index) / ((double)ofd.FileNames.Length) * 100D);
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

        private void PropertiesChanged(params string[] properties)
        {
            foreach (string property in properties)
            {
                this.RaisePropertyChanged(property);
            }
        }

        private Task<Unit> RemoveAllAssembliesCommandExecute()
        {
            LoadedAssemblies.Clear();
            this.RaisePropertyChanged(nameof(TestCount));

            return Task.FromResult(default(Unit));
        }

        private Task<Unit> RemoveSelecteddAssemblies()
        {
            if (SelectedAssembly != null)
            {
                LoadedAssemblies.Remove(SelectedAssembly);
            }

            return Task.FromResult(default(Unit));
        }

        private async Task<Unit> RunAllTestCommandExecute(CancellationToken ct, IEnumerable<ITest> testList)
        {
            RanTestsCount = 0;
            RunningTime = TimeSpan.Zero;

            var timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(250) };
            var startTime = DateTime.Now;
            timer.Tick += (sender, args) => { RunningTime = DateTime.Now - startTime; };
            timer.Start();

            int index = 1;
            int testCount = testList.Count();
            foreach (ITest test in testList)
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
            timer.Stop();
            return Unit.Default;
        }

        private void TestOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ITest.IsSelected))
                PropertiesChanged(nameof(SelectedTests));
        }
    }
}