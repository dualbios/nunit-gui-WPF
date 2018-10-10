﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
    [TypeConverter(typeof(ViewModelToViewConverter<ProjectViewModel, ProjectView>))]
    public class ProjectViewModel : ReactiveObject, IProjectViewModel, ITestEventListener
    {
        private string _fileName;
        private bool _isRunning;
        private ITestEngine _testEngine;
        private IEnumerable<TestNode> flattenTests = new List<TestNode>();

        private IDictionary<string, Action<TestNode, XmlNode>> reportActions = new Dictionary<string, Action<TestNode, XmlNode>>()
        {
            {"start-test", (node, report) => { node.TestAction = TestState.Starting; }},
            {"start-suite", (node, report) => { node.TestAction = TestState.Starting; }},
            {"start-run", (node, report) => { node.TestAction = TestState.Starting; }},
            {"test-case", (node, report) =>
            {
                node.TestAction = TestState.Finished;
                node.Duration = report.ParseDuration();
            }},
            {"test-suite", (node, report) =>
            {
                node.TestAction = TestState.Finished;
                node.Duration =  report.ParseDuration();
            }},
            {"test-run", (node, report) =>
            {
                node.TestAction = TestState.Finished;
                node.Duration =  report.ParseDuration();
            }},
        };

        private IList<TestNode> runningTestNode = new List<TestNode>();
        private DispatcherTimer timer = new DispatcherTimer();

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

        public ReactiveCommand<Unit, Unit> RunAllTestCommand { get; }

        public ITestRunner Runner { get; private set; }

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

            if (reportActions.TryGetValue(xmlNode.Name, out Action<TestNode, XmlNode> nodeAction))
            {
                nodeAction(testNode, xmlNode);
                if (testNode.TestAction == TestState.Starting)
                {
                    runningTestNode.Add(testNode);
                }
                if (testNode.TestAction == TestState.Finished)
                {
                    runningTestNode.Remove(testNode);
                }
            }

            if (xmlNode.Name == "test-run")
            {
                Application.Current.Dispatcher.Invoke(() => { IsRunning = false; });
                timer.Stop();
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
            foreach (TestNode node in test.Children.SelectMany(_ => FlattenTests(_)))
            {
                yield return node;
            }

            yield break;
        }

        private async Task LoadFile(string file)
        {
            await Task.Run(() =>
            {
                var package = new TestPackage(file);
                Runner = _testEngine.GetRunner(package);
                XmlNode node = Runner.Explore(TestFilter.Empty);
                Tests = new TestNode(node);
                flattenTests = FlattenTests(Tests).ToList();
            });

            this.RaisePropertyChanged(nameof(Tests));

            return;
        }

        private Task RunAllTestAsync(CancellationToken arg)
        {
            timer.Stop();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += (s, e) =>
            {
                foreach (TestNode tn in runningTestNode)
                {
                    tn.Duration = tn.Duration.Add(timer.Interval);
                }
            };
            runningTestNode.Clear();
            Runner.RunAsync(this, TestFilter.Empty);
            timer.Start();
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