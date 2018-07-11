using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Windows.Forms;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses
{
    [Export(typeof(IOpenFileDialog))]
    public class OpenFileDialog : IOpenFileDialog
    {
        public string FileName { get; set; }

        public string[] FileNames { get; private set; } = new string[0];

        public string Filter { get; set; }

        public bool Multiselect { get; set; }

        public DialogResult ShowDialog()
        {
            var dialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = Filter,
                Multiselect = Multiselect,
                FileName = FileName
            };
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
                FileNames = dialog.FileNames;

            return result;
        }
    }
}