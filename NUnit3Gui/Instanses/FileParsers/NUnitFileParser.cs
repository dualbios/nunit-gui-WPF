using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
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

        public Task<IEnumerable<ITest>> ParseFileAsync(string fileName, CancellationToken ct)
        {
            var _package = MakeTestPackage(fileName);
            ITestRunner Runner = _testEngine.GetRunner(_package);
            System.Xml.XmlNode result = Runner.Explore(TestFilter.Empty);

            var resultExplore = GetAttribute(result, "result");
            if (resultExplore == "Failed")
                throw new Exception($"Loading failed. {resultExplore}");

            return Task.FromResult(Enumerable.Empty<ITest>());
        }

        private TestPackage MakeTestPackage(string fileName)
        {
            var package = new TestPackage(fileName);

            return package;
        }

        public static string GetAttribute(XmlNode result, string name)
        {
            XmlAttribute attr = result.Attributes[name];

            return attr == null ? null : attr.Value;
        }
    }
}