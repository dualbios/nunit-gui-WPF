using System;
using ReactiveUI;

namespace NUnit3Gui.Interfaces
{
    public interface ITestsViewModel
    {
        IObservable<bool> IsTestRunningObservable { get; }
        IReactiveList<ITest> Tests { get; }
        IObservable<bool> HasTests { get; }
    }
}