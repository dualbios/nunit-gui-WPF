using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NUnit.Engine;
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

        public static string GetAttribute(XmlNode result, string name)
        {
            XmlAttribute attr = result.Attributes[name];

            return attr == null ? null : attr.Value;
        }

        public Task<IEnumerable<ITest>> ParseFileAsync(string fileName, CancellationToken ct)
        {
            var _package = MakeTestPackage(fileName);
            ITestRunner Runner = _testEngine.GetRunner(_package);
            XmlNode runnerResult = Runner.Explore(TestFilter.Empty);

            if (runnerResult.Name == "test-run")
            {
                var resultExplore = GetAttribute(runnerResult, "result");
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
                        var runstateValue = GetAttribute(runnerResult, "runstate");
                        if (runstateValue != "Runnable")
                            throw new Exception("Cannot be ran");

                        return Task.FromResult(ParseTestSuit(runnerResult));
                    }
                }
            }

            throw new Exception("Enexpected result.");
        }

        private TestPackage MakeTestPackage(string fileName)
        {
            var package = new TestPackage(fileName);

            return package;
        }

        private IEnumerable<ITest> ParseTestSuit(XmlNode xmlNode)
        {
            if (xmlNode.Name == "test-case")
            {
                yield return new Test(GetAttribute(xmlNode, "fullname"), GetAttribute(xmlNode, "fullname"));
            }

            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                    foreach (var test in ParseTestSuit(node))
                    {
                        yield return test;
                    }
            }
        }
    }
}