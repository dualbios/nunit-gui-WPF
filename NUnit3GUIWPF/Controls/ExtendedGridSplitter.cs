using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NUnit3GUIWPF.Controls
{
    public class ExtendedGridSplitter : GridSplitter
    {
        private ColumnDefinition columnDefinition;
        private GridLength prevColumnWidth = new GridLength(1, GridUnitType.Star);

        public ExtendedGridSplitter()
        {
            MouseDoubleClick += ExtendedGridSplitter_MouseDoubleClick;
            DragCompleted += ExtendedGridSplitter_DragCompleted;

        }

        private void ExtendedGridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (columnDefinition.Width.Value == 0)
            {
                //this.Background = new SolidColorBrush(Colors.Red);
            }
        }

        public override void OnApplyTemplate()
        {
            int columnIndex = Grid.GetColumn(this);
            columnDefinition = (Parent as Grid).ColumnDefinitions[columnIndex + 1];
        }

        private void ExtendedGridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GridLength columnWidth = columnDefinition.Width;
            if (columnWidth.IsStar)
            {
                prevColumnWidth = columnWidth;
                columnDefinition.Width = new GridLength(0);
            }
            else if (columnWidth.IsStar == false && columnWidth.Value == 0)
            {
                columnDefinition.Width = prevColumnWidth;
            }
            else
            {
                prevColumnWidth = columnWidth;
                columnDefinition.Width = new GridLength(0);
            }
        }
    }
}