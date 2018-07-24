using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NUnit3Gui.Interfaces
{
    public interface IFileItem
    {
        string FileName { get; }

        string FilePath { get; }

        string StringState { get; }

        string Message { get; }

        int TestCount { get; }

        IEnumerable<ITest> Tests { get; }

        Task LoadAsync(IFileLoaderManager fileLoaderManager, CancellationToken ct);
    }
}