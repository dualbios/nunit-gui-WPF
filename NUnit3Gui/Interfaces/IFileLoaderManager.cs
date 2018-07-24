using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NUnit3Gui.Interfaces
{
    public interface IFileLoaderManager
    {
        IEnumerable<IFileItem> LoadFiles(IEnumerable<string> fileNames);
        IEnumerable<IFileItem> ParseFile(string file);

        IFileParser CurrentFileParser { get; }

        Task<IEnumerable<ITest>> ParseFileAsync(string fileName, CancellationToken ct);
        Task RunTestAsync(ITest test, CancellationToken ct);
    }
}