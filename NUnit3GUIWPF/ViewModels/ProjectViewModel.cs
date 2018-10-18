using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Microsoft.VisualStudio.Composition;
using NUnit.Engine;
using NUnit3GUIWPF.Converters;
using NUnit3GUIWPF.Interfaces;
using NUnit3GUIWPF.Models;
using NUnit3GUIWPF.Views;
using ReactiveUI;

namespace NUnit3GUIWPF.ViewModels
{
    [Export(typeof(IProjectViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.NonShared)]
    [TypeConverter(typeof(ViewModelToViewConverter<ProjectViewModel, ProjectView>))]
    public class ProjectViewModel : ReactiveObject, IProjectViewModel, ITestEventListener
    {
        private string _fileName;
        private string _header;
        private bool _isProjectLoaded;
        private ObservableAsPropertyHelper<bool> _isProjectLoading;
        private bool _isRunning;
        private TestNode _selectedItem;
        private ProjectState _state = ProjectState.NotLoaded;
        private ITestEngine _testEngine;
        private IEnumerable<TestNode> flattenTests = new List<TestNode>();

        private IDictionary<string, Action<ProjectViewModel, TestNode, XmlNode>> reportActions = new Dictionary<string, Action<ProjectViewModel, TestNode, XmlNode>>()
        {
            {"start-test", (vm, node, report) => { node.TestAction = TestState.Starting; }},
            {"start-suite", (vm, node, report) => { node.TestAction = TestState.Starting; }},
            {"start-run", (vm, node, report) => { node.TestAction = TestState.Starting; }},
            {
                "test-case", (vm, node, report) =>
                {
                    node.TestAction = TestState.Finished;
                    node.Output = report.InnerText;
                    node.Duration = report.ParseDuration();
                }
            },
            {
                "test-suite", (vm, node, report) =>
                {
                    node.TestAction = TestState.Finished;
                    node.Output = report.InnerText;
                    node.Duration = report.ParseDuration();
                }
            },
            {
                "test-run", (vm, node, report) =>
                {
                    node.TestAction = TestState.Finished;
                    node.Duration = report.ParseDuration();
                    node.Output = report.InnerText;
                    vm.State = ProjectState.Finished;
                    Application.Current.Dispatcher.Invoke(() => { vm.IsRunning = false; });
                }
            },
        };

        [ImportingConstructor]
        public ProjectViewModel(IUnitTestEngine engine, IExportProvider provider)
        {
            _testEngine = engine.TestEngine;
            RunAllTestCommand = ReactiveCommand.CreateFromTask(
                RunAllTestAsync,
                this.WhenAny(
                    vm => vm.FileName,
                    vm => vm.IsRunning,
                    (p1, p2) => !string.IsNullOrEmpty(p1.Value) && !p2.Value));
            StopTestCommand = ReactiveCommand.CreateFromTask(
                StopTestAsync,
                this.WhenAny(vm => vm.IsRunning, p => p.Value == true));

            RunSelectedTestCommand = ReactiveCommand.CreateFromTask(
                RunSelectedTestAsync,
                this.WhenAny(vm => vm.SelectedItem, p => p.Value != null));

            OpenFileCommand = ReactiveCommand.CreateFromObservable(
                () => Observable.StartAsync(ct => LoadFile(FileName, ct))
                    .TakeUntil(CancelLoadingProjectCommand),
                this.WhenAny(vm => vm.FileName, p => !string.IsNullOrEmpty(p.Value)));

            _isProjectLoading = OpenFileCommand.IsExecuting.ToProperty(this, vm => vm.IsProjectLoading);

            CancelLoadingProjectCommand = ReactiveCommand.Create(
                () => { },
                OpenFileCommand.IsExecuting);

            
        }

        public ReactiveCommand<Unit, Unit> CancelLoadingProjectCommand { get; }


        public string FileName
        {
            get => _fileName;
            set => this.RaiseAndSetIfChanged(ref _fileName, value);
        }

        public string Header
        {
            get => _header;
            set => this.RaiseAndSetIfChanged(ref _header, value);
        }

        public bool IsProjectLoaded
        {
            get => _isProjectLoaded;
            private set => this.RaiseAndSetIfChanged(ref _isProjectLoaded, value);
        }

        public bool IsProjectLoading => _isProjectLoading?.Value ?? false;

        public bool IsRunning
        {
            get { return _isRunning; }
            private set { this.RaiseAndSetIfChanged(ref _isRunning, value); }
        }

        public ReactiveCommand<Unit, Unit> LoadProjectCommand { get; }

        public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }

        [Import]
        public IPackageSettingsViewModel PackageSettingsViewModel { get; private set; }

        public ReactiveCommand<Unit, Unit> RunAllTestCommand { get; }

        public ITestRunner Runner { get; private set; }

        public ReactiveCommand<Unit, Unit> RunSelectedTestCommand { get; }

        public TestNode SelectedItem
        {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }

        public ProjectState State
        {
            get => _state;
            private set => this.RaiseAndSetIfChanged(ref _state, value);
        }

        public ReactiveCommand<Unit, Unit> StopTestCommand { get; }

        public TestNode Tests { get; private set; }

        public void OnTestEvent(string report)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(report);
            XmlNode xmlNode = doc.FirstChild;

            string id = xmlNode.Attributes["id"]?.Value;
            string name = xmlNode.Attributes["name"]?.Value;

            if (string.IsNullOrEmpty(name) && xmlNode.Name != "test-output")
            {
                // TODO: implement test output
            }

            TestNode testNode = flattenTests.FirstOrDefault(_ => _.Id == id);
            if (testNode == null) return;

            if (reportActions.TryGetValue(xmlNode.Name, out Action<ProjectViewModel, TestNode, XmlNode> nodeAction))
            {
                nodeAction(this, testNode, xmlNode);
            }
        }

        private IEnumerable<TestNode> FlattenTests(TestNode test, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            yield return test;
            foreach (TestNode node in test.Children.SelectMany(_ => FlattenTests(_, ct)))
            {
                yield return node;
            }

            yield break;
        }

        private async Task LoadFile(string file, CancellationToken ct)
        {
            State = ProjectState.Loading;
            await Task.Run(() =>
            {
                try
                {
                    var package = new TestPackage(file);
                    foreach (var entry in PackageSettingsViewModel.GetSettings()
                        .Where(p => p.Value != null)
                        .Where(s => (s.Value is string) == false || string.Equals("Default", s.Value as string, StringComparison.InvariantCultureIgnoreCase) == false))
                    {
                        package.AddSetting(entry.Key, entry.Value);
                    }

                    ct.ThrowIfCancellationRequested();

                    Runner = _testEngine.GetRunner(package);
                    ct.ThrowIfCancellationRequested();

                    XmlNode node = Runner.Explore(TestFilter.Empty);
                    Tests = new TestNode(node);
                    flattenTests = FlattenTests(Tests, ct).ToList();

                    IsProjectLoaded = true;
                }
                catch (Exception e)
                {
                    IsProjectLoaded = false;
                }
                finally
                {
                    State = ProjectState.Loaded;
                }
            }, ct);

            this.RaisePropertyChanged(nameof(Tests));

            return;
        }

        private Task RunAllTestAsync(CancellationToken arg)
        {
            State = ProjectState.Started;
            Runner.RunAsync(this, TestFilter.Empty);
            IsRunning = true;
            return Task.CompletedTask;
        }

        private Task RunSelectedTestAsync(CancellationToken arg)
        {
            State = ProjectState.Started;
            Runner.RunAsync(this, SelectedItem.GetTestFilter());
            IsRunning = true;
            return Task.CompletedTask;
        }

        private Task StopTestAsync()
        {
            Runner.StopRun(false);
            IsRunning = false;
            return Task.CompletedTask;
        }
    }
}