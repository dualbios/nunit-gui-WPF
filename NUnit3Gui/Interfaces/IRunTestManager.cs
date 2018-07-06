using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace NUnit3Gui.Interfaces
{
    public interface IRunTestManager
    {
        Task<Unit> RunTestAsync(ITest test, CancellationToken ct);
    }
}