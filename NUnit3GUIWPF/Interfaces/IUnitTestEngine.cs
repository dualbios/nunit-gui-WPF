using NUnit.Engine;

namespace NUnit3GUIWPF.Interfaces
{
    public interface IUnitTestEngine
    {
        ITestEngine TestEngine { get; }
    }
}