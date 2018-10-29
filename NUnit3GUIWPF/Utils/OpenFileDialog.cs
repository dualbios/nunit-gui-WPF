using System.ComponentModel.Composition;
using System.Windows.Forms;
using NUnit3GUIWPF.Interfaces;

namespace NUnit3GUIWPF.Utils
{
    [Export(typeof(IOpenFileDialog))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.NonShared)]
    public class OpenFileDialog : IOpenFileDialog
    {
        private readonly System.Windows.Forms.OpenFileDialog _ofd;

        public OpenFileDialog()
        {
            _ofd = new System.Windows.Forms.OpenFileDialog
            {
                DefaultExt = "dll",
                CheckFileExists = true,
                Multiselect = true
            };
        }

        public bool ShowDialog()
        {
            return _ofd.ShowDialog() == DialogResult.OK;
        }

        public string[] FileNames => _ofd.FileNames;
    }
}