using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit3Gui.Enums;
using NUnit3Gui.Instanses;

namespace NUnit3Gui.Interfaces
{
    public interface ITestTreeCollector : IDisposable
    {
        TestTreeCollectorType CollectorType { get; }

        IEnumerable<TestTreeItem> TestTree { get; }

        void AddItem(ITest test);

        Task CreateTree(IEnumerable<ITest> tests);

        IEnumerable<ITest> GetAllTests();
    }
}