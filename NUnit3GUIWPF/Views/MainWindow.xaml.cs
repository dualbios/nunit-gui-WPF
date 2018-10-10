using System.Windows;
using Microsoft.Win32;

namespace NUnit3GUIWPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectFile_OnClick(object sender, RoutedEventArgs e)
        {
            //FileNameTextBox
            OpenFileDialog ofd = new OpenFileDialog()
            {
                CheckFileExists = true,
                DefaultExt = "*.dll"
            };
            string file = (ofd.ShowDialog(this) == true) ? ofd.FileName : null;
            if (file != null)
            {
                FileNameTextBox.Text = file;
            }
        }
    }
}