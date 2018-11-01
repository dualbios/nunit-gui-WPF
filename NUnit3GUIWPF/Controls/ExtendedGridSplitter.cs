using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NUnit3GUIWPF.Controls
{
    public class ExtendedGridSplitter : GridSplitter
    {
        public static readonly DependencyProperty CollapsedControlTemplateProperty = DependencyProperty.Register(
            nameof(CollapsedControlTemplate), typeof(ControlTemplate), typeof(ExtendedGridSplitter), new PropertyMetadata(default(ControlTemplate)));

        public static readonly DependencyProperty ExpandedControlTemplateProperty = DependencyProperty.Register(
            nameof(ExpandedControlTemplate), typeof(ControlTemplate), typeof(ExtendedGridSplitter), new PropertyMetadata(default(ControlTemplate)));

        private ColumnDefinition columnDefinition;
        private GridLength prevColumnWidth = new GridLength(1, GridUnitType.Star);

        public ExtendedGridSplitter()
        {
            MouseDoubleClick += ExtendedGridSplitter_MouseDoubleClick;
            DragCompleted += ExtendedGridSplitter_DragCompleted;
        }

        public ControlTemplate CollapsedControlTemplate
        {
            get { return (ControlTemplate) GetValue(CollapsedControlTemplateProperty); }
            set { SetValue(CollapsedControlTemplateProperty, value); }
        }

        public ControlTemplate ExpandedControlTemplate
        {
            get { return (ControlTemplate) GetValue(ExpandedControlTemplateProperty); }
            set { SetValue(ExpandedControlTemplateProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            int columnIndex = Grid.GetColumn(this);
            columnDefinition = (Parent as Grid).ColumnDefinitions[columnIndex + 1];

            SetTemplate(columnDefinition.Width.Value);
        }

        

        private void ExtendedGridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SetTemplate(columnDefinition.Width.Value);
        }

        private void SetTemplate(double widthValue)
        {
            this.Template = widthValue== 0 ? CollapsedControlTemplate : ExpandedControlTemplate;
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