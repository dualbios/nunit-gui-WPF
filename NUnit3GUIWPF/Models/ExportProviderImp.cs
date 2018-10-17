using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Composition;
using NUnit3GUIWPF.Interfaces;

namespace NUnit3GUIWPF.Models
{
    [Export(typeof(IExportProvider))]
    sealed public class ExportProviderImp : IExportProvider
    {
        public ExportProvider Provider { get; private set; }
    }
}