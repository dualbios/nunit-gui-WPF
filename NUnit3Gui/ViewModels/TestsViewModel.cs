using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using NUnit3Gui.Convertres;
using NUnit3Gui.Enums;
using NUnit3Gui.Extensions;
using NUnit3Gui.Instanses;
using NUnit3Gui.Interfaces;
using NUnit3Gui.Views;
using ReactiveUI;

namespace NUnit3Gui.ViewModels
{
    [Export(typeof(ITestsViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [TypeConverter(typeof(ViewModelToViewConverter<TestsViewModel, TestsView>))]
    public class TestsViewModel : ReactiveObject, ITestsViewModel
    {
        private readonly ObservableAsPropertyHelper<bool> isAllTestRunning;
        private readonly ITestTreeManager testTreeManager;
        private int _ranTestsCount;
        private TimeSpan _runningTime;
        private TestTreeCollectorType _selectedCollectorType;
        private ITest _selectedTest;
        private IEnumerable<TestTreeItem> _testTree;
        private ITestTreeCollector currentCollector;
        private IDisposable currentTreeSubscription;

        [ImportingConstructor]
        public TestsViewModel(IRunTestManager runTestManager,
            IProjectViewModel projectViewModel,
            ITestTreeManager testTreeManager)
        {
            this.RunTestManager = runTestManager;
            ProjectViewModel = projectViewModel;
            this.testTreeManager = testTreeManager;

            IObservable<ReactiveObject> addItems = ProjectViewModel.Tests.Changed
                .Where(_ => _.Action == NotifyCollectionChangedAction.Add)
                .SelectMany(_ => _.NewItems.OfType<ReactiveObject>());

            addItems.Subscribe(x => x.PropertyChanged += TestOnPropertyChanged);

            TreeTypes = this.testTreeManager.GetCollectorTypes().ToList();

            this.WhenAnyValue(vm => vm.SelectedCollectorType)
                .Subscribe(_ =>
                {
                    var currentTree = (currentCollector?.GetAllTests()).EmptyIfNull()
                            .Where(x => x != null)
                            .ToList();

                    if (currentTreeSubscription != null)
                        currentTreeSubscription.Dispose();
                    if (currentCollector != null)
                        currentCollector.Dispose();

                    try
                    {
                        currentCollector = this.testTreeManager.GetCollector(_);
                        currentCollector.CreateTree(currentTree);
                        TestTree = currentCollector.TestTree;
                        currentTreeSubscription = addItems.Subscribe(x => currentCollector.AddItem(x as ITest)/* AddNamespace(x as Test, TestTree)*/);
                    }
                    catch (Exception)
                    {
                        currentCollector = null;
                        TestTree = null;
                        currentTreeSubscription = null;
                    }
                });

            SelectedCollectorType = TreeTypes.First();

            ProjectViewModel.Tests.Changed
                .Where(_ => _.Action == NotifyCollectionChangedAction.Remove)
                .SelectMany(_ => (_.OldItems != null ? _.OldItems.OfType<ReactiveObject>() : Enumerable.Empty<ReactiveObject>()))
                .Where(_ => _ != null)
                .Subscribe(x => x.PropertyChanged -= TestOnPropertyChanged);

            ProjectViewModel.Tests.Changed.Subscribe(x => this.PropertiesChanged(nameof(TestCount), nameof(SelectedTests)));

            RunAllTestCommand = ReactiveCommand
                .CreateFromObservable(() => Observable.StartAsync(ct => RunAllTestCommandExecute(ct, ProjectViewModel.Tests))
                        .TakeUntil(this.CancelRunTestCommand)
                    , Observable.CombineLatest(
                        ProjectViewModel.BrowseAssembliesCommand.IsExecuting.Invert()
                        , this.WhenAny(vm => vm.IsAllTestRunning, p => p.Value).Invert()
                        , ProjectViewModel.HasTests
                        , ResultSelector.And3Result));

            RunSelectedTestCommand = ReactiveCommand
                .CreateFromObservable(() => Observable.StartAsync(ct => RunAllTestCommandExecute(ct, SelectedTests))
                        .TakeUntil(this.CancelRunTestCommand)
                    , Observable.CombineLatest(
                        this.WhenAny(vm => vm.IsAllTestRunning, p => p.Value).Invert()
                        , ProjectViewModel.BrowseAssembliesCommand.IsExecuting.Invert()
                        , ProjectViewModel.HasTests
                        , this.WhenAny(vm => vm.SelectedTests, p => p.Value != null && p.Value.Any())
                        , ResultSelector.And4Result)
                );

            ProjectViewModel.IsTestRunningObservable =
                IsTestRunningObservable = Observable.CombineLatest(
                     RunAllTestCommand.IsExecuting
                     , RunSelectedTestCommand.IsExecuting
                     , (a, b) => a || b);
            isAllTestRunning = IsTestRunningObservable.ToProperty(this, x => x.IsAllTestRunning);
            CancelRunTestCommand = ReactiveCommand.Create(() => { }, IsTestRunningObservable);
        }

        public ReactiveCommand<Unit, Unit> CancelRunTestCommand { get; }

        public bool IsAllTestRunning => isAllTestRunning?.Value ?? false;

        public IObservable<bool> IsTestRunningObservable { get; }

        public IProjectViewModel ProjectViewModel { get; }

        public int RanTestsCount
        {
            get => _ranTestsCount;
            private set => this.RaiseAndSetIfChanged(ref _ranTestsCount, value);
        }

        public ReactiveCommand<Unit, Unit> RunAllTestCommand { get; }

        public TimeSpan RunningTime
        {
            get => _runningTime;
            set => this.RaiseAndSetIfChanged(ref _runningTime, value);
        }

        public ReactiveCommand<Unit, Unit> RunSelectedTestCommand { get; }

        public IRunTestManager RunTestManager { get; }

        public TestTreeCollectorType SelectedCollectorType
        {
            get => _selectedCollectorType;
            set => this.RaiseAndSetIfChanged(ref _selectedCollectorType, value);
        }

        public ITest SelectedTest
        {
            get => _selectedTest;
            set => this.RaiseAndSetIfChanged(ref _selectedTest, value);
        }

        public IEnumerable<ITest> SelectedTests => ProjectViewModel.Tests.Where(_ => _.IsSelected).EmptyIfNull();

        public int TestCount => ProjectViewModel.Tests.Count();

        public int TestFailedCount => ProjectViewModel.Tests
            .Where(_ => _.Status == TestState.Failed)
            .Count();

        public int TestPassedCount => ProjectViewModel.Tests
            .Where(_ => _.Status == TestState.Passed)
            .Count();

        public IEnumerable<ITest> Tests => ProjectViewModel.Tests;

        public IEnumerable<TestTreeItem> TestTree
        {
            get => _testTree;
            private set => this.RaiseAndSetIfChanged(ref _testTree, value);
        }

        public IEnumerable<TestTreeCollectorType> TreeTypes { get; }

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
                this.PropertiesChanged(nameof(SelectedTests));
        }
    }
}