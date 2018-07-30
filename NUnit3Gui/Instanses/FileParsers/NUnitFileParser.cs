using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;
using NUnit.Engine;
using NUnit3Gui.Enums;
using NUnit3Gui.Extensions;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.FileParsers
{
    [Export(typeof(IFileParser))]
    public class NUnitFileParser : IFileParser, ITestEventListener
    {
        private ITestEngine _testEngine;
        private TaskCompletionSource<Unit> tcs;
        private NunitTest _runningTest = null;
        private readonly IFormatProvider parseCulture = CultureInfo.GetCultureInfo("en-us");

        public NUnitFileParser()
        {
            _testEngine = TestEngineActivator.CreateInstance(true);
        }

        public string Alias => "NUnitParser";

        public string Name => "NUnit parser";

        public ITestRunner Runner { get; private set; }

        public Task<IEnumerable<ITest>> ParseFileAsync(string fileName, CancellationToken ct)
        {
            var _package = new TestPackage(fileName);
            Runner = _testEngine.GetRunner(_package);
            XmlNode runnerResult = Runner.Explore(TestFilter.Empty);

            if (runnerResult.Name == "test-run")
            {
                var resultExplore = runnerResult.GetAttribute("result");
                if (resultExplore == "Failed")
                {
                    string message = string.Empty;
                    try
                    {
                        message = runnerResult.SelectSingleNode(@"test-suite/failure/message").InnerText;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    throw new Exception(message);
                }
                else
                {
                    runnerResult = runnerResult.FirstChild;
                    if (runnerResult.Name == "test-suite")
                    {
                        var runstateValue = runnerResult.GetAttribute("runstate");
                        var type = runnerResult.GetAttribute("type");
                        var dllName = runnerResult.GetAttribute("fullname");
                        if (runstateValue != "Runnable")
                            throw new Exception("Cannot be ran");

                        return Task.FromResult(ParseTestSuit(runnerResult, dllName));
                    }
                }
            }

            throw new Exception("Enexpected result.");
        }

        public Task<Unit> RunTestAsync(ITest test, CancellationToken ct)
        {
            return Run(test, ct);
        }

        public void OnTestEvent(string report)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(report);
            var xmlNode = doc.FirstChild;

            switch (xmlNode.Name)
            {
                case "start-test":
                    _runningTest.Status = TestState.Running;
                    break;

                //case "start-suite":
                //    SuiteStarting?.Invoke(new TestNodeEventArgs(TestAction.SuiteStarting, new TestNode(xmlNode)));
                //    break;

                //case "start-run":
                //    RunStarting?.Invoke(new RunStartingEventArgs(xmlNode.GetAttribute("count", -1)));
                //    break;

                case "test-case":
                    _runningTest.Status = GetStatus(xmlNode.GetAttribute("result"));
                    _runningTest.StringStatus = xmlNode.SelectSingleNode(@"failure/message")?.FirstChild?.Value;
                    string duration = xmlNode.GetAttribute("duration");
                    _runningTest.RunningTime = TimeSpan.FromSeconds(double.Parse(duration, parseCulture));
                    //_runningTest = null;
                    //tcs.SetResult(Unit.Default);
                    //tcs = null;
                    break;

                //case "test-suite":
                //    result = new ResultNode(xmlNode);
                //    _resultIndex[result.Id] = result;
                //    SuiteFinished?.Invoke(new TestResultEventArgs(TestAction.SuiteFinished, result));
                //    break;

                case "test-run":
                    _runningTest = null;
                    tcs.SetResult(Unit.Default);
                    tcs = null;
                    break;
            }
        }

        public Task<Unit> Run(ITest test, CancellationToken ct)
        {
            if (tcs != null)
            {
                throw new Exception("previous task does not completed.");
            }

            tcs = new TaskCompletionSource<Unit>();
            Task.Run(() =>
            {
                _runningTest = test as NunitTest;
                var _package = new TestPackage(test.AssemblyPath);

                TestFilter filter = new TestFilter(string.Format("<filter><id>{0}</id></filter>", (test as NunitTest).Id));
                Runner.RunAsync(this, filter);
            }
                , ct);

            return tcs.Task;
        }

        private TestState GetStatus(string state)
        {
            switch (state)
            {
                case "Passed":
                default:
                    return TestState.Passed;

                case "Inconclusive":
                    return TestState.Unrunned;

                case "Failed":
                    return TestState.Failed;

                case "Warning":
                    return TestState.Failed;

                case "Skipped":
                    return TestState.Ignored;
            }
        }

        private IEnumerable<ITest> ParseTestSuit(XmlNode xmlNode, string fileName)
        {
            if (xmlNode.Name == "test-case")
            {
                yield return new NunitTest(xmlNode.GetAttribute("id"), fileName, xmlNode.GetAttribute("fullname"));
            }

            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                foreach (var test in ParseTestSuit(node, fileName))
                {
                    yield return test;
                }
            }
        }
    }
}