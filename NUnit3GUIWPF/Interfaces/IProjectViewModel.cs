using NUnit3GUIWPF.Models;

namespace NUnit3GUIWPF.Interfaces
{
    public interface IProjectViewModel
    {
        int FailedTestCount { get; }

        string Header { get; set; }

        int InconclusiveTestCount { get; }

        bool IsProjectLoaded { get; }

        bool IsProjectLoading { get; }

        bool IsRunning { get; }

        int PassedTestCount { get; }

        int SkippedTestCount { get; }

        ProjectState State { get; }

        int WarningTestCount { get; }
    }
}