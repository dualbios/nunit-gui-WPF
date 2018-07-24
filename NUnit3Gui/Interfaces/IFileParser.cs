using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NUnit3Gui.Interfaces
{
    public interface IFileParser
    {
        string Name { get; }
        string Alias { get; }

        Task<IEnumerable<ITest>> ParseFileAsync(string fileName, CancellationToken ct);
    }
}