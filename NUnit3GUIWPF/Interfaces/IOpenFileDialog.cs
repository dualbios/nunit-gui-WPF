namespace NUnit3GUIWPF.Interfaces
{
    public interface IOpenFileDialog
    {
        bool ShowDialog();
        string[] FileNames { get; }
    }
}