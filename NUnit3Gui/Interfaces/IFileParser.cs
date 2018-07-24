using System.Collections.Generic;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace NUnit3Gui.Interfaces
{
    public interface IFileParser
    {
        string Name { get; }
        string Alias { get; }

        Task<IEnumerable<ITest>> ParseFileAsync(string fileName, CancellationToken ct);
        Task<Unit> RunTestAsync(ITest test, CancellationToken ct);
    }
}