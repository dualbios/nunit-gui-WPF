using System.Composition;
using NUnit3GUIWPF.Interfaces;

namespace NUnit3GUIWPF.Factories
{
    [Export(typeof(IFileDialogFactory))]
    public class FileDialogFactory : IFileDialogFactory
    {
        private readonly IExportProvider _exportProvider;

        [ImportingConstructor]
        public FileDialogFactory(
            IExportProvider exportProvider)
        {
            _exportProvider = exportProvider;
        }

        public IOpenFileDialog CreateOpenDialog()
        {
            return _exportProvider.Provider.GetExportedValue<IOpenFileDialog>();
        }
    }
}