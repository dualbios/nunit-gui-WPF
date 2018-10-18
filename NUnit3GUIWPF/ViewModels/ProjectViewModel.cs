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
using Microsoft.Win32;
using NUnit.Engine;
using NUnit3GUIWPF.Converters;
using NUnit3GUIWPF.Interfaces;
using NUnit3GUIWPF.Models;
using NUnit3GUIWPF.Views;
using ReactiveUI;
using ReactiveUI.Legacy;

namespace NUnit3GUIWPF.ViewModels
{
    [Export(typeof(IProjectViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.NonShared)]
    [TypeConverter(typeof(ViewModelToViewConverter<ProjectViewModel, ProjectView>))]
    public class ProjectViewModel : ReactiveObject, IProjectViewModel, ITestEventListener
    {
        private string _filePath;
        private string _header;
        private bool _isProjectLoaded;
        private ObservableAsPropertyHelper<bool> _isProjectLoading;
        private bool _isRunning;
        private string _selectedFilePath;
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
                    node.TestStatus = report.GetStatus();
                    node.Duration = report.ParseDuration();
                }
            },
            {
                "test-suite", (vm, node, report) =>
                {
                    node.TestAction = TestState.Finished;
                    node.Output = report.InnerText;
                    node.TestStatus = report.GetStatus();
                    node.Duration = report.ParseDuration();
                }
            },
            {
                "test-run", (vm, node, report) =>
                {
                    node.TestAction = TestState.Finished;
                    node.Duration = report.ParseDuration();
                    node.Output = report.InnerText;
                    node.TestStatus = report.GetStatus();
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
                    vm => vm.IsRunning,
                    (p2) =>  !p2.Value));
            StopTestCommand = ReactiveCommand.CreateFromTask(
                StopTestAsync,
                this.WhenAny(vm => vm.IsRunning, p => p.Value == true));

            RunSelectedTestCommand = ReactiveCommand.CreateFromTask(
                RunSelectedTestAsync,
                this.WhenAny(vm => vm.SelectedItem, p => p.Value != null));

            OpenFileCommand = ReactiveCommand.CreateFromObservable(
                () => Observable.StartAsync(ct => LoadFile(FilePathList, ct))
                    .TakeUntil(CancelLoadingProjectCommand),
                    FilePathList.CountChanged.Select(_ => _ > 0));

            _isProjectLoading = OpenFileCommand.IsExecuting.ToProperty(this, vm => vm.IsProjectLoading);

            CancelLoadingProjectCommand = ReactiveCommand.Create(
                () => { },
                OpenFileCommand.IsExecuting);

            AddFileCommand = ReactiveCommand.Create(
                () => OpenAndAddFile());

            RemoveFileCommand = ReactiveCommand.Create<string>(
                f => { FilePathList.Remove(f); },
                this.WhenAny(vm => vm.SelectedFilePath, p => string.IsNullOrEmpty(p.Value) == false));
        }

        public ReactiveCommand<Unit, Unit> AddFileCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelLoadingProjectCommand { get; }

        public int FailedTestCount => flattenTests.Where(_ => _.Type == "TestCase").Count(_ => _.TestStatus == TestStatus.Failed);

        public ReactiveList<string> FilePathList { get; } = new ReactiveList<string>();

        public string Header
        {
            get => _header;
            set => this.RaiseAndSetIfChanged(ref _header, value);
        }

        public int InconclusiveTestCount => flattenTests.Where(_ => _.Type == "TestCase").Count(_ => _.TestStatus == TestStatus.Inconclusive);

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

        public int PassedTestCount => flattenTests.Where(_ => _.Type == "TestCase").Count(_ => _.TestStatus == TestStatus.Passed);

        public ReactiveCommand<string, Unit> RemoveFileCommand { get; }

        public ReactiveCommand<Unit, Unit> RunAllTestCommand { get; }

        public ITestRunner Runner { get; private set; }

        public ReactiveCommand<Unit, Unit> RunSelectedTestCommand { get; }

        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set => this.RaiseAndSetIfChanged(ref _selectedFilePath, value);
        }

        public TestNode SelectedItem
        {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }

        public int SkippedTestCount => flattenTests.Where(_ => _.Type == "TestCase").Count(_ => _.TestStatus == TestStatus.Skipped);

        public ProjectState State
        {
            get => _state;
            private set => this.RaiseAndSetIfChanged(ref _state, value);
        }

        public ReactiveCommand<Unit, Unit> StopTestCommand { get; }

        public TestNode Tests { get; private set; }

        public int WarningTestCount => flattenTests.Where(_ => _.Type == "TestCase").Count(_ => _.TestStatus == TestStatus.Warning);

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

            this.RaisePropertyChanged(nameof(FailedTestCount));
            this.RaisePropertyChanged(nameof(InconclusiveTestCount));
            this.RaisePropertyChanged(nameof(PassedTestCount));
            this.RaisePropertyChanged(nameof(SkippedTestCount));
            this.RaisePropertyChanged(nameof(WarningTestCount));
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

        private async Task LoadFile(IEnumerable<string> files, CancellationToken ct)
        {
            State = ProjectState.Loading;
            await Task.Run(() =>
            {
                try
                {
                    var package = new TestPackage(files.ToList());
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

            this.RaisePropertyChanged(nameof(FailedTestCount));
            this.RaisePropertyChanged(nameof(InconclusiveTestCount));
            this.RaisePropertyChanged(nameof(PassedTestCount));
            this.RaisePropertyChanged(nameof(SkippedTestCount));
            this.RaisePropertyChanged(nameof(WarningTestCount));

            return;
        }

        private void OpenAndAddFile()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                CheckFileExists = true,
                DefaultExt = "*.dll"
            };
            if (ofd.ShowDialog(Application.Current.MainWindow) == true)
            {
                FilePathList.Add(ofd.FileName);
            }
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