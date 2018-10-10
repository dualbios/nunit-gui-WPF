using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void ExpandAll_OnClick(object sender, RoutedEventArgs e)
        {
            TreeViewItem rootElement = (TreeViewItem) TestTreeView.ItemContainerGenerator.ContainerFromItem(TestTreeView.Items.GetItemAt(0));
            rootElement.IsExpanded = true;
            ExpandChildren(rootElement, true);
        }

        private void ExpandChildren(ItemsControl rootElement, bool expand)
        {
            (rootElement as TreeViewItem).IsExpanded = expand;
            foreach (object item in rootElement.Items)
            {
                ItemsControl itemElement = (ItemsControl) TestTreeView.ItemContainerGenerator.ContainerFromItem(item);
                if (itemElement != null)
                {
                    ExpandChildren(itemElement, expand);
                }
            }
        }

        private void CollapseAll_OnClick(object sender, RoutedEventArgs e)
        {
            TreeViewItem rootElement = (TreeViewItem) TestTreeView.ItemContainerGenerator.ContainerFromItem(TestTreeView.Items.GetItemAt(0));
            ExpandChildren(rootElement, false);
        }
    }
}