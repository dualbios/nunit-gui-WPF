using System.Collections.ObjectModel;

namespace NUnit3Gui.Instanses
{
    public class TestTreeItem
    {
        public TestTreeItem()
        {
            Child = new ObservableCollection<TestTreeItem>();
        }

        public ObservableCollection<TestTreeItem> Child { get; }

        public string Name { get; private set; }

        public string[] Namespaces { get; private set; } = new string[0];

        public TestTreeItem Parent { get; private set; }
    }
}