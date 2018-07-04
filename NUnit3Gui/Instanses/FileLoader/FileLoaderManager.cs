using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.FileLoader
{
    [Export(typeof(IFileLoaderManager))]
    public class FileLoaderManager : IFileLoaderManager
    {
        [ImportMany(typeof(IFileLoader))]
        public IEnumerable<IFileLoader> FileLoaders { get; private set; }

        public Task<IFileItem> LoadFile(string filePath)
        {
            IFileLoader fileLoader = FileLoaders.FirstOrDefault(_ => _.Extension == Path.GetExtension(filePath));
            return fileLoader?.LoadAsync(filePath) ?? Task.FromResult(null as IFileItem);
        }
    }
}