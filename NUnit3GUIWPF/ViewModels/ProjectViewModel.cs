using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
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

        public void OnTestEvent(string report)
        {
            XmlNode xmlNode = XmlHelper.CreateXmlNode(report);
            string logLine = null;

            switch (xmlNode.Name)
            {
                case "start-test":
                    //TestStarting?.Invoke(new TestNodeEventArgs(TestAction.TestStarting, new TestNode(xmlNode)));
                    var rrr = new TestNode(xmlNode);
                    logLine = $"start-test: {rrr.Name}";
                    break;

                case "start-suite":
                    //SuiteStarting?.Invoke(new TestNodeEventArgs(TestAction.SuiteStarting, new TestNode(xmlNode)));
                    var ddd = new TestNode(xmlNode);
                    logLine = $"start-suite: {ddd.Name}";
                    break;

                case "start-run":
                    //RunStarting?.Invoke(new RunStartingEventArgs(xmlNode.GetAttribute("count", -1)));
                    var sss = xmlNode.GetAttribute("count", -1);
                    logLine = $"start-run: {sss.ToString()}";
                    break;

                case "test-case":
                    ResultNode result = new ResultNode(xmlNode);
                    _resultIndex[result.Id] = result;
                    logLine = $"test-case: ({result.Id})\r\n {result.Xml.InnerText}";
                    //TestFinished?.Invoke(new TestResultEventArgs(TestAction.TestFinished, result));
                    break;

                case "test-suite":
                    result = new ResultNode(xmlNode);
                    _resultIndex[result.Id] = result;
                    logLine = $"test-suite: ({result.Id})\r\n {result.Xml.InnerText}";
                    //SuiteFinished?.Invoke(new TestResultEventArgs(TestAction.SuiteFinished, result));
                    break;

                case "test-run":
                    result = new ResultNode(xmlNode);
                    _resultIndex[result.Id] = result;
                    logLine = $"test-run: ({result.Id})\r\n Status : {result.Status}; Duration : {result.Duration}; Test count : {result.TestCount}";
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => IsRunning = false));

                    //RunFinished?.Invoke(new TestResultEventArgs(TestAction.RunFinished, result));
                    break;
            }

            Application.Current.Dispatcher.BeginInvoke(new Action(() => Log.Add(logLine)));
        }

        public Task SetProjectFileAsync(string fileName)
        {
            Application.Current.Dispatcher.Invoke(() => FileName = fileName);
            return LoadFile(fileName);
        }

        private Task LoadFile(string file)
        {
            var package = new TestPackage(file);
            Runner = _testEngine.GetRunner(package);
            Tests = new TestNode(Runner.Explore(TestFilter.Empty));

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