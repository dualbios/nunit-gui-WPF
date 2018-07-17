using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using NUnit3Gui.Enums;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.TestTrees
{
    [Export(typeof(ITestTreeCollector))]
    public class CategoryCollector : ITestTreeCollector
    {
        private readonly ObservableCollection<TestTreeItem> _testTree = new ObservableCollection<TestTreeItem>();

        public TestTreeCollectorType CollectorType => TestTreeCollectorType.Category;

        public IEnumerable<TestTreeItem> TestTree => _testTree;

        public void AddItem(ITest test)
        {
            _testTree.Add(new TestTreeItem(test));
        }

        public Task CreateTree(IEnumerable<ITest> tests)
        {
            _testTree.Clear();
            foreach (var item in tests)
                _testTree.Add(new TestTreeItem(item));

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _testTree?.Clear();
        }

        public IEnumerable<ITest> GetAllTests()
        {
            return _testTree.Select(_ => _.Test);
        }
    }
}