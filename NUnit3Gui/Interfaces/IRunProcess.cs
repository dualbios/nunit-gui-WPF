using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit3Gui.Enums;

namespace NUnit3Gui.Interfaces
{
    public interface IRunProcess
    {
        string Args { get; }

        string AssemblyPath { get; }

        int ExitCode { get; }

        StringBuilder StandardError { get; }

        StringBuilder StandardOutput { get; }

        Task<TestState> Run();

        Task<TestState> Run(CancellationToken ct);
    }
}