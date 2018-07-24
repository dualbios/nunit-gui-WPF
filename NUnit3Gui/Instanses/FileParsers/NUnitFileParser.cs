using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NUnit.Engine;
using NUnit3Gui.Extensions;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.FileParsers
{
    [Export(typeof(IFileParser))]
    public class NUnitFileParser : IFileParser
    {
        private ITestEngine _testEngine;

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
            var runner = new NUnitTestRunner(Runner);
            return runner.Run(test, ct);
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