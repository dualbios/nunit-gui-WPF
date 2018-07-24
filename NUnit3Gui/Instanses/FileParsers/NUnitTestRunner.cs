using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NUnit.Engine;
using NUnit3Gui.Enums;
using NUnit3Gui.Extensions;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.FileParsers
{
    public class NUnitTestRunner : ITestEventListener
    {
        private NunitTest _runningTest = null;
        private ITestRunner _runner;
        private TaskCompletionSource<Unit> tcs;

        public NUnitTestRunner(ITestRunner runner)
        {
            _runner = runner;
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
                throw new Exception("previous task doe not completed.");
            }

            tcs = new TaskCompletionSource<Unit>();
            Task.Run(() =>
                {
                    _runningTest = test as NunitTest;
                    var _package = new TestPackage(test.AssemblyPath);
                    
                    TestFilter filter = new TestFilter(string.Format("<filter><id>{0}</id></filter>", (test as NunitTest).Id));
                    _runner.RunAsync(this, filter);
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
    }
}