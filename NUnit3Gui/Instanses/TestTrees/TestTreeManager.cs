using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using NUnit3Gui.Enums;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.TestTrees.Instanses
{
    [Export(typeof(ITestTreeManager))]
    public class TestTreeManager : ITestTreeManager
    {
        [ImportMany]
        public IEnumerable<ITestTreeCollector> TestTreeCollectors { get; private set; }

        public ITestTreeCollector GetCollector(TestTreeCollectorType type)
        {
            ITestTreeCollector result = TestTreeCollectors.FirstOrDefault(_ => _.CollectorType == type);
            return result ?? throw new ArgumentException($"Not fount collector for type: {type}");
        }

        public IEnumerable<TestTreeCollectorType> GetCollectorTypes()
        {
            return TestTreeCollectors.Select(_ => _.CollectorType);
        }
    }
}