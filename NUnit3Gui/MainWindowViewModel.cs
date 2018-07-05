using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework.Interfaces;
using NUnit3Gui.Instanses;
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

        private ObservableAsPropertyHelper<bool> isTestRunning;

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
                .Subscribe(x =>
                {
                    this.RaisePropertyChanged(nameof(Tests));
                    this.RaisePropertyChanged(nameof(AssembliesCount));
                    this.RaisePropertyChanged(nameof(TestCount));
                });

            CancelBrowseCommand = ReactiveCommand.Create(() => { }, BrowseAssembliesCommand.IsExecuting);
            IObservable<bool> hasTests = this.WhenAny(vm => vm.Tests, p => p.Value != null && p.Value.Any());

            RunSelectedTestCommand = ReactiveCommand.CreateFromTask(
                () => RunSelectedTestCommandExecute(),
                Observable.Merge(
                    this.WhenAny(vm => vm.SelectedTest, p => p.Value != null)
                    , hasTests)
                );

            RunAllTestCommand = ReactiveCommand
                .CreateFromObservable(() => Observable.StartAsync(ct => RunAllTestCommandExecute(ct))
                    //, Observable.Merge(
                    //    this.WhenAny(vm => vm.SelectedTest, p => p.Value != null)
                    //    , RunSelectedTestCommand.IsExecuting.Select(_ => !_)
                    //  , hasTests
                    //)
                        .TakeUntil(this.CancelRunTestCommand));

            isTestRunning = RunSelectedTestCommand.IsExecuting.ToProperty(this, x => x.IsTestRunning);

            FileLoaderManager = AppRoot.Current.CompositionManager.ExportProvider.GetExportedValue<IFileLoaderManager>();
            CancelRunTestCommand = ReactiveCommand.Create(() => { }, RunAllTestCommand.IsExecuting);
        }

        public int AssembliesCount => LoadedAssemblies.Count();

        public ReactiveCommand<Unit, Unit> BrowseAssembliesCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelBrowseCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelRunTestCommand { get; }

        public IFileLoaderManager FileLoaderManager { get; private set; }

        public bool IsTestRunning => isTestRunning.Value;

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

        public ReactiveCommand<Unit, Unit> RunSelectedTestCommand { get; }

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
            RanTestsCount = 0;
            int index = 1;
            int testCount = Tests.Count();
            foreach (ITest test in Tests)
            {
                await RunTest(test, ct);
                RanTestsCount = (int)(((double)index) / ((double)testCount) * 100D);
                if (ct.IsCancellationRequested)
                    break;
                index++;
            }
            RanTestsCount = 100;
            return Unit.Default;
        }

        private Task<Unit> RunSelectedTestCommandExecute()
        {
            return RunTest(SelectedTest, CancellationToken.None);
        }

        private async Task<Unit> RunTest(ITest test, CancellationToken ct)
        {
            try
            {
                test.IsRunning = true;
                test.StringStatus = null;
                await Task.Delay(25);

                var rrr = new RunProcess(test.AssemblyPath, test.TestName);
                var result = await rrr.Run(ct);
                await Task.Delay(25);

                test.StringStatus = rrr.StandardOutput.ToString();
                test.Status = result ? TestStatus.Passed : TestStatus.Failed;
            }
            catch (Exception e)
            {
                test.Status = TestStatus.Failed;
                test.StringStatus = e.Message;
            }
            finally
            {
                test.IsRunning = false;
            }

            this.RaisePropertyChanged(nameof(TestFailedCount));
            this.RaisePropertyChanged(nameof(TestPassedCount));

            await Task.Delay(25);
            return Unit.Default;
        }
    }
}