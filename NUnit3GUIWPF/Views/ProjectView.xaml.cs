using System.Windows;
using System.Windows.Controls;

namespace NUnit3GUIWPF.Views
{
    /// <summary>
    /// Interaction logic for ProjectView.xaml
    /// </summary>
    public partial class ProjectView : UserControl
    {
        public ProjectView()
        {
            InitializeComponent();
        }

        private void CollapseAll_OnClick(object sender, RoutedEventArgs e)
        {
            TreeViewItem rootElement = (TreeViewItem)TestTreeView.ItemContainerGenerator.ContainerFromItem(TestTreeView.Items.GetItemAt(0));
            ExpandChildren(rootElement, TestTreeView.ItemContainerGenerator, false);
        }

        private void ExpandAll_OnClick(object sender, RoutedEventArgs e)
        {
            TreeViewItem rootElement = (TreeViewItem)TestTreeView.ItemContainerGenerator.ContainerFromItem(TestTreeView.Items.GetItemAt(0));
            rootElement.IsExpanded = true;
            ExpandChildren(rootElement, TestTreeView.ItemContainerGenerator, true);
        }

        private void ExpandChildren(ItemsControl rootElement, ItemContainerGenerator parentContainerGenerator, bool expand)
        {
            (rootElement as TreeViewItem).IsExpanded = expand;
            foreach (object item in rootElement.Items)
            {
                ItemsControl itemElement = (ItemsControl)rootElement.ItemContainerGenerator.ContainerFromItem(item);
                if (itemElement != null)
                {
                    ExpandChildren(itemElement, rootElement.ItemContainerGenerator, expand);
                }
            }
        }
    }
}