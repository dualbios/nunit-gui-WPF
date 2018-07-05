using NUnit.Framework.Interfaces;

namespace NUnit3Gui.Interfaces
{
    public interface ITest
    {
        string AssemblyPath { get; }

        string TestName { get; }

        bool IsRunning { get; set; }

        TestStatus Status { get; set; }
    }
}