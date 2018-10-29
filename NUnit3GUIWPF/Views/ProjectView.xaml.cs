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

        private void CollapseAllNodes(object sender, RoutedEventArgs e)
        {
            ExpandAll(TestTreeView, false);
        }

        private void ExpandAll(ItemsControl items, bool expand)
        {
            foreach (object obj in items.Items)
            {
                TreeViewItem treeItem = items as TreeViewItem;
                if (treeItem != null)
                {
                    treeItem.IsExpanded = expand;
                    items.UpdateLayout();
                }

                ItemsControl childControl = items.ItemContainerGenerator.ContainerFromItem(obj) as ItemsControl;
                if (childControl != null)
                {
                    ExpandAll(childControl, expand);
                }
            }
        }

        private void ExpandAllnodes(object sender, RoutedEventArgs e)
        {
            ExpandAll(TestTreeView, true);
        }
    }
}