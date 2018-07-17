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
    public class NamespaceTreeCollector : ITestTreeCollector
    {
        private readonly ObservableCollection<TestTreeItem> _testTree = new ObservableCollection<TestTreeItem>();

        public TestTreeCollectorType CollectorType => TestTreeCollectorType.Namespace;

        public IEnumerable<TestTreeItem> TestTree => _testTree;

        public void AddItem(ITest test)
        {
            this.AddNamespace(test, _testTree);
        }

        public Task CreateTree(IEnumerable<ITest> tests)
        {
            _testTree.Clear();
            foreach (var item in tests)
                this.AddItem(item);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _testTree?.Clear();
        }

        public IEnumerable<ITest> GetAllTests()
        {
            return _testTree.SelectMany(_ => GetAllTests(_));
        }

        private void AddNamespace(ITest x, IList<TestTreeItem> testTree, int level = 0)
        {
            string currentNamespace = x.Namespaces[level];
            TestTreeItem treeItemForAdd = testTree.FirstOrDefault(_ => _.Name == currentNamespace);
            if (treeItemForAdd == null)
            {
                var newTreeItem = new TestTreeItem(currentNamespace);
                AddNamespaceTreeItem(x, newTreeItem, ++level);
                testTree.Add(newTreeItem);
            }
            else
            {
                if (level == x.Namespaces.Length - 1)
                {
                    var newTreeItem = new TestTreeItem(x);
                    treeItemForAdd.Child.Add(newTreeItem);
                }
                else
                    AddNamespace(x, treeItemForAdd.Child, ++level);
            }
        }

        private void AddNamespaceTreeItem(ITest x, TestTreeItem testTree, int level)
        {
            if (level >= x.Namespaces.Length)
            {
                testTree.Child.Add(new TestTreeItem(x));
            }
            else
            {
                var item = new TestTreeItem(x.Namespaces[level]);
                testTree.Child.Add(item);
                AddNamespaceTreeItem(x, item, ++level);
            }
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