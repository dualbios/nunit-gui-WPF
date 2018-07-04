using System.Collections.Generic;
using System.Threading.Tasks;

namespace NUnit3Gui.Interfaces
{
    public interface IFileLoaderManager
    {
        IEnumerable<IFileLoader> FileLoaders { get; }
        IFileLoader NotSupportedFileLoader { get; }

        Task<IEnumerable<IFileItem>> LoadFile(string filePath);
    }
}