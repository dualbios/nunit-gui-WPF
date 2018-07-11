using System;
using System.Reactive;
using ReactiveUI;

namespace NUnit3Gui.Interfaces
{
    public interface ITestsViewModel
    {
        IObservable<bool> IsTestRunningObservable { get; }

        ReactiveCommand<Unit, Unit> RunAllTestCommand { get; }

        ReactiveCommand<Unit, Unit> RunSelectedTestCommand { get; }
        ReactiveCommand<Unit, Unit> CancelRunTestCommand { get; }
    }
}