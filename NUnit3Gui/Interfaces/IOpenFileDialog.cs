using System.Windows.Forms;

namespace NUnit3Gui.Interfaces
{
    public interface IOpenFileDialog
    {
        string FileName { get; set; }

        string Filter { get; set; }

        bool Multiselect { get; set; }

        DialogResult ShowDialog();

        string[] FileNames { get; }
    }
}