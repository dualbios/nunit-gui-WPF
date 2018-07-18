using System;
using NUnit3Gui.Enums;

namespace NUnit3Gui.Interfaces
{
    public interface ITest
    {
        string AssemblyPath { get; }

        string[] Categories { get; }

        string ClassName { get; }

        bool IsRunning { get; set; }

        bool IsSelected { get; set; }

        string[] Namespaces { get; }

        TimeSpan RunningTime { get; set; }

        TestState Status { get; set; }

        string StringStatus { get; set; }

        string TestName { get; }
    }
}