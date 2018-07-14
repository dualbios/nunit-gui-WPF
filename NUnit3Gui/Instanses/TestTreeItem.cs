using System.Collections.ObjectModel;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses
{
    public class TestTreeItem
    {
        public TestTreeItem(Test test)
        {
            Test = test;
            Name = test.ClassName;
            Child = new ObservableCollection<TestTreeItem>();
        }

        public TestTreeItem(string @namespace)
        {
            Name = @namespace;
            Child = new ObservableCollection<TestTreeItem>();
        }

        public ObservableCollection<TestTreeItem> Child { get; }

        public string Name { get; private set; }

        public string[] Namespaces { get; private set; } = new string[0];

        public TestTreeItem Parent { get; private set; }

        public ITest Test { get; }
    }
}