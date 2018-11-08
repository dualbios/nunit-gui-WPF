using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
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

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}