using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NUnit3GUIWPF.Controls
{
    /// <summary>
    /// Interaction logic for TestProgress.xaml
    /// </summary>
    public partial class TestProgress : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(object), typeof(TestProgress), new PropertyMetadata(default(object), ItemsSourceChangedCallback));

        public TestProgress()
        {
            InitializeComponent();
        }

        public object ItemsSource
        {
            get { return (object) GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void ItemsSourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TestProgress control = d as TestProgress;
            if (control != null)
            {
                var collection = e.NewValue as INotifyCollectionChanged;
                if (collection != null)
                {
                    collection.CollectionChanged += control.Items_CollectionChanged;
                }
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ColumnDefinition columnDefinition = new ColumnDefinition {Width = new GridLength(1, GridUnitType.Auto)};
                columnDefinition.Tag = e.NewItems[0];
                ProgressGrid.ColumnDefinitions.Add(columnDefinition);
                Border border = new Border();
                ProgressGrid.Children.Add(border);
                Grid.SetColumn(border, ProgressGrid.ColumnDefinitions.Count - 1);
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var columnDefinition = ProgressGrid.ColumnDefinitions.FirstOrDefault(_ => _.Tag == e.NewItems[0]);
                if (columnDefinition != null)
                {
                    ProgressGrid.ColumnDefinitions.Remove(columnDefinition);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ProgressGrid.ColumnDefinitions.Clear();
            }
        }
    }
}