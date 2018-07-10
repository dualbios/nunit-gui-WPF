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
        private int _ranTestsCount;
        private TimeSpan _runningTime;
        private ITest _selectedTest;

        [ImportingConstructor]
        public TestsViewModel(IRunTestManager runTestManager)
        {
            this.RunTestManager = runTestManager;

            Tests = new ReactiveList<ITest>() { ChangeTrackingEnabled = true};

            HasTests = Tests.WhenAny(x => x.Count, p => p.Value > 0);

            Tests.Changed
                .Where(_ => _.Action == NotifyCollectionChangedAction.Add)
                .SelectMany(_ => _.NewItems.OfType<ReactiveObject>())
                .Subscribe(x => x.PropertyChanged += TestOnPropertyChanged);

            Tests.Changed
                .Where(_ => _.Action == NotifyCollectionChangedAction.Remove)
                .SelectMany(_ => (_.OldItems != null ? _.OldItems.OfType<ReactiveObject>() : Enumerable.Empty<ReactiveObject>()))
                .Where(_ => _ != null)
                .Subscribe(x => x.PropertyChanged -= TestOnPropertyChanged);

            Tests.Changed.Subscribe(x => this.PropertiesChanged(nameof(TestCount), nameof(SelectedTests)));

            RunAllTestCommand = ReactiveCommand
                .CreateFromObservable(() => Observable.StartAsync(ct => RunAllTestCommandExecute(ct, Tests))
                        .TakeUntil(this.CancelRunTestCommand)
                    , Observable.CombineLatest(
                        //_mainViewModel.ProjectViewModel.BrowseAssembliesCommand.IsExecuting.Select(_ => !_)
                         this.WhenAny(vm => vm.IsAllTestRunning, p => p.Value).Select(_ => !_)
                        , HasTests
                        , ResultSelector.And2Result));

            RunSelectedTestCommand = ReactiveCommand
                .CreateFromObservable(() => Observable.StartAsync(ct => RunAllTestCommandExecute(ct, SelectedTests))
                        .TakeUntil(this.CancelRunTestCommand)
                    , Observable.CombineLatest(
                        this.WhenAny(vm => vm.IsAllTestRunning, p => p.Value).Invert()
                        //, _mainViewModel.ProjectViewModel.BrowseAssembliesCommand.IsExecuting.Invert()
                        , HasTests
                        , this.WhenAny(vm => vm.SelectedTests, p => p.Value != null && p.Value.Any())
                        , ResultSelector.And3Result)
                );

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

        public ITest SelectedTest
        {
            get => _selectedTest;
            set => this.RaiseAndSetIfChanged(ref _selectedTest, value);
        }

        public IEnumerable<ITest> SelectedTests => Tests.Where(_ => _.IsSelected).EmptyIfNull();

        public int TestCount => Tests?.Count() ?? 0;

        public int TestFailedCount => Tests
            .Where(_ => _.Status == TestState.Failed)
            .Count();

        public int TestPassedCount => Tests
            .Where(_ => _.Status == TestState.Passed)
            .Count();

        public IReactiveList<ITest> Tests { get; }

        public IObservable<bool> HasTests { get; }

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