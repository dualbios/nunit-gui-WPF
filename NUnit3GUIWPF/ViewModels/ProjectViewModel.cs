﻿using System;
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
    [TypeConverter(typeof(ViewModelToViewConverter<ProjectViewModel, ProjectView>))]
    public class ProjectViewModel : ReactiveObject, IProjectViewModel, ITestEventListener
    {
        private string _fileName;
        private bool _isRunning;
        private TestNode _selectedItem;
        private ITestEngine _testEngine;
        private IEnumerable<TestNode> flattenTests = new List<TestNode>();

        private IDictionary<string, Action<TestNode, XmlNode>> reportActions = new Dictionary<string, Action<TestNode, XmlNode>>()
        {
            {"start-test", (node, report) => { node.TestAction = TestState.Starting; }},
            {"start-suite", (node, report) => { node.TestAction = TestState.Starting; }},
            {"start-run", (node, report) => { node.TestAction = TestState.Starting; }},
            {
                "test-case", (node, report) =>
                {
                    node.TestAction = TestState.Finished;
                    node.Duration = report.ParseDuration();
                }
            },
            {
                "test-suite", (node, report) =>
                {
                    node.TestAction = TestState.Finished;
                    node.Duration = report.ParseDuration();
                }
            },
            {
                "test-run", (node, report) =>
                {
                    node.TestAction = TestState.Finished;
                    node.Duration = report.ParseDuration();
                }
            },
        };

        [ImportingConstructor]
        public ProjectViewModel(IUnitTestEngine engine)
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
        }

        public ReactiveCommand<Unit, Unit> CloseProjectCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadProjectCommand { get; }

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

        public IDictionary<string, object> PackageSettings { get; private set; }

        public ReactiveCommand<Unit, Unit> RunAllTestCommand { get; }

        public ITestRunner Runner { get; private set; }

        public ReactiveCommand<Unit, Unit> RunSelectedTestCommand { get; }

        public TestNode SelectedItem
        {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
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

            if (reportActions.TryGetValue(xmlNode.Name, out Action<TestNode, XmlNode> nodeAction))
            {
                nodeAction(testNode, xmlNode);
            }

            if (xmlNode.Name == "test-run")
            {
                Application.Current.Dispatcher.Invoke(() => { IsRunning = false; });
            }
        }

        public void SetProjectFileAsync(string fileName, IDictionary<string, object> packageSettings)
        {
            Application.Current.Dispatcher.Invoke(() => FileName = fileName);
            Task.Run(async () => await LoadFile(fileName, packageSettings));
            //return LoadFile(fileName,
            //    packageSettings);
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

        public bool IsProjectLoading
        {
            get => _isProjectLoading;
            set => this.RaiseAndSetIfChanged(ref _isProjectLoading, value);
        }

        private bool _isProjectLoading;


        private async Task LoadFile(string file, IDictionary<string, object> packageSettings)
        {
            await Task.Run(() =>
            {
                try
                {
                    IsProjectLoading = true;
                    var package = new TestPackage(file);
                    PackageSettings = packageSettings;
                    foreach (var entry in PackageSettings
                        .Where(p => p.Value != null)
                        .Where(s => (s.Value is string) == false || string.Equals("Default", s.Value as string, StringComparison.InvariantCultureIgnoreCase) == false))
                    {
                        package.AddSetting(entry.Key, entry.Value);
                    }

                    Runner = _testEngine.GetRunner(package);
                    XmlNode node = Runner.Explore(TestFilter.Empty);
                    Tests = new TestNode(node);
                    flattenTests = FlattenTests(Tests).ToList();
                }
                catch (Exception e)
                {
                }
                finally
                {
                    IsProjectLoading = false;
                }
            });

            this.RaisePropertyChanged(nameof(Tests));

            return;
        }

        private Task RunAllTestAsync(CancellationToken arg)
        {
            Runner.RunAsync(this, TestFilter.Empty);
            IsRunning = true;
            return Task.CompletedTask;
        }

        private Task RunSelectedTestAsync(CancellationToken arg)
        {
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