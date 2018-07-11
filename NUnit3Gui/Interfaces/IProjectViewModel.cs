using System;
using System.Reactive;
using ReactiveUI;

namespace NUnit3Gui.Interfaces
{
    public interface IProjectViewModel
    {
        int AssembliesCount { get; }

        ReactiveCommand<Unit, Unit> BrowseAssembliesCommand { get; }

        IReactiveList<IFileItem> LoadedAssemblies { get; }

        int TestCount { get; }

         IReactiveList<ITest> Tests { get; }

        IObservable<bool> HasTests { get; }
        IObservable<bool> IsTestRunningObservable { get; set; }
    }
}