using System.Reflection;
using System.Windows;
using NUnit3GUIWPF.Controls;

namespace NUnit3GUIWPF.Views
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class AboutWindow : BaseWindow
    {
        public AboutWindow()
        {
            InitializeComponent();

            VersionText.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}