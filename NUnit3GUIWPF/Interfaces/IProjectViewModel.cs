using NUnit3GUIWPF.Models;

namespace NUnit3GUIWPF.Interfaces
{
    public interface IProjectViewModel 
    {
        string FileName { get; }

        string Header { get; set; }

        bool IsRunning { get; }

        ProjectState State { get; }
    }
}