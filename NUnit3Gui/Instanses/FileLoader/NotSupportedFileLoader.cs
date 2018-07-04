using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Threading.Tasks;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.FileLoader
{
    [Export(typeof(IFileLoader))]
    [Export(typeof(IFileLoaderNotSupported))]
    internal class NotSupportedFileLoader : IFileLoader, IFileLoaderNotSupported
    {
        public string Extension => string.Empty;

        public Task<IEnumerable<IFileItem>> LoadAsync(string filePath)
        {
            var fileItem = new FileItem(filePath) { StringState = "(not supported)" };
            return Task.FromResult(new IFileItem[] { fileItem }.AsEnumerable());
        }
    }
}