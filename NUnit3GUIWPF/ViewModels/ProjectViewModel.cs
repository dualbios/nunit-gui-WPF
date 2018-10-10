using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Microsoft.VisualStudio.Composition;
using NUnit.Engine;
using NUnit3GUIWPF.Converters;
using NUnit3GUIWPF.Interfaces;
using NUnit3GUIWPF.Models;
using NUnit3GUIWPF.Utils;
using NUnit3GUIWPF.Views;
using ReactiveUI;

namespace NUnit3GUIWPF.ViewModels
{
    [Export(typeof(IProjectViewModel))]
    [TypeConverter(typeof(ViewModelToViewConverter<ProjectViewModel, ProjectView>))]
    public class ProjectViewModel : ReactiveObject, IProjectViewModel, ITestEventListener
    {
        private string _fileName;
        private bool _isRunning;
        private ObservableCollection<string> _log = new ObservableCollection<string>();
        private IDictionary<string, ResultNode> _resultIndex = new Dictionary<string, ResultNode>();

        private ITestEngine _testEngine;

        private IEnumerable<TestNode> flattenTests = new List<TestNode>();

        [ImportingConstructor]
        public ProjectViewModel(IUnitTestEngine engine)
        {
            _testEngine = engine.TestEngine;
            RunAllTestCommand = ReactiveCommand.CreateFromTask(RunAllTestAsync,
                this.WhenAny(
                    vm => vm.FileName,
                    vm => vm.IsRunning,
                    (p1, p2) => !string.IsNullOrEmpty(p1.Value) && !p2.Value));
            StopTestCommand = ReactiveCommand.CreateFromTask(StopTestAsync,
                this.WhenAny(vm => vm.IsRunning, p => p.Value == true));
        }

        public string FileName
        {
            get => _fileName;
            set => this.RaiseAndSetIfChanged(ref _fileName, value);
        }

        public bool IsRunning
        {
            get { return _isRunning; }
            private set { this.RaiseAndSetIfChanged(ref _isRunning, value); }
        }

        public ObservableCollection<string> Log
        {
            get => _log;
            set => this.RaiseAndSetIfChanged(ref _log, value);
        }

        public ReactiveCommand<Unit, Unit> RunAllTestCommand { get; }

        public ITestRunner Runner { get; private set; }

        public ReactiveCommand<Unit, Unit> StopTestCommand { get; }

        public TestNode Tests { get; private set; }

        private IDictionary<string, Action<TestNode>> reportActions = new Dictionary<string, Action<TestNode>>()
        {
            { "start-test", node => { node.TestAction = TestAction.TestStarting; }},
            { "start-suite", node => { node.TestAction = TestAction.SuiteStarting; }},
            { "start-run", node => { node.TestAction = TestAction.RunStarting; }},
            { "test-case", node => { node.TestAction = TestAction.TestFinished; }},
            { "test-suite", node => { node.TestAction = TestAction.SuiteFinished; }},
            { "test-run", node => { node.TestAction = TestAction.RunFinished; }},
        };

        public void OnTestEvent(string report)
        {
            XmlNode xmlNode = XmlHelper.CreateXmlNode(report);
            string id = xmlNode.GetAttribute("id");
            string name = xmlNode.GetAttribute("name");

            if (string.IsNullOrEmpty(name) && xmlNode.Name!="test-output")
            {

            }

            if (name == "test-run")
            {
                Application.Current.Dispatcher.Invoke(() => { IsRunning = false; });
            }

            TestNode testNode = flattenTests.FirstOrDefault(_ => _.Id == id);
            if (testNode == null) return;

            if (reportActions.TryGetValue(xmlNode.Name, out Action<TestNode> nodeAction))
            {
                nodeAction(testNode);
            }
        }

        public Task SetProjectFileAsync(string fileName, CancellationToken ct)
        {
            Application.Current.Dispatcher.Invoke(() => FileName = fileName);
            return LoadFile(fileName);
        }

        private IEnumerable<TestNode> FlattenTests(TestNode test)
        {
            yield return test;
            foreach (TestNode child in test.Children)
            {
                foreach (TestNode node in FlattenTests(child))
                {
                    yield return node;
                }
            }

            yield break;
        }

        private Task LoadFile(string file)
        {
            var package = new TestPackage(file);
            Runner = _testEngine.GetRunner(package);
            XmlNode node = Runner.Explore(TestFilter.Empty);
            Tests = new TestNode(node);
            flattenTests = FlattenTests(Tests).ToList();

            this.RaisePropertyChanged(nameof(Tests));

            return Task.CompletedTask;
        }

        private Task RunAllTestAsync(CancellationToken arg)
        {
            Log.Clear();
            _resultIndex.Clear();
            Runner.RunAsync(this, TestFilter.Empty);
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