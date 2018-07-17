using System.Collections.ObjectModel;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses
{
    public class TestTreeItem
    {
        public TestTreeItem(Test test)
        {
            Test = test;
            Name = test.TestName;
            Child = new ObservableCollection<TestTreeItem>();
        }

        public TestTreeItem(string name)
        {
            Name = name;
            Child = new ObservableCollection<TestTreeItem>();
        }

        public ObservableCollection<TestTreeItem> Child { get; }

        public string Name { get; private set; }

        public ITest Test { get; }
    }
}