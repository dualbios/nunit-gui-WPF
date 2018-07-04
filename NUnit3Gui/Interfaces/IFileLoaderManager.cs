using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace NUnit3Gui.Interfaces
{
    public interface IFileLoaderManager
    {
        IEnumerable<IFileLoader> FileLoaders { get; }

        Task<IFileItem> LoadFile(string filePath);
    }

    
}