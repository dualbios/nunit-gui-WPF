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
        private readonly IList<ITest> testList = new List<ITest>();

        public TestTreeCollectorType CollectorType => TestTreeCollectorType.Category;

        public IEnumerable<TestTreeItem> TestTree => _testTree;

        public void AddItem(ITest test)
        {
            foreach (string category in test.Categories)
            {
                TestTreeItem testItem = _testTree.FirstOrDefault(_ => _.Name == category);
                if (testItem == null)
                {
                    testItem = new TestTreeItem(category);
                    _testTree.Add(testItem);
                }

                testItem.Child.Add(new TestTreeItem(test));
            }
            if(testList.Contains(test)==false)
                testList.Add(test);
        }

        public Task CreateTree(IEnumerable<ITest> tests)
        {
            _testTree.Clear();
            testList.Clear();
            foreach (var item in tests)
            {
                AddItem(item);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _testTree?.Clear();
            testList.Clear();
        }

        public IEnumerable<ITest> GetAllTests()
        {
            return testList;
        }
        private IEnumerable<ITest> GetAllTests(TestTreeItem testTreeItem)
        {
            if (testTreeItem.Test != null)
                yield return testTreeItem.Test;

            foreach (var children in testTreeItem.Child)
            {
                foreach (var c in GetAllTests(children))
                {
                    yield return c;
                }
            }
        }
    }
}