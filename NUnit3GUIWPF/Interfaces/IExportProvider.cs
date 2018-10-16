using Microsoft.VisualStudio.Composition;

namespace NUnit3GUIWPF.Interfaces
{
    public interface IExportProvider
    {
        ExportProvider Provider { get; }
    }
}