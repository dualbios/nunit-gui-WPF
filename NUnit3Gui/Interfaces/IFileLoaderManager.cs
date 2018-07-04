using System.Collections.Generic;
using System.Threading.Tasks;

namespace NUnit3Gui.Interfaces
{
    public interface IFileLoaderManager
    {
        IEnumerable<IFileItem> LoadFiles(IEnumerable<string> fileNames);
    }
}