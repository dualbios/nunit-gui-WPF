using System.Collections.Generic;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Win32;
using NUnit.Engine;
using NUnit3GUIWPF.Models;
using NUnit3GUIWPF.Utils;
using ReactiveUI;

namespace NUnit3GUIWPF.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, ITestEventListener
    {
        private readonly ObservableAsPropertyHelper<string> _file;
        private ITestRunner _runner;
        private ITestEngine _testEngine;
        private IDictionary<string, ResultNode> _resultIndex = new Dictionary<string, ResultNode>();

        public MainWindowViewModel(ITestEngine testEngine)
        {
            _testEngine = testEngine;
            OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFileAsync);
            RunAllTestCommand = ReactiveCommand.CreateFromTask(RunAllTestAsync);

            _file = OpenFileCommand.ToProperty(this, vm => vm.File);
        }

        public string File => _file?.Value;

        public ReactiveCommand<Unit, string> OpenFileCommand { get; }

        public ReactiveCommand<Unit, Unit> RunAllTestCommand { get; }

        public ITestRunner Runner
        {
            get => _runner;
            private set => _runner = value;
        }

        public TestNode Tests { get; private set; }

        public void OnTestEvent(string report)
        {
            XmlNode xmlNode = XmlHelper.CreateXmlNode(report);

            switch (xmlNode.Name)
            {
                case "start-test":
                    //TestStarting?.Invoke(new TestNodeEventArgs(TestAction.TestStarting, new TestNode(xmlNode)));
                    var rrr = new TestNode(xmlNode);
                    break;

                case "start-suite":
                    //SuiteStarting?.Invoke(new TestNodeEventArgs(TestAction.SuiteStarting, new TestNode(xmlNode)));
                    var ddd = new TestNode(xmlNode);
                    break;

                case "start-run":
                    //RunStarting?.Invoke(new RunStartingEventArgs(xmlNode.GetAttribute("count", -1)));
                    var sss = xmlNode.GetAttribute("count", -1);
                    break;

                case "test-case":
                    ResultNode result = new ResultNode(xmlNode);
                    _resultIndex[result.Id] = result;
                    //TestFinished?.Invoke(new TestResultEventArgs(TestAction.TestFinished, result));
                    break;

                case "test-suite":
                    result = new ResultNode(xmlNode);
                    _resultIndex[result.Id] = result;
                    //SuiteFinished?.Invoke(new TestResultEventArgs(TestAction.SuiteFinished, result));
                    break;

                case "test-run":
                    result = new ResultNode(xmlNode);
                    _resultIndex[result.Id] = result;
                    //RunFinished?.Invoke(new TestResultEventArgs(TestAction.RunFinished, result));
                    break;
            }
        }

        private Task LoadFile(string file)
        {
            var package = new TestPackage(file);
            Runner = _testEngine.GetRunner(package);
            Tests = new TestNode(Runner.Explore(TestFilter.Empty));

            return Task.CompletedTask;
        }

        private Task<string> OpenFileAsync()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

            Task.Run(async () =>
            {
                OpenFileDialog ofd = new OpenFileDialog()
                {
                    CheckFileExists = true,
                    DefaultExt = "*.dll"
                };
                string file = (ofd.ShowDialog() == true) ? ofd.FileName : null;
                if (file != null)
                {
                    await LoadFile(file);
                }

                tcs.SetResult(file);
            });

            return tcs.Task;
        }

        private Task RunAllTestAsync(CancellationToken arg)
        {
            return Task.Run(() =>
            {
                Runner.RunAsync(this, TestFilter.Empty);
            });
        }
    }
}